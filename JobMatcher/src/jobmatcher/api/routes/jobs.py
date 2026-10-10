from typing import Annotated

from fastapi import APIRouter, HTTPException, Query

from jobmatcher.api.deps import EmbedderDep, SessionDep
from jobmatcher.api.schemas import JobDetail
from jobmatcher.db.models import Job
from jobmatcher.search.hybrid import Mode, SearchFilters, SearchHit, search_jobs

router = APIRouter(prefix="/jobs", tags=["jobs"])


@router.get("/search", response_model=list[SearchHit])
async def search(
    session: SessionDep,
    embedder: EmbedderDep,
    q: Annotated[str, Query(min_length=2, max_length=500)],
    remote_policy: Annotated[list[str] | None, Query()] = None,
    city: str | None = None,
    max_years_required: Annotated[int | None, Query(ge=0, le=30)] = None,
    seniority: Annotated[list[str] | None, Query()] = None,
    tech: Annotated[list[str] | None, Query()] = None,
    k: Annotated[int, Query(ge=1, le=50)] = 10,
    mode: Mode = "hybrid",
) -> list[SearchHit]:
    filters = SearchFilters(
        remote_policy=remote_policy,
        city=city,
        max_years_required=max_years_required,
        seniority=seniority,
        tech=tech,
    )
    return await search_jobs(session, embedder, q, filters, k=k, mode=mode)


@router.get("/{job_id}", response_model=JobDetail)
async def get_job(job_id: int, session: SessionDep) -> Job:
    job = await session.get(Job, job_id)
    if job is None:
        raise HTTPException(status_code=404, detail="job not found")
    return job
