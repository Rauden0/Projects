import asyncio
from datetime import UTC, datetime, timedelta

import pytest

from jobmatcher.budget.limits import ChatGate, ClientLimiter
from jobmatcher.errors import BudgetExceeded, ClientLimited

START = datetime(2026, 11, 26, 23, 59, 0, tzinfo=UTC)


class Tokens:
    def __init__(self, used: int = 0) -> None:
        self.used = used

    async def client_tokens_since(self, client_id: str, since: datetime) -> int:
        return self.used


class Clock:
    def __init__(self) -> None:
        self.current = START

    def __call__(self) -> datetime:
        return self.current


async def test_rate_limit_per_client_with_sliding_window() -> None:
    clock = Clock()
    limiter = ClientLimiter(Tokens(), requests_per_minute=2, daily_tokens=1000, now=clock)
    await limiter.check("a")
    await limiter.check("a")
    await limiter.check("b")
    with pytest.raises(ClientLimited) as exc:
        await limiter.check("a")
    assert exc.value.reason == "too many requests"
    clock.current = START + timedelta(minutes=1)
    await limiter.check("a")


async def test_daily_token_limit_until_midnight() -> None:
    limiter = ClientLimiter(Tokens(used=1000), 10, daily_tokens=1000, now=lambda: START)
    with pytest.raises(ClientLimited) as exc:
        await limiter.check("a")
    assert exc.value.retry_after_s == 60


async def test_gate_checks_budget_before_counting_request() -> None:
    class NoBudget:
        async def check(self, purpose: str) -> None:
            raise BudgetExceeded("chat", 40.0, 40.0)

    limiter = ClientLimiter(Tokens(), requests_per_minute=1, daily_tokens=1000, now=lambda: START)
    with pytest.raises(BudgetExceeded):
        await ChatGate(limiter, NoBudget()).admit("a")
    await limiter.check("a")


class SlowTokens(Tokens):
    def __init__(self) -> None:
        super().__init__()
        self.release = asyncio.Event()

    async def client_tokens_since(self, client_id: str, since: datetime) -> int:
        await self.release.wait()
        return self.used


async def test_concurrent_requests_count_against_the_rate_limit() -> None:
    tokens = SlowTokens()
    limiter = ClientLimiter(tokens, requests_per_minute=2, daily_tokens=1000, now=lambda: START)
    pending = [asyncio.create_task(limiter.check("a")) for _ in range(3)]
    await asyncio.sleep(0)
    tokens.release.set()
    results = await asyncio.gather(*pending, return_exceptions=True)
    assert sum(isinstance(r, ClientLimited) for r in results) == 1


async def test_rejected_by_daily_tokens_does_not_count_as_request() -> None:
    tokens = Tokens(used=1000)
    limiter = ClientLimiter(tokens, requests_per_minute=1, daily_tokens=1000, now=lambda: START)
    with pytest.raises(ClientLimited):
        await limiter.check("a")
    tokens.used = 0
    await limiter.check("a")


async def test_idle_clients_are_evicted() -> None:
    clock = Clock()
    limiter = ClientLimiter(Tokens(), requests_per_minute=5, daily_tokens=1000, now=clock)
    await limiter.check("a")
    clock.current = START + timedelta(minutes=2)
    await limiter.check("b")
    assert set(limiter._requests) == {"b"}
