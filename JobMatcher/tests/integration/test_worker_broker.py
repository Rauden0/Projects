import asyncio
import gzip
import os

import aio_pika
import pytest
from aio_pika.abc import AbstractChannel, AbstractIncomingMessage, AbstractQueue
from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher import messaging
from jobmatcher.db.models import EMBEDDING_DIM, Job
from jobmatcher.domain import RawPosting
from jobmatcher.ingest.extraction import HeuristicExtractor
from jobmatcher.ingest.indexer import Indexer, IndexResult
from jobmatcher.ingest.listing_sync import ListingSyncConsumer
from jobmatcher.ingest.worker import start_consumers
from jobmatcher.search.embeddings import HashingEmbedder

POSTING = RawPosting(
    kind="greenhouse",
    company="acme",
    external_id="42",
    url="https://acme.example/jobs/42",
    title="Python Developer",
    location="Brno",
    description_html="<p>Hybrid in Brno. 3+ years of Python.</p>",
)


class FailingIndexer(Indexer):
    def __init__(self) -> None:
        pass

    async def index(self, posting: RawPosting) -> IndexResult:
        raise RuntimeError("database exploded")


async def publish(channel: AbstractChannel, body: bytes, encoding: str | None = None) -> None:
    exchange = await channel.get_exchange(messaging.EXCHANGE)
    message = aio_pika.Message(body, content_encoding=encoding, headers={"x-attempt": 0})
    await exchange.publish(message, routing_key=messaging.DISCOVERED)


async def wait_for_message(queue: AbstractQueue, timeout_s: float = 10) -> aio_pika.IncomingMessage:
    for _ in range(int(timeout_s * 10)):
        message = await queue.get(no_ack=True, fail=False)
        if message is not None:
            return message  # type: ignore[return-value]
        await asyncio.sleep(0.1)
    raise AssertionError(f"nothing arrived in {queue.name}")


async def test_gzipped_posting_from_the_exchange_is_indexed(
    sessionmaker: async_sessionmaker[AsyncSession], amqp_channel: AbstractChannel
) -> None:
    indexer = Indexer(sessionmaker, HeuristicExtractor(), HashingEmbedder(EMBEDDING_DIM))
    await start_consumers(amqp_channel, indexer, sessionmaker, prefetch=2)

    await publish(amqp_channel, gzip.compress(POSTING.model_dump_json().encode()), "gzip")

    for _ in range(100):
        async with sessionmaker() as session:
            job = (await session.execute(select(Job))).scalar_one_or_none()
        if job is not None:
            break
        await asyncio.sleep(0.1)
    assert job is not None
    assert (job.title, job.remote_policy, job.years_required_min) == (
        "Python Developer",
        "hybrid",
        3,
    )


async def test_failed_indexing_goes_to_the_first_retry_queue(
    sessionmaker: async_sessionmaker[AsyncSession], amqp_channel: AbstractChannel
) -> None:
    await start_consumers(amqp_channel, FailingIndexer(), sessionmaker, prefetch=2)

    await publish(amqp_channel, POSTING.model_dump_json().encode())

    retry = await amqp_channel.get_queue(f"{messaging.DISCOVERED}.retry.1")
    message = await wait_for_message(retry)
    assert message.headers["x-attempt"] == 1
    assert RawPosting.model_validate_json(message.body) == POSTING


async def test_malformed_message_goes_to_the_dlq(
    sessionmaker: async_sessionmaker[AsyncSession], amqp_channel: AbstractChannel
) -> None:
    indexer = Indexer(sessionmaker, HeuristicExtractor(), HashingEmbedder(EMBEDDING_DIM))
    await start_consumers(amqp_channel, indexer, sessionmaker, prefetch=2)

    await publish(amqp_channel, b'{"not": "a posting"}')

    dlq = await amqp_channel.get_queue(messaging.DLQ)
    message = await wait_for_message(dlq)
    assert message.body == b'{"not": "a posting"}'


async def test_listing_syncs_get_their_own_prefetch_and_do_not_block_postings(
    sessionmaker: async_sessionmaker[AsyncSession],
    amqp_channel: AbstractChannel,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    release = asyncio.Event()

    async def stuck(self: ListingSyncConsumer, msg: AbstractIncomingMessage) -> None:
        await release.wait()

    monkeypatch.setattr(ListingSyncConsumer, "handle", stuck)
    indexer = Indexer(sessionmaker, HeuristicExtractor(), HashingEmbedder(EMBEDDING_DIM))
    connection = await aio_pika.connect(
        os.environ.get("TEST_AMQP_URL", "amqp://guest:guest@localhost:5672/jobmatcher_test"),
        timeout=5,
    )
    async with connection:
        listing_channel = await connection.channel()
        await start_consumers(
            amqp_channel, indexer, sessionmaker, prefetch=8, listing_channel=listing_channel
        )
        exchange = await amqp_channel.get_exchange(messaging.EXCHANGE)
        for _ in range(3):
            await exchange.publish(aio_pika.Message(b"{}"), routing_key=messaging.SOURCE_CRAWLED)
        await publish(amqp_channel, POSTING.model_dump_json().encode())

        try:
            job = None
            for _ in range(100):
                async with sessionmaker() as session:
                    job = (await session.execute(select(Job))).scalar_one_or_none()
                if job is not None:
                    break
                await asyncio.sleep(0.1)
            assert job is not None
            listings = await amqp_channel.declare_queue(
                messaging.SOURCE_CRAWLED_QUEUE, durable=True, passive=True
            )
            assert listings.declaration_result.message_count == 2
        finally:
            release.set()
