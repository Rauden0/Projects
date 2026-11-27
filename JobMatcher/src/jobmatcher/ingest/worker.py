import asyncio
import logging

import aio_pika
from aio_pika.abc import AbstractChannel, AbstractIncomingMessage
from aio_pika.exceptions import AMQPError, ChannelInvalidStateError
from pydantic import ValidationError
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher import messaging
from jobmatcher.config import configure_logging, configure_tracing, get_settings
from jobmatcher.db.session import get_sessionmaker
from jobmatcher.domain import RawPosting
from jobmatcher.ingest.extraction import Extractor, HeuristicExtractor, LLMExtractor
from jobmatcher.ingest.indexer import Indexer
from jobmatcher.ingest.listing_sync import ListingSyncConsumer
from jobmatcher.llm import get_llm
from jobmatcher.search.embeddings import get_embedder

log = logging.getLogger(__name__)


class Worker:
    def __init__(self, indexer: Indexer, channel: AbstractChannel) -> None:
        self.indexer = indexer
        self.channel = channel

    async def handle(self, msg: AbstractIncomingMessage) -> None:
        try:
            await self._handle(msg)
        except Exception:  # noqa: BLE001 - consumer boundary: an unsettled message pins a prefetch slot
            log.exception("could not settle message, requeueing")
            await _requeue(msg)

    async def _handle(self, msg: AbstractIncomingMessage) -> None:
        attempt = messaging.attempt_of(msg.headers)
        try:
            body = messaging.decode_body(msg.body, msg.content_encoding)
            posting = RawPosting.model_validate_json(body)
        except (ValidationError, messaging.UndecodableBody) as exc:
            log.error("malformed message, sending to DLQ")
            await self._dead_letter(
                msg, msg.body, msg.content_encoding, attempt, type(exc).__name__
            )
            await msg.ack()
            return

        try:
            result = await self.indexer.index(posting)
            log.info(
                "indexed %s/%s job_id=%s changed=%s",
                posting.company,
                posting.external_id,
                result.job_id,
                result.changed,
            )
        except Exception as exc:  # noqa: BLE001 - consumer boundary: any failure is retried, then DLQ'd
            next_attempt = attempt + 1
            if next_attempt <= len(messaging.RETRY_DELAYS_S):
                log.exception("indexing failed, retry %d scheduled", next_attempt)
                exchange = await self.channel.get_exchange(messaging.RETRY_EXCHANGE)
                retry = messaging.message(body, next_attempt)
                await exchange.publish(retry, routing_key=str(next_attempt))
            else:
                log.exception("indexing failed permanently, sending to DLQ")
                await self._dead_letter(msg, body, None, attempt, type(exc).__name__)
        # Ack only after the retry/DLQ publish succeeded; if that publish raises, the
        # message is requeued instead of lost.
        await msg.ack()

    async def _dead_letter(
        self,
        msg: AbstractIncomingMessage,
        body: bytes,
        encoding: str | None,
        attempt: int,
        error: str,
    ) -> None:
        dead = messaging.message(
            body, attempt, content_encoding=encoding, headers={**msg.headers, "x-error": error}
        )
        await self.channel.default_exchange.publish(dead, routing_key=messaging.DLQ)


async def _requeue(msg: AbstractIncomingMessage) -> None:
    try:
        await msg.nack(requeue=True)
    except (AMQPError, ChannelInvalidStateError, OSError):
        log.exception("could not requeue message; the broker redelivers it when the channel closes")


def build_extractor() -> Extractor:
    llm = get_llm()
    return LLMExtractor(llm) if llm else HeuristicExtractor()


async def start_consumers(
    channel: AbstractChannel,
    indexer: Indexer,
    sessionmaker: async_sessionmaker[AsyncSession],
    prefetch: int,
    *,
    listing_channel: AbstractChannel | None = None,
    listing_prefetch: int = 1,
) -> None:
    await channel.set_qos(prefetch_count=prefetch)
    await messaging.declare_topology(channel)
    queue = await channel.get_queue(messaging.DISCOVERED)
    await queue.consume(Worker(indexer, channel).handle)
    # A listing sync can run for minutes: its own channel and QoS bound how many run at once
    # independently of WORKER_PREFETCH, and a channel error there leaves job.discovered running.
    if listing_channel is None:
        listing_channel = channel
    else:
        await listing_channel.set_qos(prefetch_count=listing_prefetch)
    listings = await listing_channel.get_queue(messaging.SOURCE_CRAWLED_QUEUE)
    await listings.consume(ListingSyncConsumer(sessionmaker).handle)


async def main() -> None:
    configure_logging()
    configure_tracing()
    settings = get_settings()
    indexer = Indexer(
        get_sessionmaker(),
        build_extractor(),
        get_embedder(),
        defer_extraction=settings.extraction_mode == "batch",
    )
    connection = await aio_pika.connect_robust(settings.amqp_url)
    async with connection:
        channel = await connection.channel(publisher_confirms=True)
        await start_consumers(
            channel,
            indexer,
            get_sessionmaker(),
            settings.worker_prefetch,
            listing_channel=await connection.channel(),
            listing_prefetch=settings.listing_prefetch,
        )
        log.info("worker started")
        await asyncio.Future()


if __name__ == "__main__":
    asyncio.run(main())
