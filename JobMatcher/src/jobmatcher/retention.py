"""Deletes data past its retention period: uploaded CVs, the client address on usage rows,
published outbox rows, and expired cached chat answers and rate limit windows.

    python -m jobmatcher.retention               # once
    python -m jobmatcher.retention --every 3600  # keep running
"""

import argparse
import asyncio
import logging
from collections.abc import Sequence
from dataclasses import dataclass
from datetime import UTC, datetime, timedelta

from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher.config import configure_logging, get_settings
from jobmatcher.db import cache_store, repository
from jobmatcher.db.session import get_sessionmaker

log = logging.getLogger(__name__)


@dataclass(frozen=True, slots=True)
class Retention:
    cv: timedelta
    usage_client: timedelta
    outbox: timedelta


@dataclass(frozen=True, slots=True)
class Purged:
    cvs: int
    usage_clients: int
    outbox: int
    cache_rows: int = 0


async def purge(
    sessionmaker: async_sessionmaker[AsyncSession], retention: Retention, now: datetime
) -> Purged:
    async with sessionmaker.begin() as session:
        cvs = await repository.delete_cvs_before(session, now - retention.cv)
        clients = await repository.forget_usage_clients_before(
            session, now - retention.usage_client
        )
        outbox = await repository.delete_outbox_published_before(session, now - retention.outbox)
        cache_rows = await cache_store.delete_expired(session, now)
    log.info(
        "retention_purge cvs=%d usage_clients=%d outbox=%d cache_rows=%d",
        cvs,
        clients,
        outbox,
        cache_rows,
        extra={
            "event": "retention_purge",
            "cvs": cvs,
            "usage_clients": clients,
            "outbox": outbox,
            "cache_rows": cache_rows,
        },
    )
    return Purged(cvs, clients, outbox, cache_rows)


async def loop(retention: Retention, every_s: float | None) -> None:
    sessionmaker = get_sessionmaker()
    while True:
        await purge(sessionmaker, retention, datetime.now(UTC))
        if every_s is None:
            return
        await asyncio.sleep(every_s)


def main(argv: Sequence[str] | None = None) -> None:
    configure_logging()
    parser = argparse.ArgumentParser()
    parser.add_argument("--every", type=float, default=None, help="seconds between runs")
    args = parser.parse_args(argv)
    settings = get_settings()
    retention = Retention(
        cv=timedelta(days=settings.cv_retention_days),
        usage_client=timedelta(days=settings.usage_client_retention_days),
        outbox=timedelta(days=settings.outbox_retention_days),
    )
    asyncio.run(loop(retention, args.every))


if __name__ == "__main__":
    main()
