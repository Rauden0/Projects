"""RabbitMQ topology.

    jobs (topic) --job.discovered--> [job.discovered] --> worker
                                          | on failure, attempt n (1..3)
                                          v
    jobs.retry (direct) --n--> [job.discovered.retry.n]  (queue TTL, no consumers)
                                          | TTL expires -> dead-lettered back to
                                          v
                                   jobs / job.discovered
    after the last attempt --> [job.discovered.dlq]

One retry queue per delay keeps TTLs uniform within a queue, which avoids the head-of-line
blocking you get with per-message expiration.
"""

import aio_pika
from aio_pika.abc import AbstractChannel, AbstractExchange

EXCHANGE = "jobs"
RETRY_EXCHANGE = "jobs.retry"
DISCOVERED = "job.discovered"
DLQ = "job.discovered.dlq"
RETRY_DELAYS_S = (30, 300, 1800)


async def declare_topology(channel: AbstractChannel) -> AbstractExchange:
    exchange = await channel.declare_exchange(
        EXCHANGE, aio_pika.ExchangeType.TOPIC, durable=True
    )
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
    return exchange


def message(body: bytes, attempt: int = 0) -> aio_pika.Message:
    return aio_pika.Message(
        body,
        content_type="application/json",
        delivery_mode=aio_pika.DeliveryMode.PERSISTENT,
        headers={"x-attempt": attempt},
    )
