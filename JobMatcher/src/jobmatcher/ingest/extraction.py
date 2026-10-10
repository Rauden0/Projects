"""Structured fields extracted from free-text postings.

The LLM extractor is the primary path; the heuristic extractor is used when no LLM is
configured, and by the indexer as a recorded fallback when the LLM call fails or its output
fails validation twice (``extraction_status = 'fallback'``, retried by ``reextract``).
"""

import json
import re
from typing import Literal, Protocol

from pydantic import BaseModel, Field, ValidationError

from jobmatcher.llm import LLMClient

Seniority = Literal["junior", "medior", "senior", "staff"]
RemotePolicy = Literal["onsite", "hybrid", "remote_country", "remote_region", "remote_global"]
ContractType = Literal["employment", "contract", "freelance"]


class JobFacts(BaseModel):
    seniority: Seniority | None = None
    years_required_min: int | None = Field(default=None, ge=0, le=30)
    remote_policy: RemotePolicy | None = None
    office_city: str | None = None
    salary_min: int | None = Field(default=None, ge=0)
    salary_max: int | None = Field(default=None, ge=0)
    currency: str | None = Field(default=None, min_length=3, max_length=3)
    contract_type: ContractType | None = None
    tech_stack: list[str] = Field(default_factory=list)
    required_skills: list[str] = Field(default_factory=list)
    nice_to_have_skills: list[str] = Field(default_factory=list)
    languages_required: list[str] = Field(default_factory=list)


class Extractor(Protocol):
    async def extract(self, title: str, location: str | None, text: str) -> JobFacts: ...


_TECH = [
    "python", "fastapi", "django", "flask", "sqlalchemy", "pydantic", "asyncio", "celery",
    "postgresql", "mysql", "mongodb", "redis", "timescaledb", "elasticsearch", "opensearch",
    "rabbitmq", "kafka", "docker", "kubernetes", "terraform", "aws", "gcp", "azure",
    "airflow", "dbt", "spark", "databricks", "snowflake", "go", "rust", "typescript",
    "react", "java", "c++", "pytorch", "langchain", "langgraph",
]
_TECH_RE = {t: re.compile(rf"(?<![\w+]){re.escape(t)}(?![\w+])", re.I) for t in _TECH}
_YEARS_RE = re.compile(
    r"(\d{1,2})\s*\+?\s*(?:-\s*\d{1,2}\s*)?(?:years?|yrs|let|roky|roků|rok)\b", re.I
)
_CITY_RE = re.compile(r"\b(Brno|Prague|Praha|Ostrava|Bratislava|Vienna|Berlin|Warsaw)\b", re.I)


class HeuristicExtractor:
    async def extract(self, title: str, location: str | None, text: str) -> JobFacts:
        haystack = f"{title}\n{location or ''}\n{text}"
        lowered = haystack.lower()

        years = [int(m.group(1)) for m in _YEARS_RE.finditer(text)]
        seniority: Seniority | None = None
        for level in ("staff", "senior", "medior", "junior"):
            if level in title.lower() or (level == "medior" and "mid" in title.lower()):
                seniority = level  # type: ignore[assignment]
                break

        remote: RemotePolicy | None = None
        if "hybrid" in lowered:
            remote = "hybrid"
        elif re.search(r"fully remote|100\s*% remote|remote[- ]first|work from anywhere", lowered):
            remote = "remote_global" if "anywhere" in lowered else "remote_region"
        elif "remote" in (location or "").lower():
            remote = "remote_region"

        city = _CITY_RE.search(location or "") or _CITY_RE.search(text)
        return JobFacts(
            seniority=seniority,
            years_required_min=min(years) if years else None,
            remote_policy=remote,
            office_city=city.group(1).replace("Praha", "Prague") if city else None,
            tech_stack=[t for t, rx in _TECH_RE.items() if rx.search(haystack)],
        )


_SYSTEM = """You extract structured facts from job postings.
Return only a JSON object matching this schema, using null when the posting does not say:
{schema}
Rules:
- years_required_min: the minimum years of experience explicitly required, as an integer.
- remote_policy: onsite | hybrid | remote_country (remote only within one country) |
  remote_region (remote within a region such as EU, EMEA or a time zone) |
  remote_global (explicitly anywhere in the world).
- office_city: city name only, e.g. "Brno", not "Brno, Czech Republic".
- seniority: from the title or required experience; "II"/"III" levels are not "senior".
- tech_stack: lowercase technology names that the posting asks for or uses.
- required_skills / nice_to_have_skills: short lowercase skills split by how the posting
  states them (must have vs. plus / bonus / preferred), e.g. "python", "kubernetes",
  "distributed systems", "linux". Technologies and hard skills only, no soft skills.
- salary: monthly or yearly as written, in the stated currency (ISO code)."""


class ExtractionError(Exception):
    """The LLM did not produce valid facts; callers fall back and record it."""


class LLMExtractor:
    def __init__(self, llm: LLMClient) -> None:
        self.llm = llm

    async def extract(self, title: str, location: str | None, text: str) -> JobFacts:
        system = _SYSTEM.format(schema=json.dumps(JobFacts.model_json_schema()))
        user = f"Title: {title}\nLocation: {location or 'n/a'}\n\n{text[:12000]}"
        error: str | None = None
        for _ in range(2):
            prompt = user if error is None else f"{user}\n\nPrevious output was invalid: {error}"
            raw = await self.llm.complete(system=system, user=prompt, max_tokens=800)
            try:
                return JobFacts.model_validate_json(_strip_fences(raw))
            except ValidationError as exc:
                error = str(exc)[:500]
        raise ExtractionError(f"invalid LLM output after retry: {error}")


def _strip_fences(raw: str) -> str:
    raw = raw.strip()
    if raw.startswith("```"):
        raw = raw.split("\n", 1)[1].rsplit("```", 1)[0]
    return raw
