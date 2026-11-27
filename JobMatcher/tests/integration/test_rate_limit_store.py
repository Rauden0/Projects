import asyncio
from collections.abc import AsyncIterator
from datetime import UTC, datetime, timedelta

import pytest
from sqlalchemy import select, text
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher.budget.limits import PostgresRateStore, Rate, RateLimited, RequestLimiter
from jobmatcher.db.models import RateLimit

NOW = datetime(2026, 11, 27, 10, 0, 30, tzinfo=UTC)
MINUTE = Rate(2, timedelta(minutes=1))


@pytest.fixture
async def store(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> AsyncIterator[PostgresRateStore]:
    async with sessionmaker.begin() as session:
        await session.execute(text("TRUNCATE rate_limit"))
    yield PostgresRateStore(sessionmaker)


async def test_fixed_window_counts_until_the_limit(store: PostgresRateStore) -> None:
    assert await store.take("chat:a", MINUTE, NOW) == 0
    assert await store.take("chat:a", MINUTE, NOW) == 0
    assert await store.take("chat:a", MINUTE, NOW) == 30
    assert await store.take("chat:b", MINUTE, NOW) == 0
    assert await store.take("chat:a", MINUTE, NOW + timedelta(seconds=30)) == 0


async def test_concurrent_hits_never_exceed_the_limit(store: PostgresRateStore) -> None:
    waits = await asyncio.gather(*(store.take("cv:a", MINUTE, NOW) for _ in range(6)))
    assert sorted(waits) == [0, 0, 30, 30, 30, 30]


async def test_give_back_frees_a_hit(store: PostgresRateStore) -> None:
    await store.take("chat:a", MINUTE, NOW)
    await store.take("chat:a", MINUTE, NOW)
    await store.give_back("chat:a", MINUTE, NOW)
    assert await store.take("chat:a", MINUTE, NOW) == 0


async def test_keys_are_hashed_and_rows_expire_with_the_window(
    store: PostgresRateStore, sessionmaker: async_sessionmaker[AsyncSession]
) -> None:
    limiter = RequestLimiter(store, {"cv": [MINUTE]}, now=lambda: NOW)
    await limiter.check("cv", "10.0.0.1")
    await limiter.check("cv", "10.0.0.1")
    with pytest.raises(RateLimited):
        await limiter.check("cv", "10.0.0.1")
    async with sessionmaker() as session:
        [row] = (await session.execute(select(RateLimit))).scalars()
    assert b"10.0.0.1" not in row.key
    assert (row.hits, row.expires_at) == (2, datetime(2026, 11, 27, 10, 1, tzinfo=UTC))
