"""Tools the agent can call. Each takes validated input and returns JSON-serializable data."""

import uuid
from typing import Any

from pydantic import BaseModel, Field
from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession

from jobmatcher.cv.profile import CvProfile, compare
from jobmatcher.db.models import Cv, Job, JobChunk
from jobmatcher.search.embeddings import Embedder
from jobmatcher.search.hybrid import SearchFilters, search_jobs

MAX_K = 6
REMOTE_POLICIES = ["remote_country", "remote_region", "remote_global"]
THIN_DESCRIPTION_CHARS = 300


class SearchJobsInput(SearchFilters):
    query: str = Field(description="What to look for, in natural language or keywords")
    # No schema maximum: models like to ask for 10 and providers reject schema violations.
    # The value is clamped instead.
    k: int = Field(default=5, ge=1)


class GetJobInput(BaseModel):
    job_id: int


class CompareInput(BaseModel):
    job_id: int


class ToolError(Exception):
    pass


class Toolbox:
    def __init__(self, session: AsyncSession, embedder: Embedder, cv_id: uuid.UUID | None):
        self.session = session
        self.embedder = embedder
        self.cv_id = cv_id
        self.seen_urls: set[str] = set()

    @staticmethod
    def definitions() -> list[dict[str, Any]]:
        return [
            {
                "name": "search_jobs",
                "description": "Hybrid search over open job postings with optional hard filters. "
                "Returns ranked jobs with a matching snippet.",
                "input_schema": SearchJobsInput.model_json_schema(),
            },
            {
                "name": "get_job",
                "description": "Full posting text and extracted facts for one job.",
                "input_schema": GetJobInput.model_json_schema(),
            },
            {
                "name": "compare_with_cv",
                "description": "Compare one job with the user's CV: matched skills, missing "
                "required and nice-to-have skills, years gap and location fit.",
                "input_schema": CompareInput.model_json_schema(),
            },
        ]

    async def call(self, name: str, raw_input: dict[str, Any]) -> Any:
        handler = {
            "search_jobs": self.search_jobs,
            "get_job": self.get_job,
            "compare_with_cv": self.compare_with_cv,
        }.get(name)
        if handler is None:
            raise ToolError(f"unknown tool {name!r}")
        return await handler(raw_input)

    async def search_jobs(self, raw: dict[str, Any]) -> list[dict[str, Any]]:
        args = SearchJobsInput.model_validate(raw)
        filters = SearchFilters.model_validate(args.model_dump(exclude={"query", "k"}))
        k = min(args.k, MAX_K)
        wants_remote = not filters.remote_policy or any(
            p.startswith("remote") for p in filters.remote_policy
        )
        if filters.city and wants_remote:
            # "In <city> or remote": rank the two groups separately. Mixed together, remote
            # postings from large companies (long, keyword-rich descriptions) push local ones
            # out of the top results, and the user never sees the jobs in their own city.
            local_filters = filters.model_copy(update={"remote_policy": None})
            remote_filters = filters.model_copy(
                update={
                    "city": None,
                    "remote_policy": [
                        p for p in (filters.remote_policy or REMOTE_POLICIES)
                        if p.startswith("remote")
                    ],
                }
            )
            half = max(1, k // 2)
            local = await search_jobs(
                self.session, self.embedder, args.query, local_filters, k=half, strict_city=True
            )
            remote = await search_jobs(
                self.session, self.embedder, args.query, remote_filters, k=k - len(local)
            )
            tagged = [(h, "local") for h in local] + [
                (h, "remote") for h in remote if h.job_id not in {x.job_id for x in local}
            ]
        else:
            hits = await search_jobs(self.session, self.embedder, args.query, filters, k=k)
            tagged = [(h, None) for h in hits]

        self.seen_urls.update(h.url for h, _ in tagged)
        # Snippets are for the model to judge relevance; keep them short to save tokens.
        return [
            {**h.model_dump(), "snippet": h.snippet[:200], **({"group": g} if g else {})}
            for h, g in tagged
        ]

    async def _job(self, job_id: int) -> Job:
        job = await self.session.get(Job, job_id)
        if job is None:
            raise ToolError(f"job {job_id} does not exist")
        return job

    async def get_job(self, raw: dict[str, Any]) -> dict[str, Any]:
        job = await self._job(GetJobInput.model_validate(raw).job_id)
        self.seen_urls.add(job.url)
        return {
            "job_id": job.id,
            "title": job.title,
            "company": job.company,
            "url": job.url,
            "location": job.location_raw,
            "status": job.status,
            "seniority": job.seniority,
            "years_required_min": job.years_required_min,
            "remote_policy": job.remote_policy,
            "salary": [job.salary_min, job.salary_max, job.currency],
            "tech_stack": job.tech_stack,
            "required_skills": job.required_skills,
            "nice_to_have_skills": job.nice_to_have_skills,
            "description": job.description[:3000],
        }

    async def compare_with_cv(self, raw: dict[str, Any]) -> dict[str, Any]:
        if self.cv_id is None:
            raise ToolError("no CV uploaded in this conversation")
        cv = await self.session.get(Cv, self.cv_id)
        if cv is None:
            raise ToolError("CV not found")
        job = await self._job(CompareInput.model_validate(raw).job_id)

        chunks = (
            await self.session.execute(select(JobChunk).where(JobChunk.job_id == job.id))
        ).scalars().all()
        requirements = "\n".join(c.content for c in chunks if c.section == "requirements")
        nice = "\n".join(c.content for c in chunks if c.section == "nice_to_have")

        result = compare(
            CvProfile.model_validate(cv.profile),
            job.tech_stack,
            requirements or job.description,
            nice,
            job.years_required_min,
            job.remote_policy,
            job.office_city,
            required_skills=job.required_skills,
            nice_to_have_skills=job.nice_to_have_skills,
            cv_text=cv.raw_text,
        )
        out = {
            "job_id": job.id,
            "title": job.title,
            "url": job.url,
            "remote_policy": job.remote_policy,
            **result.model_dump(),
        }
        if len(job.description) < THIN_DESCRIPTION_CHARS:
            # Ads that redirect to an employer's JavaScript site arrive without their text.
            out["note"] = (
                "The full posting text is not available, so requirements are unknown, "
                "not absent. Tell the user to check the posting."
            )
        return out
