from collections.abc import Sequence
from dataclasses import dataclass
from datetime import datetime

from sqlalchemy import (
    Boolean,
    ColumnElement,
    Text,
    all_,
    any_,
    bindparam,
    delete,
    func,
    literal_column,
    or_,
    select,
    text,
    true,
    update,
)
from sqlalchemy.dialects.postgresql import ARRAY, insert
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy.sql.elements import BindParameter

from jobmatcher.db.models import Cv, Job, JobChunk, LlmUsage, Outbox, Source
from jobmatcher.domain import (
    JOB_CLOSED,
    JOB_CREATED,
    JOB_UPDATED,
    THIN_DESCRIPTION_CHARS,
    Chunk,
    JobChanged,
    JobClosed,
    JobFacts,
)
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
    source: str | None = None


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
    """Insert or replace a job and its chunks, and enqueue a job.created / job.updated event.

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
        .returning(Job.id, literal_column("xmax = 0", Boolean).label("inserted"))
    )
    row = (await session.execute(stmt)).one_or_none()
    if row is None:
        return None
    job_id, inserted = row

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
    changed = JobChanged(
        job_id=job_id,
        kind=record.kind,
        source=record.source or record.company,
        external_id=record.external_id,
        url=record.url,
    )
    session.add(
        Outbox(topic=JOB_CREATED if inserted else JOB_UPDATED, payload=changed.model_dump())
    )
    await session.flush()
    await assign_canonical(session, await _keys_around(session, job_id))
    return job_id


# Two full postings with the same blocking key from different sources are the same job only
# when their first chunks also agree; a thin board stub needs the key alone.
FULL_DUPLICATE_MIN_SIMILARITY = 0.9

_ASSIGN_CANONICAL = text(
    """
    WITH grp AS (
        SELECT j.id, j.source_id, length(j.description) AS len,
               first_value(j.id) OVER (
                   PARTITION BY j.dedup_key ORDER BY length(j.description) DESC, j.id
               ) AS canon
        FROM job j
        WHERE j.status = 'open' AND j.dedup_key = ANY(:keys)
    ),
    decided AS (
        SELECT g.id,
               CASE WHEN g.id = g.canon THEN NULL
                    WHEN g.len < :thin AND c.len >= :thin THEN g.canon
                    WHEN g.len >= :thin AND g.source_id <> c.source_id AND (
                        SELECT 1 - (a.embedding <=> b.embedding)
                        FROM job_chunk a, job_chunk b
                        WHERE a.job_id = g.id AND a.ord = 0 AND b.job_id = g.canon AND b.ord = 0
                    ) >= :min_similarity THEN g.canon
               END AS canonical
        FROM grp g JOIN grp c ON c.id = g.canon
    )
    UPDATE job SET canonical_job_id = d.canonical
    FROM decided d
    WHERE job.id = d.id AND job.canonical_job_id IS DISTINCT FROM d.canonical
    """
)


async def assign_canonical(session: AsyncSession, keys: set[str]) -> None:
    """Point every duplicate in the ``keys`` groups at the group's fullest open posting
    (``canonical_job_id``); the canonical posting and distinct jobs get NULL."""
    if not keys:
        return
    ordered = sorted(keys)
    # Serialises concurrent recomputes of a group; sorted, so two groups cannot deadlock.
    await session.execute(
        text("SELECT pg_advisory_xact_lock(hashtext(k)) FROM unnest(CAST(:keys AS text[])) k"),
        {"keys": ordered},
    )
    await session.execute(
        _ASSIGN_CANONICAL,
        {
            "keys": ordered,
            "thin": THIN_DESCRIPTION_CHARS,
            "min_similarity": FULL_DUPLICATE_MIN_SIMILARITY,
        },
    )


async def _keys_around(session: AsyncSession, job_id: int) -> set[str]:
    """The job's group, and the groups of jobs pointing at it (its old group if it moved)."""
    rows = await session.execute(
        select(Job.dedup_key).where(or_(Job.id == job_id, Job.canonical_job_id == job_id))
    )
    return set(rows.scalars())


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
        .values(status="closed", canonical_job_id=None)
        .returning(Job.id, Job.external_id, Job.dedup_key)
    )
    closed = (await session.execute(stmt)).all()
    await assign_canonical(session, {key for _, _, key in closed})
    session.add_all(
        Outbox(
            topic=JOB_CLOSED,
            payload=JobClosed(job_id=job_id, external_id=external_id).model_dump(),
        )
        for job_id, external_id, _ in closed
    )
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
        update(Job)
        .where(listed, Job.status == "closed")
        .values(status="open")
        .returning(Job.dedup_key)
    )
    keys = list(reopened.scalars())
    await session.execute(update(Job).where(listed).values(last_seen_at=func.now()))
    await assign_canonical(session, set(keys))
    return len(keys)


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
