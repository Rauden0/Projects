import asyncio
import logging

import aio_pika
import httpx

from jobmatcher import messaging
from jobmatcher.config import configure_logging, get_settings
from jobmatcher.db import repository
from jobmatcher.db.session import get_sessionmaker
from jobmatcher.sources import ADAPTERS, SourceConfig
from jobmatcher.sources.base import load_sources

log = logging.getLogger(__name__)


async def crawl_source(
    config: SourceConfig, client: httpx.AsyncClient, exchange: aio_pika.abc.AbstractExchange
) -> None:
    adapter = ADAPTERS[config.kind](config, client)
    seen: set[str] = set()
    async for posting in adapter.list_postings():
        seen.add(posting.external_id)
        await exchange.publish(
            messaging.message(posting.model_dump_json().encode()),
            routing_key=messaging.DISCOVERED,
        )

    # Only a complete, successful listing may close jobs; a failed request raises before this.
    async with get_sessionmaker()() as session, session.begin():
        source_id = await repository.get_or_create_source(session, config.kind, config.company)
        closed = await repository.close_unseen_jobs(session, source_id, seen)
    log.info("%s/%s: %d postings, %d closed", config.kind, config.company, len(seen), closed)


async def crawl_once(exchange: aio_pika.abc.AbstractExchange) -> None:
    settings = get_settings()
    sources = load_sources(settings.sources_file)
    limits = httpx.Limits(max_connections=settings.http_concurrency_per_host)
    headers = {"User-Agent": "JobMatcher/0.1 (personal job search; +https://github.com/Rauden0/Projects)"}
    async with httpx.AsyncClient(timeout=30, limits=limits, headers=headers) as client:
        results = await asyncio.gather(
            *(crawl_source(s, client, exchange) for s in sources), return_exceptions=True
        )
    for source, result in zip(sources, results, strict=True):
        if isinstance(result, Exception):
            log.error("crawl of %s/%s failed: %r", source.kind, source.company, result)


async def main() -> None:
    configure_logging()
    settings = get_settings()
    connection = await aio_pika.connect_robust(settings.amqp_url)
    async with connection:
        channel = await connection.channel(publisher_confirms=True)
        exchange = await messaging.declare_topology(channel)
        while True:
            await crawl_once(exchange)
            await asyncio.sleep(settings.crawl_interval_s)


if __name__ == "__main__":
    asyncio.run(main())
