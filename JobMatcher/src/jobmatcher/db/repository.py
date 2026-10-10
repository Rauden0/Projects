from dataclasses import dataclass

from sqlalchemy import func, select, true, update
from sqlalchemy.dialects.postgresql import insert
from sqlalchemy.ext.asyncio import AsyncSession

from jobmatcher.db.models import Job, JobChunk, Outbox, Source
from jobmatcher.ingest.chunking import Chunk
from jobmatcher.ingest.extraction import JobFacts


@dataclass(frozen=True)
class PostingRecord:
    kind: str
    company: str
    external_id: str
    url: str
    title: str
    location: str | None
    description: str
    content_hash: bytes


async def get_or_create_source(session: AsyncSession, kind: str, company: str) -> int:
    stmt = (
        insert(Source)
        .values(kind=kind, company=company)
        .on_conflict_do_update(index_elements=["kind", "company"], set_={"kind": kind})
        .returning(Source.id)
    )
    return (await session.execute(stmt)).scalar_one()


async def find_job(session: AsyncSession, source_id: int, external_id: str) -> Job | None:
    stmt = select(Job).where(Job.source_id == source_id, Job.external_id == external_id)
    return (await session.execute(stmt)).scalar_one_or_none()


async def touch_job(session: AsyncSession, job_id: int) -> None:
    await session.execute(
        update(Job).where(Job.id == job_id).values(last_seen_at=func.now(), status="open")
    )


async def upsert_job(
    session: AsyncSession,
    source_id: int,
    record: PostingRecord,
    facts: JobFacts,
    extraction_status: str,
    chunks: list[Chunk],
    embeddings: list[list[float]],
) -> int:
    """Insert or replace a job and its chunks, and enqueue a job.indexed event.

    Must run inside the caller's transaction: the outbox row commits atomically with the data.
    """
    values = {
        "source_id": source_id,
        "external_id": record.external_id,
        "url": record.url,
        "title": record.title,
        "company": record.company,
        "location_raw": record.location,
        "description": record.description,
        "content_hash": record.content_hash,
        "seniority": facts.seniority,
        "years_required_min": facts.years_required_min,
        "remote_policy": facts.remote_policy,
        "office_city": facts.office_city,
        "salary_min": facts.salary_min,
        "salary_max": facts.salary_max,
        "currency": facts.currency,
        "contract_type": facts.contract_type,
        "tech_stack": facts.tech_stack,
        "extraction_status": extraction_status,
        "status": "open",
        "last_seen_at": func.now(),
    }
    stmt = (
        insert(Job)
        .values(**values)
        .on_conflict_do_update(
            index_elements=["source_id", "external_id"],
            set_={k: v for k, v in values.items() if k not in {"source_id", "external_id"}},
        )
        .returning(Job.id)
    )
    job_id = (await session.execute(stmt)).scalar_one()

    await session.execute(JobChunk.__table__.delete().where(JobChunk.job_id == job_id))
    session.add_all(
        JobChunk(
            job_id=job_id, ord=c.ord, section=c.section, content=c.content, embedding=e
        )
        for c, e in zip(chunks, embeddings, strict=True)
    )
    session.add(
        Outbox(
            topic="job.indexed",
            payload={"job_id": job_id, "source": record.kind, "external_id": record.external_id},
        )
    )
    await session.flush()
    return job_id


async def close_unseen_jobs(session: AsyncSession, source_id: int, seen_ids: set[str]) -> int:
    """Mark open jobs of a source that were not in the latest crawl as closed."""
    stmt = (
        update(Job)
        .where(Job.source_id == source_id, Job.status == "open")
        .where(Job.external_id.not_in(seen_ids) if seen_ids else true())
        .values(status="closed")
        .returning(Job.id)
    )
    closed = (await session.execute(stmt)).scalars().all()
    session.add_all(Outbox(topic="job.closed", payload={"job_id": job_id}) for job_id in closed)
    return len(closed)
