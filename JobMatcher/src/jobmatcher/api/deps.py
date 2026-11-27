from collections.abc import AsyncIterator
from functools import lru_cache
from typing import Annotated

from fastapi import Depends, Request
from sqlalchemy.ext.asyncio import AsyncSession

from jobmatcher.api.auth import Authenticator, build_authenticator
from jobmatcher.budget.guard import BudgetGuard, BudgetPolicy
from jobmatcher.budget.ledger import UsageLedger
from jobmatcher.budget.limits import ChatGate, ClientLimiter
from jobmatcher.config import get_settings
from jobmatcher.db.session import get_sessionmaker
from jobmatcher.errors import CsrfRejected, Unauthenticated
from jobmatcher.search.embeddings import Embedder, get_embedder


async def get_session() -> AsyncIterator[AsyncSession]:
    async with get_sessionmaker()() as session:
        yield session


SessionDep = Annotated[AsyncSession, Depends(get_session)]
EmbedderDep = Annotated[Embedder, Depends(get_embedder)]


@lru_cache
def get_chat_gate() -> ChatGate:
    settings = get_settings()
    ledger = UsageLedger(get_sessionmaker())
    policy = BudgetPolicy(settings.llm_monthly_budget_usd, settings.llm_extraction_budget_share)
    limiter = ClientLimiter(
        ledger, settings.chat_requests_per_minute, settings.chat_daily_tokens_per_client
    )
    return ChatGate(limiter, BudgetGuard(ledger, policy))


ChatGateDep = Annotated[ChatGate, Depends(get_chat_gate)]


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
