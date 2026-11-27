import json
from typing import Any

import pytest
from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher.db.models import Outbox
from jobmatcher.domain import EVENT_VERSION, EventEnvelope
from jobmatcher.outbox.relay import relay_batch

pytestmark = pytest.mark.integration


class RecordingExchange:
    def __init__(self, fail: bool = False) -> None:
        self.fail = fail
        self.routing_keys: list[str] = []
        self.messages: list[Any] = []
        self.timeouts: list[float | None] = []

    async def publish(
        self,
        message: Any,
        routing_key: str,
        timeout: float | None = None,  # noqa: ASYNC109
    ) -> None:
        if self.fail:
            raise ConnectionError("broker down")
        self.routing_keys.append(routing_key)
        self.messages.append(message)
        self.timeouts.append(timeout)


async def add_events(sm: async_sessionmaker[AsyncSession], *topics: str) -> None:
    async with sm() as session, session.begin():
        session.add_all(Outbox(topic=t, payload={"job_id": i}) for i, t in enumerate(topics))


async def test_relay_publishes_in_order_and_marks_rows(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    await add_events(sessionmaker, "job.created", "job.closed")
    exchange = RecordingExchange()

    assert await relay_batch(sessionmaker, exchange) == 2  # type: ignore[arg-type]
    assert exchange.routing_keys == ["job.created", "job.closed"]
    assert all(t is not None for t in exchange.timeouts)
    assert await relay_batch(sessionmaker, exchange) == 0  # type: ignore[arg-type]


async def test_failed_publish_leaves_rows_unpublished(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    await add_events(sessionmaker, "job.created")

    with pytest.raises(ConnectionError):
        await relay_batch(sessionmaker, RecordingExchange(fail=True))  # type: ignore[arg-type]

    async with sessionmaker() as session:
        pending = (await session.execute(select(Outbox.published_at))).scalars().all()
    assert pending == [None]


async def test_relay_wraps_payload_in_envelope(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    await add_events(sessionmaker, "job.updated")
    exchange = RecordingExchange()

    await relay_batch(sessionmaker, exchange)  # type: ignore[arg-type]

    [message] = exchange.messages
    body = json.loads(message.body)
    event = EventEnvelope[dict[str, Any]].model_validate(body)
    assert (event.event_id, event.type, event.version, event.data) == (
        "1",
        "job.updated",
        EVENT_VERSION,
        {"job_id": 0},
    )
    assert message.message_id == "1" and message.type == "job.updated"
