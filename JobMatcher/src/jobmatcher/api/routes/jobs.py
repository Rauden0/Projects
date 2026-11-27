from typing import Annotated

from fastapi import APIRouter, Depends, Path, Query, Response

from jobmatcher.agent.tools import MAX_JOB_ID
from jobmatcher.api.deps import (
    AuthenticatorDep,
    EmbedderDep,
    SearchCacheDep,
    SessionDep,
    limit_search,
    require_search_access,
)
from jobmatcher.api.errors import ERROR_RESPONSES
from jobmatcher.api.schemas import JobDetail
from jobmatcher.caching import normalize_text
from jobmatcher.db import repository
from jobmatcher.db.models import Job
from jobmatcher.domain import RemotePolicy, Seniority
from jobmatcher.search.hybrid import Mode, SearchFilters, SearchHit, search_jobs

router = APIRouter(
    prefix="/jobs",
    tags=["jobs"],
    responses=ERROR_RESPONSES,
    dependencies=[Depends(require_search_access)],
)


@router.get("/search", response_model=list[SearchHit], dependencies=[Depends(limit_search)])
async def search(
    response: Response,
    session: SessionDep,
    embedder: EmbedderDep,
    cache: SearchCacheDep,
    auth: AuthenticatorDep,
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
    query = normalize_text(q)
    if cache.ttl_s > 0:
        scope = "private" if auth.protects_search else "public"
        response.headers["Cache-Control"] = f"{scope}, max-age={cache.ttl_s}"
    return await cache.fetch(
        session,
        (query, filters.model_dump_json(), k, mode),
        lambda: search_jobs(session, embedder, query, filters, k=k, mode=mode),
    )


@router.get("/{job_id}", response_model=JobDetail)
async def get_job(job_id: Annotated[int, Path(ge=1, le=MAX_JOB_ID)], session: SessionDep) -> Job:
    return await repository.get_job(session, job_id)
