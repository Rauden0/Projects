import os
from collections.abc import AsyncIterator
from pathlib import Path

import pytest
from sqlalchemy import text
from sqlalchemy.engine import make_url
from sqlalchemy.exc import SQLAlchemyError
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker, create_async_engine

from jobmatcher.config import get_settings
from jobmatcher.db.session import get_engine, get_sessionmaker

# Integration tests truncate tables, so they never run against the database in .env: they use
# TEST_DATABASE_URL (default: jobmatcher_test on localhost) and refuse any other name.
TEST_DATABASE_URL = os.environ.get(
    "TEST_DATABASE_URL", "postgresql+asyncpg://jobmatcher:jobmatcher@localhost:5432/jobmatcher_test"
)
TEST_AMQP_URL = os.environ.get("TEST_AMQP_URL", "amqp://guest:guest@localhost:5672/")
if not (make_url(TEST_DATABASE_URL).database or "").endswith("_test"):
    raise pytest.UsageError(f"TEST_DATABASE_URL must name a *_test database: {TEST_DATABASE_URL}")
os.environ["DATABASE_URL"] = TEST_DATABASE_URL
os.environ["AMQP_URL"] = TEST_AMQP_URL
for cached in (get_settings, get_engine, get_sessionmaker):
    cached.cache_clear()

HERE = Path(__file__).parent


def pytest_collection_modifyitems(items: list[pytest.Item]) -> None:
    for item in items:
        if HERE in Path(item.path).parents:
            item.add_marker(pytest.mark.integration)


@pytest.fixture
async def sessionmaker() -> AsyncIterator[async_sessionmaker[AsyncSession]]:
    """Fresh, empty tables per test. Expects `alembic upgrade head` against TEST_DATABASE_URL."""
    engine = create_async_engine(TEST_DATABASE_URL)
    try:
        async with engine.begin() as conn:
            await conn.execute(
                text("TRUNCATE outbox, cv, job_chunk, job, source, llm_usage RESTART IDENTITY")
            )
    except (OSError, SQLAlchemyError) as exc:  # pragma: no cover - no database available
        await engine.dispose()
        pytest.skip(f"database not available: {exc}")
    yield async_sessionmaker(engine, expire_on_commit=False)
    await engine.dispose()
