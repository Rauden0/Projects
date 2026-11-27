import json

import pytest
from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher.db.models import EMBEDDING_DIM, Job, Outbox
from jobmatcher.domain import RawPosting
from jobmatcher.ingest.extraction import HeuristicExtractor
from jobmatcher.ingest.indexer import Indexer
from jobmatcher.ingest.listing_sync import ListingSyncConsumer
from jobmatcher.search.embeddings import HashingEmbedder

pytestmark = pytest.mark.integration


def posting(external_id: str) -> RawPosting:
    return RawPosting(
        kind="greenhouse",
        company="acme",
        external_id=external_id,
        url=f"https://example.com/{external_id}",
        title="Python Developer",
        location="Brno",
        description_html="<p>Python</p>",
    )


def event(ids: list[str], complete: bool = True) -> bytes:
    body = {"kind": "greenhouse", "source": "acme", "external_ids": ids, "complete": complete}
    return json.dumps(body).encode()


async def index(sm: async_sessionmaker[AsyncSession], *ids: str) -> None:
    indexer = Indexer(sm, HeuristicExtractor(), HashingEmbedder(EMBEDDING_DIM))
    for external_id in ids:
        await indexer.index(posting(external_id))


async def statuses(sm: async_sessionmaker[AsyncSession]) -> dict[str, str]:
    async with sm() as session:
        rows = await session.execute(select(Job.external_id, Job.status))
        return dict(rows.tuples().all())


async def test_complete_listing_closes_missing_jobs(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    await index(sessionmaker, "1", "2")

    result = await ListingSyncConsumer(sessionmaker).sync(event(["1"]))

    assert result is not None and result.closed == 1
    assert await statuses(sessionmaker) == {"1": "open", "2": "closed"}
    async with sessionmaker() as session:
        topics = (await session.execute(select(Outbox.topic))).scalars().all()
    assert "job.closed" in topics


async def test_incomplete_listing_never_closes(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    await index(sessionmaker, "1", "2")

    await ListingSyncConsumer(sessionmaker).sync(event(["1"], complete=False))

    assert await statuses(sessionmaker) == {"1": "open", "2": "open"}


async def test_empty_listing_never_closes(sessionmaker: async_sessionmaker[AsyncSession]) -> None:
    await index(sessionmaker, "1")

    await ListingSyncConsumer(sessionmaker).sync(event([]))

    assert await statuses(sessionmaker) == {"1": "open"}


async def test_listed_closed_job_is_reopened(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    await index(sessionmaker, "1", "2")
    consumer = ListingSyncConsumer(sessionmaker)
    await consumer.sync(event(["1"]))

    result = await consumer.sync(event(["1", "2"]))

    assert result is not None and result.reopened == 1
    assert await statuses(sessionmaker) == {"1": "open", "2": "open"}
