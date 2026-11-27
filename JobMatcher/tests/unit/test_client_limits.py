import asyncio
from datetime import UTC, datetime, timedelta

import pytest

from jobmatcher.budget.limits import (
    ChatGate,
    ClientLimiter,
    DuplicateGuard,
    MemoryRateStore,
    Rate,
    RateLimited,
    RequestLimiter,
    ServerBusy,
    StreamSlots,
    window_start,
)
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
    store = MemoryRateStore()
    limiter = ClientLimiter(Tokens(), 5, daily_tokens=1000, now=clock, store=store)
    await limiter.check("a")
    clock.current = START + timedelta(minutes=2)
    await limiter.check("b")
    assert len(store) == 1


CV_RATES = {"cv": [Rate(2, timedelta(minutes=1)), Rate(3, timedelta(days=1))]}


async def test_bucket_needs_every_rate_to_admit() -> None:
    clock = Clock()
    clock.current = datetime(2026, 11, 26, 8, 0, tzinfo=UTC)
    limiter = RequestLimiter(MemoryRateStore(), CV_RATES, now=clock)
    await limiter.check("cv", "a")
    await limiter.check("cv", "a")
    with pytest.raises(RateLimited) as exc:
        await limiter.check("cv", "a")
    assert exc.value.kind == "rate_cv"
    assert exc.value.retry_after_s == 60
    clock.current += timedelta(minutes=1)
    await limiter.check("cv", "a")
    clock.current += timedelta(minutes=1)
    with pytest.raises(RateLimited) as exc:
        await limiter.check("cv", "a")
    assert exc.value.retry_after_s == 24 * 3600 - 120


async def test_hit_refused_by_one_rate_is_not_counted_by_the_others() -> None:
    clock = Clock()
    rates = {"cv": [Rate(5, timedelta(minutes=1)), Rate(1, timedelta(days=1))]}
    limiter = RequestLimiter(MemoryRateStore(), rates, now=clock)
    await limiter.check("cv", "a")
    for _ in range(5):
        with pytest.raises(RateLimited):
            await limiter.check("cv", "a")
    await limiter.check("cv", "b")


async def test_buckets_and_unknown_buckets_are_independent() -> None:
    limiter = RequestLimiter(MemoryRateStore(), CV_RATES, now=lambda: START)
    await limiter.check("cv", "a")
    await limiter.check("cv", "a")
    for _ in range(10):
        await limiter.check("search", "a")


async def test_cached_chat_turn_counts_only_against_the_request_rate() -> None:
    class NoBudget:
        async def check(self, purpose: str) -> None:
            raise BudgetExceeded("chat", 40.0, 40.0)

    limiter = ClientLimiter(Tokens(used=10**9), 1, daily_tokens=1000, now=lambda: START)
    gate = ChatGate(limiter, NoBudget())
    await gate.admit_cached("a")
    with pytest.raises(ClientLimited):
        await gate.admit_cached("a")


def test_window_start_aligns_days_to_utc_midnight() -> None:
    assert window_start(START, timedelta(days=1)) == datetime(2026, 11, 26, tzinfo=UTC)
    assert window_start(START, timedelta(minutes=1)) == START


class Ticks:
    def __init__(self) -> None:
        self.now = 0.0

    def __call__(self) -> float:
        return self.now


def test_stream_slots_cap_per_client_and_in_total() -> None:
    slots = StreamSlots(total=3, per_client=2, hold_s=100, clock=Ticks())
    a1, _ = slots.acquire("a"), slots.acquire("a")
    with pytest.raises(RateLimited) as exc:
        slots.acquire("a")
    assert not isinstance(exc.value, ServerBusy)
    slots.acquire("b")
    with pytest.raises(ServerBusy):
        slots.acquire("c")
    a1.release()
    a1.release()
    slots.acquire("c")
    assert len(slots) == 3


def test_stream_slot_never_released_is_reclaimed_after_hold() -> None:
    clock = Ticks()
    slots = StreamSlots(total=1, per_client=1, hold_s=100, clock=clock)
    slots.acquire("a")
    clock.now = 100
    slots.acquire("a")


def test_duplicate_message_is_refused_within_the_window() -> None:
    clock = Ticks()
    guard = DuplicateGuard(window_s=10, clock=clock)
    guard.check("a", "hello")
    guard.check("b", "hello")
    guard.check("a", "other")
    clock.now = 4
    with pytest.raises(RateLimited) as exc:
        guard.check("a", "hello")
    assert (exc.value.kind, exc.value.retry_after_s) == ("duplicate", 6)
    clock.now = 10
    guard.check("a", "hello")
