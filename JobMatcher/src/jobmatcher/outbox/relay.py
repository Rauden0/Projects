"""Publishes outbox rows to RabbitMQ (at-least-once).

Rows are locked with FOR UPDATE SKIP LOCKED, so several relays can run side by side, and are
marked published in the same transaction only after the broker confirmed the publish. If the
relay dies between confirm and commit, the row is published again: consumers must be
idempotent, which they can be by deduplicating on the envelope's event_id (the outbox id).
"""

import asyncio
import logging
from typing import Any

import aio_pika
from aio_pika.abc import AbstractExchange
from sqlalchemy import func, select, update
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher import messaging, metrics
from jobmatcher.config import configure_logging, get_settings
from jobmatcher.db.models import Outbox
from jobmatcher.db.session import get_sessionmaker
from jobmatcher.domain import EventEnvelope

log = logging.getLogger(__name__)

BATCH_SIZE = 100
IDLE_SLEEP_S = 1.0
PUBLISH_TIMEOUT_S = 30.0


async def relay_batch(
    sessionmaker: async_sessionmaker[AsyncSession], exchange: AbstractExchange
) -> int:
    async with sessionmaker() as session, session.begin():
        rows = (
            (
                await session.execute(
                    select(Outbox)
                    .where(Outbox.published_at.is_(None))
                    .order_by(Outbox.id)
                    .limit(BATCH_SIZE)
                    .with_for_update(skip_locked=True)
                )
            )
            .scalars()
            .all()
        )

        for row in rows:
            event = envelope(row)
            await exchange.publish(
                messaging.message(
                    event.model_dump_json().encode(), message_id=event.event_id, type=event.type
                ),
                routing_key=row.topic,
                timeout=PUBLISH_TIMEOUT_S,
            )

        if rows:
            await session.execute(
                update(Outbox)
                .where(Outbox.id.in_([r.id for r in rows]))
                .values(published_at=func.now())
            )
        metrics.recorder.outbox_batch(len(rows), rows[0].created_at if rows else None)
    return len(rows)


def envelope(row: Outbox) -> EventEnvelope[dict[str, Any]]:
    return EventEnvelope(
        event_id=str(row.id), type=row.topic, occurred_at=row.created_at, data=row.payload
    )


async def main() -> None:
    configure_logging()
    metrics.serve_from_env()
    connection = await aio_pika.connect_robust(get_settings().amqp_url)
    async with connection:
        channel = await connection.channel(publisher_confirms=True)
        exchange = await messaging.declare_topology(channel)
        sessionmaker = get_sessionmaker()
        while True:
            published = await relay_batch(sessionmaker, exchange)
            if published:
                log.info("published %d events", published)
            else:
                await asyncio.sleep(IDLE_SLEEP_S)


if __name__ == "__main__":
    asyncio.run(main())
