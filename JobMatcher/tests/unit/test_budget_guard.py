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
        self.reads = 0

    async def spent_since(self, since: datetime) -> float:
        self.since = since
        self.reads += 1
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


async def test_spend_is_cached_for_the_ttl_then_read_again() -> None:
    spend = FixedSpend(10.0)
    clock = [100.0]
    guard = BudgetGuard(
        spend, BudgetPolicy(40.0), lambda: NOW, ttl_s=5.0, monotonic=lambda: clock[0]
    )
    await guard.check("chat")
    spend.spent = 40.0
    clock[0] = 104.9
    await guard.check("chat")
    assert spend.reads == 1

    clock[0] = 105.0
    with pytest.raises(BudgetExceeded):
        await guard.check("chat")
    assert spend.reads == 2


async def test_new_month_is_not_served_from_the_cache() -> None:
    spend = FixedSpend(40.0)
    today = [NOW]
    guard = BudgetGuard(spend, BudgetPolicy(40.0), lambda: today[0], monotonic=lambda: 0.0)
    with pytest.raises(BudgetExceeded):
        await guard.check("chat")
    spend.spent = 0.0
    today[0] = datetime(2026, 12, 1, tzinfo=UTC)
    await guard.check("chat")
    assert spend.since == datetime(2026, 12, 1, tzinfo=UTC)
