from datetime import UTC, datetime

import pytest

from jobmatcher.budget.guard import BudgetGuard, BudgetPolicy
from jobmatcher.budget.ledger import day_start, month_start
from jobmatcher.errors import BudgetExceeded

NOW = datetime(2026, 11, 26, 13, 30, tzinfo=UTC)


class FixedSpend:
    def __init__(self, spent: float) -> None:
        self.spent = spent
        self.since: datetime | None = None

    async def spent_since(self, since: datetime) -> float:
        self.since = since
        return self.spent


def test_period_starts() -> None:
    assert month_start(NOW) == datetime(2026, 11, 1, tzinfo=UTC)
    assert day_start(NOW) == datetime(2026, 11, 26, tzinfo=UTC)


async def test_guard_counts_spend_from_month_start() -> None:
    spend = FixedSpend(1.0)
    await BudgetGuard(spend, BudgetPolicy(40.0), lambda: NOW).check("chat")
    assert spend.since == datetime(2026, 11, 1, tzinfo=UTC)


async def test_extraction_stops_before_chat() -> None:
    guard = BudgetGuard(FixedSpend(25.0), BudgetPolicy(40.0, extraction_share=0.6), lambda: NOW)
    await guard.check("chat")
    with pytest.raises(BudgetExceeded) as exc:
        await guard.check("extraction")
    assert exc.value.limit_usd == pytest.approx(24.0)


async def test_chat_stops_at_monthly_limit() -> None:
    guard = BudgetGuard(FixedSpend(40.0), BudgetPolicy(40.0), lambda: NOW)
    with pytest.raises(BudgetExceeded):
        await guard.check("chat")
