from collections.abc import AsyncIterator

import pytest
from sqlalchemy import text
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker, create_async_engine

from jobmatcher.config import get_settings


@pytest.fixture
async def sessionmaker() -> AsyncIterator[async_sessionmaker[AsyncSession]]:
    """Fresh, empty schema per test. Expects `alembic upgrade head` to have run (CI does)."""
    engine = create_async_engine(get_settings().database_url)
    try:
        async with engine.begin() as conn:
            await conn.execute(text("TRUNCATE outbox, cv, job_chunk, job, source RESTART IDENTITY"))
    except Exception as exc:  # pragma: no cover - only when no database is available
        await engine.dispose()
        pytest.skip(f"database not available: {exc}")
    yield async_sessionmaker(engine, expire_on_commit=False)
    await engine.dispose()
