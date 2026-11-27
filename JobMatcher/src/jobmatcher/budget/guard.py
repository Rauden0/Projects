import time
from collections.abc import Callable
from dataclasses import dataclass
from datetime import datetime
from typing import Literal, Protocol

from jobmatcher.budget.ledger import month_start, utc_now
from jobmatcher.errors import BudgetExceeded

Purpose = Literal["extraction", "chat"]
SPEND_CACHE_TTL_S = 5.0


class SpendSource(Protocol):
    async def spent_since(self, since: datetime) -> float: ...


@dataclass(frozen=True, slots=True)
class BudgetPolicy:
    monthly_usd: float
    extraction_share: float = 0.6

    def limit_for(self, purpose: Purpose) -> float:
        share = self.extraction_share if purpose == "extraction" else 1.0
        return self.monthly_usd * share


class BudgetGuard:
    """Raises `BudgetExceeded` once this month's spend reaches the purpose's limit.

    The month's spend is cached for `ttl_s` per process, so the cap can overshoot by about
    what is spent in one TTL (times the number of processes) before calls stop.
    """

    def __init__(
        self,
        ledger: SpendSource,
        policy: BudgetPolicy,
        now: Callable[[], datetime] = utc_now,
        *,
        ttl_s: float = SPEND_CACHE_TTL_S,
        monotonic: Callable[[], float] = time.monotonic,
    ) -> None:
        self.ledger = ledger
        self.policy = policy
        self.now = now
        self.ttl_s = ttl_s
        self.monotonic = monotonic
        self._cached: tuple[datetime, float, float] | None = None

    async def check(self, purpose: Purpose) -> None:
        spent = await self._spent(month_start(self.now()))
        limit = self.policy.limit_for(purpose)
        if spent >= limit:
            raise BudgetExceeded(purpose, spent, limit)

    async def _spent(self, since: datetime) -> float:
        at = self.monotonic()
        if self._cached is not None:
            window, read_at, spent = self._cached
            if window == since and at - read_at < self.ttl_s:
                return spent
        spent = await self.ledger.spent_since(since)
        self._cached = (since, at, spent)
        return spent
