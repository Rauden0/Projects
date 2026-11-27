import os
from collections.abc import AsyncIterator
from pathlib import Path

import aio_pika
import pytest
from aio_pika.abc import AbstractChannel
from aio_pika.exceptions import AMQPException
from sqlalchemy import text
from sqlalchemy.engine import make_url
from sqlalchemy.exc import SQLAlchemyError
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker, create_async_engine

from jobmatcher import messaging
from jobmatcher.config import get_settings
from jobmatcher.db.session import get_engine, get_sessionmaker

# Integration and e2e tests truncate tables, so they never run against the database in .env:
# they use TEST_DATABASE_URL (default: jobmatcher_test on localhost) and refuse any other name.
# RabbitMQ tests use their own vhost for the same reason.
TEST_DATABASE_URL = os.environ.get(
    "TEST_DATABASE_URL", "postgresql+asyncpg://jobmatcher:jobmatcher@localhost:5432/jobmatcher_test"
)
TEST_AMQP_URL = os.environ.get("TEST_AMQP_URL", "amqp://guest:guest@localhost:5672/jobmatcher_test")
if not (make_url(TEST_DATABASE_URL).database or "").endswith("_test"):
    raise pytest.UsageError(f"TEST_DATABASE_URL must name a *_test database: {TEST_DATABASE_URL}")
os.environ["DATABASE_URL"] = TEST_DATABASE_URL
os.environ["AMQP_URL"] = TEST_AMQP_URL
# Tests install their own in-memory tracer; never export to whatever .env points at.
os.environ["OTEL_EXPORTER_OTLP_ENDPOINT"] = ""
# Session mode with a key made per run, and a cookie the http:// test clients send back.
os.environ["AUTH_MODE"] = "session"
os.environ["SESSION_SECRET"] = ""
os.environ["AUTH_INSECURE_DEV"] = "true"
os.environ["SESSION_COOKIE_SECURE"] = "false"
for cached in (get_settings, get_engine, get_sessionmaker):
    cached.cache_clear()

LEVELS = {Path(__file__).parent / name: name for name in ("integration", "e2e")}


def pytest_collection_modifyitems(items: list[pytest.Item]) -> None:
    for item in items:
        for directory, marker in LEVELS.items():
            if directory in Path(item.path).parents:
                item.add_marker(marker)


@pytest.fixture
async def sessionmaker() -> AsyncIterator[async_sessionmaker[AsyncSession]]:
    """Fresh, empty tables per test. Expects `alembic upgrade head` against TEST_DATABASE_URL."""
    engine = create_async_engine(TEST_DATABASE_URL)
    try:
        async with engine.begin() as conn:
            await conn.execute(
                # CASCADE: tables referencing these (chat_answer_cache) are emptied too.
                text(
                    "TRUNCATE outbox, cv, job_chunk, job, source, llm_usage"
                    " RESTART IDENTITY CASCADE"
                )
            )
    except (OSError, SQLAlchemyError) as exc:  # pragma: no cover - no database available
        await engine.dispose()
        pytest.skip(f"database not available: {exc}")
    yield async_sessionmaker(engine, expire_on_commit=False)
    await engine.dispose()


@pytest.fixture
async def amqp_channel() -> AsyncIterator[AbstractChannel]:
    """A channel on the test vhost with the JobMatcher topology deleted first."""
    try:
        connection = await aio_pika.connect(TEST_AMQP_URL, timeout=5)
    except (OSError, AMQPException, TimeoutError) as exc:  # pragma: no cover
        pytest.skip(f"broker not available: {exc}")
    async with connection:
        channel = await connection.channel(publisher_confirms=True)
        queues = [messaging.DISCOVERED, messaging.DLQ, messaging.SOURCE_CRAWLED_QUEUE]
        queues += [f"{messaging.DISCOVERED}.retry.{n}" for n in (1, 2, 3)]
        for queue in queues:
            await channel.queue_delete(queue)
        yield channel
