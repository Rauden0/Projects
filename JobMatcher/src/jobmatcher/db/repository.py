from collections.abc import Sequence
from dataclasses import dataclass
from datetime import datetime

from sqlalchemy import (
    ColumnElement,
    Text,
    all_,
    any_,
    bindparam,
    delete,
    func,
    or_,
    select,
    true,
    update,
)
from sqlalchemy.dialects.postgresql import ARRAY, insert
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy.sql.elements import BindParameter

from jobmatcher.db.models import Cv, Job, JobChunk, LlmUsage, Outbox, Source
from jobmatcher.domain import Chunk, JobFacts
from jobmatcher.errors import NotFound


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
    crawled_at: datetime | None = None


@dataclass(frozen=True)
class Derivation:
    """Which extraction prompt and embedding model produced a job's facts and chunks."""

    extraction_version: int | None
    embedding_model: str | None


async def get_or_create_source(session: AsyncSession, kind: str, company: str) -> int:
    stmt = (
        insert(Source)
        .values(kind=kind, company=company)
        .on_conflict_do_update(index_elements=["kind", "company"], set_={"kind": kind})
        .returning(Source.id)
    )
    return (await session.execute(stmt)).scalar_one()


async def get_job(session: AsyncSession, job_id: int) -> Job:
    job = await session.get(Job, job_id)
    if job is None:
        raise NotFound("job", job_id)
    return job


async def find_job(session: AsyncSession, source_id: int, external_id: str) -> Job | None:
    stmt = select(Job).where(Job.source_id == source_id, Job.external_id == external_id)
    return (await session.execute(stmt)).scalar_one_or_none()


def _not_older(incoming: datetime | None) -> ColumnElement[bool]:
    if incoming is None:
        return true()
    return or_(Job.crawled_at.is_(None), Job.crawled_at <= incoming)


def is_stale(job: Job, crawled_at: datetime | None) -> bool:
    """A redelivered event crawled before the stored version must not overwrite it."""
    return crawled_at is not None and job.crawled_at is not None and crawled_at < job.crawled_at


async def touch_job(session: AsyncSession, job_id: int, crawled_at: datetime | None = None) -> bool:
    result = await session.execute(
        update(Job)
        .where(Job.id == job_id, _not_older(crawled_at))
        .values(
            last_seen_at=func.now(),
            status="open",
            crawled_at=func.coalesce(crawled_at, Job.crawled_at),
        )
        .returning(Job.id)
    )
    return result.scalar_one_or_none() is not None


async def upsert_job(
    session: AsyncSession,
    source_id: int,
    record: PostingRecord,
    facts: JobFacts,
    extraction_status: str,
    chunks: list[Chunk],
    embeddings: list[list[float]],
    derivation: Derivation = Derivation(None, None),
) -> int | None:
    """Insert or replace a job and its chunks, and enqueue a job.indexed event.

    Returns None, changing nothing, when the stored job was crawled later than ``record``.
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
        "required_skills": facts.required_skills,
        "nice_to_have_skills": facts.nice_to_have_skills,
        "extraction_status": extraction_status,
        "extraction_version": derivation.extraction_version,
        "status": "open",
        "last_seen_at": func.now(),
        "crawled_at": record.crawled_at,
    }
    update_values = {k: v for k, v in values.items() if k not in {"source_id", "external_id"}}
    update_values["crawled_at"] = func.coalesce(record.crawled_at, Job.crawled_at)
    stmt = (
        insert(Job)
        .values(**values)
        .on_conflict_do_update(
            index_elements=["source_id", "external_id"],
            set_=update_values,
            where=_not_older(record.crawled_at),
        )
        .returning(Job.id)
    )
    job_id = (await session.execute(stmt)).scalar_one_or_none()
    if job_id is None:
        return None

    await session.execute(delete(JobChunk).where(JobChunk.job_id == job_id))
    session.add_all(
        JobChunk(
            job_id=job_id,
            ord=c.ord,
            section=c.section,
            content=c.content,
            embedding=e,
            embedding_model=derivation.embedding_model,
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


async def close_unseen_jobs(
    session: AsyncSession,
    source_id: int,
    seen_ids: set[str],
    listed_at: datetime | None = None,
) -> int:
    """``listed_at`` (the listing's crawl time) spares jobs seen after it: their
    job.discovered was crawled later than the listing that omits them."""
    stmt = (
        update(Job)
        .where(Job.source_id == source_id, Job.status == "open")
        .where(Job.external_id != all_(_ids_param(seen_ids)) if seen_ids else true())
        .where(
            func.coalesce(Job.crawled_at, Job.last_seen_at) < listed_at
            if listed_at is not None
            else true()
        )
        .values(status="closed")
        .returning(Job.id)
    )
    closed = (await session.execute(stmt)).scalars().all()
    session.add_all(Outbox(topic="job.closed", payload={"job_id": job_id}) for job_id in closed)
    return len(closed)


def _ids_param(ids: set[str]) -> BindParameter[Sequence[str]]:
    values: Sequence[str] = sorted(ids)
    return bindparam("external_ids", values, type_=ARRAY(Text))


@dataclass(frozen=True)
class ListingSync:
    reopened: int
    closed: int


async def sync_listing(
    session: AsyncSession,
    source_id: int,
    listed_ids: set[str],
    complete: bool,
    listed_at: datetime | None = None,
) -> ListingSync:
    reopened = await _touch_listed(session, source_id, listed_ids)
    if not (complete and listed_ids):
        return ListingSync(reopened, 0)
    closed = await close_unseen_jobs(session, source_id, listed_ids, listed_at)
    return ListingSync(reopened, closed)


async def _touch_listed(session: AsyncSession, source_id: int, listed_ids: set[str]) -> int:
    if not listed_ids:
        return 0
    listed = (Job.source_id == source_id) & (Job.external_id == any_(_ids_param(listed_ids)))
    reopened = await session.execute(
        update(Job).where(listed, Job.status == "closed").values(status="open").returning(Job.id)
    )
    await session.execute(update(Job).where(listed).values(last_seen_at=func.now()))
    return len(reopened.all())


async def delete_cvs_before(session: AsyncSession, cutoff: datetime) -> int:
    result = await session.execute(delete(Cv).where(Cv.created_at < cutoff).returning(Cv.id))
    return len(result.all())


async def delete_outbox_published_before(session: AsyncSession, cutoff: datetime) -> int:
    result = await session.execute(
        delete(Outbox).where(Outbox.published_at < cutoff).returning(Outbox.id)
    )
    return len(result.all())


async def forget_usage_clients_before(session: AsyncSession, cutoff: datetime) -> int:
    result = await session.execute(
        update(LlmUsage)
        .where(LlmUsage.created_at < cutoff, LlmUsage.client_id.is_not(None))
        .values(client_id=None)
        .returning(LlmUsage.id)
    )
    return len(result.all())
