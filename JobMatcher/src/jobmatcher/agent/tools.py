"""Tools the agent can call. Each takes validated input and returns JSON-serializable data."""

import uuid
from typing import Any

from pydantic import BaseModel, Field
from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession

from jobmatcher.cv.profile import CvProfile, compare
from jobmatcher.db.models import Cv, Job, JobChunk
from jobmatcher.search.embeddings import Embedder
from jobmatcher.search.hybrid import THIN_DESCRIPTION_CHARS, SearchFilters, search_jobs

MAX_K = 6
REMOTE_POLICIES = ["remote_country", "remote_region", "remote_global"]


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
        self._cv_cache: Cv | None = None

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
        fits = await self._fit_summaries([h.job_id for h, _ in tagged])
        # Snippets are for the model to judge relevance; keep them short to save tokens.
        return [
            {
                **h.model_dump(exclude={"location", "remote_policy", "score"}),
                # One ready-made label, so the model cannot pair a location with the wrong
                # work mode (it labelled a Brno hybrid job "remote" when given both fields).
                "where": _where(h.location, h.remote_policy),
                "snippet": h.snippet[:200],
                **({"group": g} if g else {}),
                **({"fit": fits[h.job_id]} if h.job_id in fits else {}),
            }
            for h, g in tagged
        ]

    async def _fit_summaries(self, job_ids: list[int]) -> dict[int, dict[str, Any]]:
        """Short CV fit per search hit, so the model can rank and explain gaps for every job
        without a compare_with_cv round trip each (rounds are the scarce resource)."""
        cv = await self._cv()
        if cv is None or not job_ids:
            return {}
        jobs = (await self.session.execute(select(Job).where(Job.id.in_(job_ids)))).scalars()
        chunks = (
            await self.session.execute(
                select(JobChunk.job_id, JobChunk.section, JobChunk.content).where(
                    JobChunk.job_id.in_(job_ids),
                    JobChunk.section.in_(["requirements", "nice_to_have"]),
                )
            )
        ).all()
        sections: dict[tuple[int, str], list[str]] = {}
        for job_id, section, content in chunks:
            sections.setdefault((job_id, section), []).append(content)

        summaries = {}
        for job in jobs:
            requirements = "\n".join(sections.get((job.id, "requirements"), []))
            nice = "\n".join(sections.get((job.id, "nice_to_have"), []))
            full = self._compare(cv, job, requirements, nice)
            summaries[job.id] = {
                "missing_required": full["missing_required"][:6],
                "years_gap": full["years_gap"],
                **({"note": "requirements unknown"} if "note" in full else {}),
            }
        return summaries

    async def _cv(self) -> Cv | None:
        if self.cv_id is None:
            return None
        if self._cv_cache is None:
            self._cv_cache = await self.session.get(Cv, self.cv_id)
        return self._cv_cache

    @staticmethod
    def _compare(cv: Cv, job: Job, requirements: str, nice: str) -> dict[str, Any]:
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
        out = result.model_dump()
        if len(job.description) < THIN_DESCRIPTION_CHARS:
            # Ads that redirect to an employer's JavaScript site arrive without their text.
            out["note"] = (
                "The full posting text is not available, so requirements are unknown, "
                "not absent. Tell the user to check the posting."
            )
        return out

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
        cv = await self._cv()
        if cv is None:
            raise ToolError("no CV uploaded in this conversation")
        job = await self._job(CompareInput.model_validate(raw).job_id)

        chunks = (
            await self.session.execute(select(JobChunk).where(JobChunk.job_id == job.id))
        ).scalars().all()
        requirements = "\n".join(c.content for c in chunks if c.section == "requirements")
        nice = "\n".join(c.content for c in chunks if c.section == "nice_to_have")
        return {
            "job_id": job.id,
            "title": job.title,
            "url": job.url,
            "where": _where(job.location_raw, job.remote_policy),
            **self._compare(cv, job, requirements, nice),
            # Stated explicitly so the model does not claim the CV lacks skills it has.
            "cv_skills": CvProfile.model_validate(cv.profile).skills,
        }


_WORK_MODE = {
    "onsite": "on-site",
    "hybrid": "hybrid",
    "remote_country": "remote within one country",
    "remote_region": "remote within a region",
    "remote_global": "remote from anywhere",
}


def _where(location: str | None, remote_policy: str | None) -> str:
    mode = _WORK_MODE.get(remote_policy or "", "work mode not stated")
    return f"{location or 'location not stated'} · {mode}"
