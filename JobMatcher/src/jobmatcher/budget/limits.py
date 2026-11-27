from collections import defaultdict, deque
from collections.abc import Callable
from datetime import datetime, timedelta
from typing import Protocol

from jobmatcher.budget.guard import Purpose
from jobmatcher.budget.ledger import day_start, utc_now
from jobmatcher.errors import ClientLimited

WINDOW = timedelta(minutes=1)


class ClientTokens(Protocol):
    async def client_tokens_since(self, client_id: str, since: datetime) -> int: ...


class ClientLimiter:
    def __init__(
        self,
        tokens: ClientTokens,
        requests_per_minute: int,
        daily_tokens: int,
        now: Callable[[], datetime] = utc_now,
    ) -> None:
        self.tokens = tokens
        self.requests_per_minute = requests_per_minute
        self.daily_tokens = daily_tokens
        self.now = now
        self._requests: defaultdict[str, deque[datetime]] = defaultdict(deque)

    async def check(self, client_id: str) -> None:
        now = self.now()
        self._check_rate(client_id, now)
        # Counted before the await, so concurrent requests see each other.
        self._requests[client_id].append(now)
        used = await self.tokens.client_tokens_since(client_id, day_start(now))
        if used >= self.daily_tokens:
            self._forget(client_id, now)
            tomorrow = day_start(now) + timedelta(days=1)
            raise ClientLimited("daily token limit reached", int((tomorrow - now).total_seconds()))

    def _check_rate(self, client_id: str, now: datetime) -> None:
        self._prune(now)
        recent = self._requests.get(client_id)
        if recent is not None and len(recent) >= self.requests_per_minute:
            retry_after = WINDOW - (now - recent[0])
            raise ClientLimited("too many requests", max(1, int(retry_after.total_seconds())))

    def _prune(self, now: datetime) -> None:
        for client_id in list(self._requests):
            recent = self._requests[client_id]
            while recent and now - recent[0] >= WINDOW:
                recent.popleft()
            if not recent:
                del self._requests[client_id]

    def _forget(self, client_id: str, at: datetime) -> None:
        recent = self._requests.get(client_id)
        if recent is None or at not in recent:
            return
        recent.remove(at)
        if not recent:
            del self._requests[client_id]


class BudgetCheck(Protocol):
    async def check(self, purpose: Purpose) -> None: ...


class ChatGate:
    def __init__(self, limiter: ClientLimiter, budget: BudgetCheck) -> None:
        self.limiter = limiter
        self.budget = budget

    async def admit(self, client_id: str) -> None:
        await self.budget.check("chat")
        await self.limiter.check(client_id)
