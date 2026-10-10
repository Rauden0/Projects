import pytest
from sqlalchemy import func, select
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher.db import repository
from jobmatcher.db.models import EMBEDDING_DIM, Job, JobChunk, Outbox
from jobmatcher.ingest.extraction import HeuristicExtractor
from jobmatcher.ingest.indexer import Indexer
from jobmatcher.search.embeddings import HashingEmbedder
from jobmatcher.sources import RawPosting

pytestmark = pytest.mark.integration


def posting(external_id: str = "1", description: str = "<p>Python, 2+ years</p>") -> RawPosting:
    return RawPosting(
        kind="greenhouse",
        company="acme",
        external_id=external_id,
        url=f"https://example.com/{external_id}",
        title="Python Developer",
        location="Brno, Czechia",
        description_html=description,
    )


def make_indexer(sm: async_sessionmaker[AsyncSession]) -> Indexer:
    return Indexer(sm, HeuristicExtractor(), HashingEmbedder(EMBEDDING_DIM))


async def count(sm: async_sessionmaker[AsyncSession], model: type) -> int:
    async with sm() as session:
        return (await session.execute(select(func.count()).select_from(model))).scalar_one()


async def test_index_is_idempotent(sessionmaker: async_sessionmaker[AsyncSession]) -> None:
    indexer = make_indexer(sessionmaker)

    first = await indexer.index(posting())
    second = await indexer.index(posting())

    assert first.changed and not second.changed
    assert first.job_id == second.job_id
    assert await count(sessionmaker, Job) == 1
    assert await count(sessionmaker, Outbox) == 1


async def test_changed_description_replaces_chunks(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    indexer = make_indexer(sessionmaker)
    await indexer.index(posting())
    result = await indexer.index(posting(description="<p>Python and Go, 5+ years</p>"))

    assert result.changed
    async with sessionmaker() as session:
        job = await session.get(Job, result.job_id)
        assert job is not None and job.years_required_min == 5
        chunks = (await session.execute(select(JobChunk))).scalars().all()
        assert all("Go" in c.content for c in chunks)
    assert await count(sessionmaker, Outbox) == 2


async def test_close_unseen_jobs(sessionmaker: async_sessionmaker[AsyncSession]) -> None:
    indexer = make_indexer(sessionmaker)
    await indexer.index(posting("1"))
    await indexer.index(posting("2"))

    async with sessionmaker() as session, session.begin():
        source_id = await repository.get_or_create_source(session, "greenhouse", "acme")
        closed = await repository.close_unseen_jobs(session, source_id, {"1"})

    assert closed == 1
    async with sessionmaker() as session:
        statuses = dict((await session.execute(select(Job.external_id, Job.status))).all())
    assert statuses == {"1": "open", "2": "closed"}
