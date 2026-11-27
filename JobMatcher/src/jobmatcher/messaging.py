"""RabbitMQ topology.

    jobs (topic) --job.discovered--> [job.discovered] --> worker
                                          | on failure, attempt n (1..3)
                                          v
    jobs.retry (direct) --n--> [job.discovered.retry.n]  (queue TTL, no consumers)
                                          | TTL expires -> dead-lettered back to
                                          v
                                   jobs / job.discovered
    after the last attempt --> [job.discovered.dlq]

    jobs (topic) --source.crawled--> [job.source_crawled] --> worker (listing sync)

One retry queue per delay keeps TTLs uniform within a queue, which avoids the head-of-line
blocking you get with per-message expiration.
"""

import gzip
import zlib
from collections.abc import Mapping

import aio_pika
from aio_pika.abc import AbstractChannel, AbstractExchange

EXCHANGE = "jobs"
RETRY_EXCHANGE = "jobs.retry"
DISCOVERED = "job.discovered"
DLQ = "job.discovered.dlq"
SOURCE_CRAWLED = "source.crawled"
SOURCE_CRAWLED_QUEUE = "job.source_crawled"
RETRY_DELAYS_S = (30, 300, 1800)


async def declare_topology(channel: AbstractChannel) -> AbstractExchange:
    exchange = await channel.declare_exchange(EXCHANGE, aio_pika.ExchangeType.TOPIC, durable=True)
    retry_exchange = await channel.declare_exchange(
        RETRY_EXCHANGE, aio_pika.ExchangeType.DIRECT, durable=True
    )

    queue = await channel.declare_queue(DISCOVERED, durable=True)
    await queue.bind(exchange, routing_key=DISCOVERED)

    for attempt, delay in enumerate(RETRY_DELAYS_S, start=1):
        retry_queue = await channel.declare_queue(
            f"{DISCOVERED}.retry.{attempt}",
            durable=True,
            arguments={
                "x-message-ttl": delay * 1000,
                "x-dead-letter-exchange": EXCHANGE,
                "x-dead-letter-routing-key": DISCOVERED,
            },
        )
        await retry_queue.bind(retry_exchange, routing_key=str(attempt))

    await channel.declare_queue(DLQ, durable=True)

    listings = await channel.declare_queue(SOURCE_CRAWLED_QUEUE, durable=True)
    await listings.bind(exchange, routing_key=SOURCE_CRAWLED)
    return exchange


def message(body: bytes, attempt: int = 0) -> aio_pika.Message:
    return aio_pika.Message(
        body,
        content_type="application/json",
        delivery_mode=aio_pika.DeliveryMode.PERSISTENT,
        headers={"x-attempt": attempt},
    )


def attempt_of(headers: Mapping[str, object] | None) -> int:
    value = (headers or {}).get("x-attempt", 0)
    return value if isinstance(value, int) else int(str(value)) if str(value).isdigit() else 0


class UndecodableBody(ValueError):
    pass


def decode_body(body: bytes, encoding: str | None) -> bytes:
    if encoding in (None, "", "identity"):
        return body
    if encoding != "gzip":
        raise UndecodableBody(f"unsupported content encoding {encoding}")
    try:
        return gzip.decompress(body)
    except (OSError, EOFError, zlib.error) as exc:
        raise UndecodableBody(str(exc)) from exc
