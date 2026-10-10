import asyncio
import logging

import aio_pika
from aio_pika.abc import AbstractChannel, AbstractIncomingMessage
from pydantic import ValidationError

from jobmatcher import messaging
from jobmatcher.config import configure_logging, get_settings
from jobmatcher.db.session import get_sessionmaker
from jobmatcher.ingest.extraction import Extractor, HeuristicExtractor, LLMExtractor
from jobmatcher.ingest.indexer import Indexer
from jobmatcher.llm import get_llm
from jobmatcher.search.embeddings import get_embedder
from jobmatcher.sources import RawPosting

log = logging.getLogger(__name__)


class Worker:
    def __init__(self, indexer: Indexer, channel: AbstractChannel) -> None:
        self.indexer = indexer
        self.channel = channel

    async def handle(self, msg: AbstractIncomingMessage) -> None:
        attempt = int((msg.headers or {}).get("x-attempt", 0))
        try:
            posting = RawPosting.model_validate_json(msg.body)
        except ValidationError:
            log.error("malformed message, sending to DLQ")
            await self._publish(messaging.DLQ, msg.body, attempt, default_exchange=True)
            await msg.ack()
            return

        try:
            result = await self.indexer.index(posting)
            log.info(
                "indexed %s/%s job_id=%s changed=%s",
                posting.company, posting.external_id, result.job_id, result.changed,
            )
        except Exception:
            next_attempt = attempt + 1
            if next_attempt <= len(messaging.RETRY_DELAYS_S):
                log.exception("indexing failed, retry %d scheduled", next_attempt)
                await self._publish(str(next_attempt), msg.body, next_attempt)
            else:
                log.exception("indexing failed permanently, sending to DLQ")
                await self._publish(messaging.DLQ, msg.body, attempt, default_exchange=True)
        # Ack only after the retry/DLQ publish succeeded; if that publish raises, the
        # message is redelivered instead of lost.
        await msg.ack()

    async def _publish(
        self, routing_key: str, body: bytes, attempt: int, default_exchange: bool = False
    ) -> None:
        exchange = (
            self.channel.default_exchange
            if default_exchange
            else await self.channel.get_exchange(messaging.RETRY_EXCHANGE)
        )
        await exchange.publish(messaging.message(body, attempt), routing_key=routing_key)


def build_extractor() -> Extractor:
    llm = get_llm()
    return LLMExtractor(llm) if llm else HeuristicExtractor()


async def main() -> None:
    configure_logging()
    settings = get_settings()
    connection = await aio_pika.connect_robust(settings.amqp_url)
    async with connection:
        channel = await connection.channel(publisher_confirms=True)
        await channel.set_qos(prefetch_count=settings.worker_prefetch)
        await messaging.declare_topology(channel)

        indexer = Indexer(get_sessionmaker(), build_extractor(), get_embedder())
        worker = Worker(indexer, channel)
        queue = await channel.get_queue(messaging.DISCOVERED)
        await queue.consume(worker.handle)
        log.info("worker started")
        await asyncio.Future()


if __name__ == "__main__":
    asyncio.run(main())
