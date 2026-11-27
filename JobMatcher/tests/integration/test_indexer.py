from datetime import UTC, datetime, timedelta

import pytest
from sqlalchemy import func, select
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher.db import repository
from jobmatcher.db.models import EMBEDDING_DIM, Job, JobChunk, Outbox
from jobmatcher.domain import JobFacts, RawPosting
from jobmatcher.ingest.extraction import EXTRACTION_VERSION, HeuristicExtractor
from jobmatcher.ingest.indexer import Indexer
from jobmatcher.search.embeddings import HashingEmbedder

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


async def test_deferred_extraction_stores_pending_heuristic_facts(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    indexer = Indexer(
        sessionmaker, HeuristicExtractor(), HashingEmbedder(EMBEDDING_DIM), defer_extraction=True
    )
    result = await indexer.index(posting())
    async with sessionmaker() as session:
        job = await session.get(Job, result.job_id)
    assert job is not None
    assert job.extraction_status == "pending"
    assert "python" in job.tech_stack


NEWER = datetime(2026, 11, 27, 12, tzinfo=UTC)
OLDER = NEWER - timedelta(hours=1)


def crawled(at: datetime | None, description: str, external_id: str = "1") -> RawPosting:
    return posting(external_id, description).model_copy(update={"crawled_at": at})


async def stored(sm: async_sessionmaker[AsyncSession]) -> Job:
    async with sm() as session:
        return (await session.execute(select(Job))).scalar_one()


async def test_stale_retry_does_not_overwrite_newer_version(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    indexer = make_indexer(sessionmaker)
    await indexer.index(crawled(NEWER, "<p>Python and Go, 5+ years</p>"))

    result = await indexer.index(crawled(OLDER, "<p>Python, 2+ years</p>"))

    assert not result.changed
    job = await stored(sessionmaker)
    assert job.id == result.job_id and job.years_required_min == 5 and job.crawled_at == NEWER
    assert await count(sessionmaker, Outbox) == 1


async def test_upsert_guard_rejects_older_crawl(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    await make_indexer(sessionmaker).index(crawled(NEWER, "<p>Go</p>"))
    record = repository.PostingRecord(
        "greenhouse", "acme", "1", "u", "t", None, "old", b"x", crawled_at=OLDER
    )
    async with sessionmaker() as session, session.begin():
        source_id = await repository.get_or_create_source(session, "greenhouse", "acme")
        job_id = await repository.upsert_job(session, source_id, record, JobFacts(), "ok", [], [])
    assert job_id is None
    assert (await stored(sessionmaker)).description == "Go"


async def test_missing_crawled_at_keeps_last_write_wins(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    indexer = make_indexer(sessionmaker)
    await indexer.index(crawled(NEWER, "<p>Go</p>"))

    result = await indexer.index(crawled(None, "<p>Rust</p>"))

    assert result.changed
    job = await stored(sessionmaker)
    assert job.description == "Rust" and job.crawled_at == NEWER


async def test_listing_spares_jobs_crawled_after_it(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    indexer = make_indexer(sessionmaker)
    await indexer.index(crawled(OLDER, "<p>a</p>", "1"))
    await indexer.index(crawled(NEWER, "<p>b</p>", "2"))
    await indexer.index(crawled(OLDER, "<p>c</p>", "3"))

    async with sessionmaker() as session, session.begin():
        source_id = await repository.get_or_create_source(session, "greenhouse", "acme")
        listed_at = OLDER + timedelta(minutes=30)
        result = await repository.sync_listing(session, source_id, {"1"}, True, listed_at)

    assert result.closed == 1
    async with sessionmaker() as session:
        statuses = dict((await session.execute(select(Job.external_id, Job.status))).all())
    assert statuses == {"1": "open", "2": "open", "3": "closed"}


async def test_index_records_extraction_version_and_embedding_model(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    await make_indexer(sessionmaker).index(posting())
    async with sessionmaker() as session:
        job = (await session.execute(select(Job))).scalar_one()
        models = set((await session.execute(select(JobChunk.embedding_model))).scalars())
    assert job.extraction_version == EXTRACTION_VERSION
    assert models == {f"hashing-{EMBEDDING_DIM}"}


async def test_fallback_extraction_has_no_version(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    indexer = Indexer(
        sessionmaker, HeuristicExtractor(), HashingEmbedder(EMBEDDING_DIM), defer_extraction=True
    )
    await indexer.index(posting())
    assert (await stored(sessionmaker)).extraction_version is None
