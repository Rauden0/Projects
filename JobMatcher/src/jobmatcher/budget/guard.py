from collections.abc import Callable
from dataclasses import dataclass
from datetime import datetime
from typing import Literal, Protocol

from jobmatcher.budget.ledger import month_start, utc_now

Purpose = Literal["extraction", "chat"]


class SpendSource(Protocol):
    async def spent_since(self, since: datetime) -> float: ...


@dataclass(frozen=True, slots=True)
class BudgetPolicy:
    monthly_usd: float
    extraction_share: float = 0.6

    def limit_for(self, purpose: Purpose) -> float:
        share = self.extraction_share if purpose == "extraction" else 1.0
        return self.monthly_usd * share


class BudgetExceeded(Exception):
    def __init__(self, purpose: Purpose, spent_usd: float, limit_usd: float) -> None:
        super().__init__(f"{purpose} budget exhausted: ${spent_usd:.2f} of ${limit_usd:.2f}")
        self.purpose = purpose
        self.spent_usd = spent_usd
        self.limit_usd = limit_usd


class BudgetGuard:
    def __init__(
        self,
        ledger: SpendSource,
        policy: BudgetPolicy,
        now: Callable[[], datetime] = utc_now,
    ) -> None:
        self.ledger = ledger
        self.policy = policy
        self.now = now

    async def check(self, purpose: Purpose) -> None:
        spent = await self.ledger.spent_since(month_start(self.now()))
        limit = self.policy.limit_for(purpose)
        if spent >= limit:
            raise BudgetExceeded(purpose, spent, limit)
