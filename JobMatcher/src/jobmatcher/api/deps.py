from collections.abc import AsyncIterator, Awaitable, Callable
from datetime import timedelta
from functools import lru_cache
from typing import Annotated

from fastapi import Depends, Request
from sqlalchemy.ext.asyncio import AsyncSession

from jobmatcher.agent.answers import AnswerCache, PostgresAnswerCache
from jobmatcher.agent.session import ChatGuards
from jobmatcher.api.auth import Authenticator, build_authenticator
from jobmatcher.api.captcha import HEADER, Turnstile
from jobmatcher.budget.guard import BudgetGuard, BudgetPolicy
from jobmatcher.budget.ledger import UsageLedger
from jobmatcher.budget.limits import (
    DAY,
    WINDOW,
    ChatGate,
    ClientLimiter,
    DuplicateGuard,
    MemoryRateStore,
    PostgresRateStore,
    Rate,
    RateStore,
    RequestLimiter,
    StreamSlots,
)
from jobmatcher.config import get_settings
from jobmatcher.db.session import get_sessionmaker
from jobmatcher.errors import CsrfRejected, Unauthenticated
from jobmatcher.search.cache import SearchCache
from jobmatcher.search.embeddings import Embedder, get_embedder


async def get_session() -> AsyncIterator[AsyncSession]:
    async with get_sessionmaker()() as session:
        yield session


SessionDep = Annotated[AsyncSession, Depends(get_session)]
EmbedderDep = Annotated[Embedder, Depends(get_embedder)]


@lru_cache
def get_rate_store() -> RateStore:
    if get_settings().rate_limit_backend == "postgres":
        return PostgresRateStore(get_sessionmaker())
    return MemoryRateStore()


@lru_cache
def get_chat_gate() -> ChatGate:
    settings = get_settings()
    ledger = UsageLedger(get_sessionmaker())
    policy = BudgetPolicy(settings.llm_monthly_budget_usd, settings.llm_extraction_budget_share)
    limiter = ClientLimiter(
        ledger,
        settings.chat_requests_per_minute,
        settings.chat_daily_tokens_per_client,
        store=get_rate_store(),
    )
    return ChatGate(limiter, BudgetGuard(ledger, policy))


ChatGateDep = Annotated[ChatGate, Depends(get_chat_gate)]

# A slot outlives a stream that ended without releasing it by this much.
SLOT_GRACE_S = 30.0


@lru_cache
def get_stream_slots() -> StreamSlots:
    s = get_settings()
    hold_s = s.chat_stream_timeout_s + SLOT_GRACE_S
    return StreamSlots(s.chat_max_streams, s.chat_max_streams_per_client, hold_s)


@lru_cache
def get_duplicate_guard() -> DuplicateGuard:
    return DuplicateGuard(get_settings().chat_duplicate_window_s)


@lru_cache
def get_answer_cache() -> AnswerCache | None:
    ttl_s = get_settings().chat_answer_cache_ttl_s
    if ttl_s == 0:
        return None
    return PostgresAnswerCache(get_sessionmaker(), timedelta(seconds=ttl_s))


def get_chat_guards(
    gate: ChatGateDep,
    slots: Annotated[StreamSlots, Depends(get_stream_slots)],
    duplicates: Annotated[DuplicateGuard, Depends(get_duplicate_guard)],
    answers: Annotated[AnswerCache | None, Depends(get_answer_cache)],
) -> ChatGuards:
    return ChatGuards(gate, slots, duplicates, answers, get_settings().chat_stream_timeout_s)


ChatGuardsDep = Annotated[ChatGuards, Depends(get_chat_guards)]


@lru_cache
def get_authenticator() -> Authenticator:
    return build_authenticator(get_settings())


AuthenticatorDep = Annotated[Authenticator, Depends(get_authenticator)]


async def get_principal(request: Request, auth: AuthenticatorDep) -> str:
    principal = await auth.authenticate(request)
    if principal is None:
        raise Unauthenticated("this endpoint needs an Authorization: Bearer token")
    return principal


PrincipalDep = Annotated[str, Depends(get_principal)]


def client_key(request: Request, principal: PrincipalDep, auth: AuthenticatorDep) -> str:
    if auth.limits_by_principal:
        return principal
    return request.client.host if request.client else "anonymous"


ClientKeyDep = Annotated[str, Depends(client_key)]


async def require_search_access(request: Request, auth: AuthenticatorDep) -> None:
    if auth.protects_search:
        await get_principal(request, auth)


@lru_cache
def get_search_cache() -> SearchCache:
    settings = get_settings()
    return SearchCache(settings.search_cache_size, settings.search_cache_ttl_s)


SearchCacheDep = Annotated[SearchCache, Depends(get_search_cache)]


@lru_cache
def get_request_limiter() -> RequestLimiter:
    s = get_settings()
    rates: dict[str, list[Rate]] = {
        "search": [Rate(s.search_requests_per_minute, WINDOW)],
        "cv_upload": [Rate(s.cv_uploads_per_minute, WINDOW), Rate(s.cv_uploads_per_day, DAY)],
        "cv_delete": [Rate(s.cv_deletes_per_minute, WINDOW)],
    }
    return RequestLimiter(get_rate_store(), rates)


RequestLimiterDep = Annotated[RequestLimiter, Depends(get_request_limiter)]


def limit(bucket: str) -> Callable[..., Awaitable[None]]:
    async def check(limiter: RequestLimiterDep, client_id: ClientKeyDep) -> None:
        await limiter.check(bucket, client_id)

    return check


async def limit_search(
    request: Request, auth: AuthenticatorDep, limiter: RequestLimiterDep
) -> None:
    """Search needs no identity, so it is limited by address unless a token names the caller."""
    principal = await auth.authenticate(request) if auth.limits_by_principal else None
    address = request.client.host if request.client else "anonymous"
    await limiter.check("search", principal or address)


UNSAFE_METHODS = frozenset({"POST", "PUT", "PATCH", "DELETE"})


def reject_cross_site(request: Request) -> None:
    """With cookie auth a cross-site page can make the browser send our cookie; SameSite=Lax
    stops that except from sibling subdomains. A form or a no-cors fetch can only send a
    "simple" request: never DELETE and never a JSON content type, which needs a preflight
    this API does not answer."""
    if request.method not in UNSAFE_METHODS:
        return
    if request.headers.get("sec-fetch-site") == "cross-site":
        raise CsrfRejected("cross-site requests are not accepted")
    media_type = request.headers.get("content-type", "").split(";")[0].strip().lower()
    if request.method != "DELETE" and media_type != "application/json":
        raise CsrfRejected("state-changing requests need Content-Type: application/json")


@lru_cache
def get_captcha() -> Turnstile | None:
    secret = get_settings().turnstile_secret.get_secret_value()
    return Turnstile(secret) if secret else None


async def require_captcha(
    request: Request, captcha: Annotated[Turnstile | None, Depends(get_captcha)]
) -> None:
    if captcha is None:
        return
    address = request.client.host if request.client else None
    await captcha.verify(request.headers.get(HEADER, ""), address)
