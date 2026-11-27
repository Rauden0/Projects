from typing import Annotated

from fastapi import APIRouter, Path, Query

from jobmatcher.agent.tools import MAX_JOB_ID
from jobmatcher.api.deps import EmbedderDep, SessionDep
from jobmatcher.api.errors import ERROR_RESPONSES
from jobmatcher.api.schemas import JobDetail
from jobmatcher.db import repository
from jobmatcher.db.models import Job
from jobmatcher.domain import RemotePolicy, Seniority
from jobmatcher.search.hybrid import Mode, SearchFilters, SearchHit, search_jobs

router = APIRouter(prefix="/jobs", tags=["jobs"], responses=ERROR_RESPONSES)


@router.get("/search", response_model=list[SearchHit])
async def search(
    session: SessionDep,
    embedder: EmbedderDep,
    q: Annotated[str, Query(min_length=2, max_length=500)],
    remote_policy: Annotated[list[RemotePolicy] | None, Query()] = None,
    city: str | None = None,
    max_years_required: Annotated[int | None, Query(ge=0, le=30)] = None,
    seniority: Annotated[list[Seniority] | None, Query()] = None,
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
async def get_job(job_id: Annotated[int, Path(ge=1, le=MAX_JOB_ID)], session: SessionDep) -> Job:
    return await repository.get_job(session, job_id)
