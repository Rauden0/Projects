import asyncio
import logging
from collections import defaultdict

import aio_pika
import httpx

from jobmatcher import messaging
from jobmatcher.config import configure_logging, get_settings
from jobmatcher.db import repository
from jobmatcher.db.session import get_sessionmaker
from jobmatcher.sources import ADAPTERS, SourceConfig
from jobmatcher.sources.base import load_sources

log = logging.getLogger(__name__)

USER_AGENT = "JobMatcher/0.1 (personal job search; +https://github.com/Rauden0/Projects)"


async def crawl_source(
    config: SourceConfig, client: httpx.AsyncClient, exchange: aio_pika.abc.AbstractExchange
) -> set[str]:
    """Publish every posting of one configured source; return the external ids seen."""
    adapter = ADAPTERS[config.kind](config, client)
    seen: set[str] = set()
    async for posting in adapter.list_postings():
        seen.add(posting.external_id)
        await exchange.publish(
            messaging.message(posting.model_dump_json().encode()),
            routing_key=messaging.DISCOVERED,
        )
    log.info("%s/%s: %d postings", config.kind, config.company, len(seen))
    return seen


async def close_unseen(key: tuple[str, str], seen: set[str]) -> None:
    async with get_sessionmaker()() as session, session.begin():
        source_id = await repository.get_or_create_source(session, *key)
        closed = await repository.close_unseen_jobs(session, source_id, seen)
    log.info("%s/%s: %d closed", *key, closed)


async def crawl_once(exchange: aio_pika.abc.AbstractExchange, kind: str | None = None) -> None:
    settings = get_settings()
    sources = [s for s in load_sources(settings.sources_file) if kind in (None, s.kind)]
    limits = httpx.Limits(max_connections=settings.http_concurrency_per_host)
    headers = {"User-Agent": USER_AGENT}
    async with httpx.AsyncClient(timeout=30, limits=limits, headers=headers) as client:
        results = await asyncio.gather(
            *(crawl_source(s, client, exchange) for s in sources), return_exceptions=True
        )

    # Several searches can feed one stored source (e.g. jobs.cz queries). Jobs may only be
    # closed when every search of that source listed successfully, against their union.
    groups: dict[tuple[str, str], list[set[str] | BaseException]] = defaultdict(list)
    for source, result in zip(sources, results, strict=True):
        if isinstance(result, BaseException):
            log.error("crawl of %s/%s failed: %r", source.kind, source.company, result)
        groups[(source.kind, source.source_key or source.company)].append(result)
    for key, outcomes in groups.items():
        if all(isinstance(o, set) for o in outcomes):
            await close_unseen(key, set().union(*outcomes))  # type: ignore[arg-type]


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
