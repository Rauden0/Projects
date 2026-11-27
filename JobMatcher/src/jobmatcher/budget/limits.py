"""Abuse limits: request rates per named bucket and client, the chat's daily tokens per client,
concurrent chat streams and repeated messages. See docs/adr/0004."""

import hashlib
import itertools
import logging
import time
from collections import deque
from collections.abc import Callable, Mapping, Sequence
from dataclasses import dataclass
from datetime import UTC, datetime, timedelta
from typing import Protocol

from sqlalchemy import text
from sqlalchemy.exc import SQLAlchemyError
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher.budget.guard import Purpose
from jobmatcher.budget.ledger import day_start, utc_now
from jobmatcher.caching import TtlCache
from jobmatcher.errors import ClientLimited

log = logging.getLogger(__name__)

WINDOW = timedelta(minutes=1)
DAY = timedelta(days=1)
PRUNE_EVERY_S = 10.0


class RateLimited(ClientLimited):
    """A ClientLimited that names the guard which refused, as a bounded metric label."""

    def __init__(self, kind: str, reason: str, retry_after_s: int) -> None:
        super().__init__(reason, retry_after_s)
        self.kind = kind


class ServerBusy(RateLimited):
    """Refused because of everyone's load, not this client's: 503 instead of 429."""


@dataclass(frozen=True, slots=True)
class Rate:
    hits: int
    window: timedelta


def _seconds(delta: timedelta) -> int:
    return max(1, int(delta.total_seconds() + 0.999))


class RateStore(Protocol):
    async def take(self, key: str, rate: Rate, now: datetime) -> int:
        """Counts a hit and returns 0, or returns the seconds until one is allowed."""
        ...

    async def give_back(self, key: str, rate: Rate, at: datetime) -> None: ...


class MemoryRateStore:
    """Sliding windows in this process; each replica counts on its own."""

    def __init__(self) -> None:
        self._hits: dict[str, tuple[timedelta, deque[datetime]]] = {}
        self._pruned_at: datetime | None = None

    def __len__(self) -> int:
        return len(self._hits)

    async def take(self, key: str, rate: Rate, now: datetime) -> int:
        self._prune(now)
        _, recent = self._hits.setdefault(key, (rate.window, deque()))
        while recent and now - recent[0] >= rate.window:
            recent.popleft()
        if len(recent) >= rate.hits:
            return _seconds(rate.window - (now - recent[0]))
        recent.append(now)
        return 0

    async def give_back(self, key: str, rate: Rate, at: datetime) -> None:
        entry = self._hits.get(key)
        if entry is not None and at in entry[1]:
            entry[1].remove(at)

    def _prune(self, now: datetime) -> None:
        if self._pruned_at is not None and (now - self._pruned_at).total_seconds() < PRUNE_EVERY_S:
            return
        self._pruned_at = now
        for key, (window, recent) in list(self._hits.items()):
            while recent and now - recent[0] >= window:
                recent.popleft()
            if not recent:
                del self._hits[key]


_TAKE = text(
    """
    INSERT INTO rate_limit (key, window_start, hits, expires_at)
    VALUES (:key, :window_start, 1, :expires_at)
    ON CONFLICT (key, window_start) DO UPDATE SET hits = rate_limit.hits + 1
    WHERE rate_limit.hits < :limit
    RETURNING hits
    """
)
_GIVE_BACK = text(
    "UPDATE rate_limit SET hits = hits - 1"
    " WHERE key = :key AND window_start = :window_start AND hits > 0"
)
_EPOCH = datetime(1970, 1, 1, tzinfo=UTC)


def window_start(now: datetime, window: timedelta) -> datetime:
    return _EPOCH + (now - _EPOCH) // window * window


class PostgresRateStore:
    """Fixed windows in the `rate_limit` table, shared by every replica; one atomic upsert per
    hit. Keys are hashed, so the table holds no client addresses. A database outage lets
    requests through rather than refusing everyone."""

    def __init__(self, sessionmaker: async_sessionmaker[AsyncSession]) -> None:
        self.sessionmaker = sessionmaker

    async def take(self, key: str, rate: Rate, now: datetime) -> int:
        start = window_start(now, rate.window)
        params = {
            "key": _digest(key),
            "window_start": start,
            "expires_at": start + rate.window,
            "limit": rate.hits,
        }
        try:
            async with self.sessionmaker.begin() as session:
                hits = (await session.execute(_TAKE, params)).scalar_one_or_none()
        except (SQLAlchemyError, OSError) as exc:
            log.warning("rate limit store unavailable, letting the request through: %s", exc)
            return 0
        return 0 if hits is not None else _seconds(start + rate.window - now)

    async def give_back(self, key: str, rate: Rate, at: datetime) -> None:
        params = {"key": _digest(key), "window_start": window_start(at, rate.window)}
        try:
            async with self.sessionmaker.begin() as session:
                await session.execute(_GIVE_BACK, params)
        except (SQLAlchemyError, OSError) as exc:
            log.warning("rate limit store unavailable: %s", exc)


def _digest(key: str) -> bytes:
    return hashlib.sha256(key.encode()).digest()


@dataclass(frozen=True, slots=True)
class Ticket:
    bucket: str
    client_id: str
    at: datetime


class RequestLimiter:
    """Every rate of a bucket must admit the hit (e.g. 5 a minute and 20 a day); a hit refused
    by one rate is not counted by the others."""

    def __init__(
        self,
        store: RateStore,
        rates: Mapping[str, Sequence[Rate]],
        now: Callable[[], datetime] = utc_now,
    ) -> None:
        self.store = store
        self.rates = rates
        self.now = now

    async def check(self, bucket: str, client_id: str) -> Ticket:
        ticket = Ticket(bucket, client_id, self.now())
        taken: list[Rate] = []
        for rate in self.rates.get(bucket, ()):
            wait = await self.store.take(_key(ticket, rate), rate, ticket.at)
            if wait:
                await self._give_back(ticket, taken)
                raise RateLimited(f"rate_{bucket}", "too many requests", wait)
            taken.append(rate)
        return ticket

    async def release(self, ticket: Ticket) -> None:
        await self._give_back(ticket, self.rates.get(ticket.bucket, ()))

    async def _give_back(self, ticket: Ticket, rates: Sequence[Rate]) -> None:
        for rate in rates:
            await self.store.give_back(_key(ticket, rate), rate, ticket.at)


def _key(ticket: Ticket, rate: Rate) -> str:
    return f"{ticket.bucket}:{int(rate.window.total_seconds())}:{ticket.client_id}"


class ClientTokens(Protocol):
    async def client_tokens_since(self, client_id: str, since: datetime) -> int: ...


class ClientLimiter:
    """The chat's limits: requests per minute and LLM tokens per day, per client."""

    def __init__(
        self,
        tokens: ClientTokens,
        requests_per_minute: int,
        daily_tokens: int,
        now: Callable[[], datetime] = utc_now,
        store: RateStore | None = None,
    ) -> None:
        self.tokens = tokens
        self.daily_tokens = daily_tokens
        self.requests = RequestLimiter(
            MemoryRateStore() if store is None else store,
            {"chat": [Rate(requests_per_minute, WINDOW)]},
            now,
        )

    async def check(self, client_id: str) -> None:
        # Counted before the await, so concurrent requests see each other.
        ticket = await self.requests.check("chat", client_id)
        used = await self.tokens.client_tokens_since(client_id, day_start(ticket.at))
        if used >= self.daily_tokens:
            await self.requests.release(ticket)
            tomorrow = day_start(ticket.at) + DAY
            wait = _seconds(tomorrow - ticket.at)
            raise RateLimited("daily_tokens", "daily token limit reached", wait)

    async def check_rate(self, client_id: str) -> None:
        await self.requests.check("chat", client_id)


class BudgetCheck(Protocol):
    async def check(self, purpose: Purpose) -> None: ...


class ChatGate:
    def __init__(self, limiter: ClientLimiter, budget: BudgetCheck) -> None:
        self.limiter = limiter
        self.budget = budget

    async def admit(self, client_id: str) -> None:
        await self.budget.check("chat")
        await self.limiter.check(client_id)

    async def admit_cached(self, client_id: str) -> None:
        """A replayed answer costs no tokens, but still counts as a request."""
        await self.limiter.check_rate(client_id)


class Slot:
    def __init__(self, release: Callable[[], None]) -> None:
        self._release: Callable[[], None] | None = release

    def release(self) -> None:
        if self._release is not None:
            self._release()
            self._release = None


class StreamSlots:
    """Caps concurrent chat streams in total and per client, refusing instead of queueing. A
    slot is reclaimed after `hold_s` even if never released (a stream whose client went away
    before it started), so `hold_s` must exceed the stream timeout."""

    def __init__(
        self,
        total: int,
        per_client: int,
        hold_s: float,
        clock: Callable[[], float] = time.monotonic,
    ) -> None:
        self.total = total
        self.per_client = per_client
        self.hold_s = hold_s
        self.clock = clock
        self._held: dict[int, tuple[str, float]] = {}
        self._ids = itertools.count()

    def __len__(self) -> int:
        return len(self._held)

    def acquire(self, client_id: str) -> Slot:
        now = self.clock()
        self._held = {k: v for k, v in self._held.items() if v[1] > now}
        mine = sum(1 for owner, _ in self._held.values() if owner == client_id)
        if mine >= self.per_client:
            raise RateLimited("concurrent_streams", "too many chats at once", 5)
        if len(self._held) >= self.total:
            raise ServerBusy("server_busy", "the chat is busy, try again shortly", 10)
        slot_id = next(self._ids)
        self._held[slot_id] = (client_id, now + self.hold_s)
        return Slot(lambda: self._free(slot_id))

    def _free(self, slot_id: int) -> None:
        self._held.pop(slot_id, None)


class DuplicateGuard:
    """Refuses a message the same client already sent within `window_s` with the same CV and
    history: a double submit or a script replaying one request."""

    def __init__(
        self,
        window_s: float,
        max_entries: int = 10_000,
        clock: Callable[[], float] = time.monotonic,
    ) -> None:
        self.clock = clock
        self._seen: TtlCache[tuple[str, str], float] = TtlCache(max_entries, window_s, clock)

    def check(self, client_id: str, fingerprint: str) -> None:
        key = (client_id, fingerprint)
        expires = self._seen.get(key)
        if expires is not None:
            wait = max(1, int(expires - self.clock() + 0.999))
            raise RateLimited("duplicate", "the same message was just sent", wait)
        self._seen.put(key, self.clock() + self._seen.ttl_s)
