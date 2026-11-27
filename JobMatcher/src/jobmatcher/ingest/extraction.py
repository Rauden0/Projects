"""Structured fields extracted from free-text postings.

The LLM extractor is the primary path; the heuristic extractor is used when no LLM is
configured, and by the indexer as a recorded fallback when the LLM call fails or its output
fails validation twice (``extraction_status = 'fallback'``, retried by ``reextract``).
"""

import re
from typing import Any, Protocol, get_args

from pydantic import BaseModel, Field, ValidationError

from jobmatcher.domain import ContractType, JobFacts, RemotePolicy, Seniority
from jobmatcher.ingest.normalize import compact_for_llm
from jobmatcher.llm import LLMClient


class Extractor(Protocol):
    async def extract(self, title: str, location: str | None, text: str) -> JobFacts: ...


_TECH = [
    "python",
    "fastapi",
    "django",
    "flask",
    "sqlalchemy",
    "pydantic",
    "asyncio",
    "celery",
    "postgresql",
    "mysql",
    "mongodb",
    "redis",
    "timescaledb",
    "elasticsearch",
    "opensearch",
    "rabbitmq",
    "kafka",
    "docker",
    "kubernetes",
    "terraform",
    "aws",
    "gcp",
    "azure",
    "airflow",
    "dbt",
    "spark",
    "databricks",
    "snowflake",
    "go",
    "rust",
    "typescript",
    "react",
    "java",
    "c++",
    "pytorch",
    "langchain",
    "langgraph",
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
        levels: tuple[Seniority, ...] = ("staff", "senior", "medior", "junior")
        for level in levels:
            if level in title.lower() or (level == "medior" and "mid" in title.lower()):
                seniority = level
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


def _nullable(schema: dict[str, Any]) -> dict[str, Any]:
    return {"anyOf": [schema, {"type": "null"}]}


def _enum(values: tuple[str, ...]) -> dict[str, Any]:
    return _nullable({"type": "string", "enum": list(values)})


_STRINGS = {"type": "array", "items": {"type": "string"}}

COMPACT_SCHEMA: dict[str, Any] = {
    "type": "object",
    "properties": {
        "sen": _enum(get_args(Seniority)),
        "yrs": _nullable({"type": "integer"}),
        "rem": _enum(get_args(RemotePolicy)),
        "city": _nullable({"type": "string"}),
        "smin": _nullable({"type": "integer"}),
        "smax": _nullable({"type": "integer"}),
        "cur": _nullable({"type": "string"}),
        "ct": _enum(get_args(ContractType)),
        "tech": _STRINGS,
        "req": _STRINGS,
        "nice": _STRINGS,
        "lang": _STRINGS,
    },
    "required": [
        "sen",
        "yrs",
        "rem",
        "city",
        "smin",
        "smax",
        "cur",
        "ct",
        "tech",
        "req",
        "nice",
        "lang",
    ],
    "additionalProperties": False,
}

EXTRACTION_SYSTEM = """Extract facts from a job posting as JSON. Keys:
sen: seniority from title or experience; "II"/"III" levels are not senior
yrs: minimum years of experience explicitly required
rem: onsite | hybrid | remote_country (one country) | remote_region (EU, EMEA, time zone) |
  remote_global (anywhere)
city: office city only, e.g. "Brno"
smin, smax, cur: salary as written, ISO currency
ct: contract type
tech: lowercase technologies asked for or used
req, nice: short lowercase hard skills, must-have vs. plus/bonus/preferred
lang: spoken languages required
Use null or [] when the posting does not say."""


class CompactFacts(BaseModel):
    sen: Seniority | None = None
    yrs: int | None = None
    rem: RemotePolicy | None = None
    city: str | None = None
    smin: int | None = None
    smax: int | None = None
    cur: str | None = None
    ct: ContractType | None = None
    tech: list[str] = Field(default_factory=list)
    req: list[str] = Field(default_factory=list)
    nice: list[str] = Field(default_factory=list)
    lang: list[str] = Field(default_factory=list)

    def to_facts(self) -> JobFacts:
        return JobFacts(
            seniority=self.sen,
            years_required_min=self.yrs,
            remote_policy=self.rem,
            office_city=self.city,
            salary_min=self.smin,
            salary_max=self.smax,
            currency=self.cur,
            contract_type=self.ct,
            tech_stack=self.tech,
            required_skills=self.req,
            nice_to_have_skills=self.nice,
            languages_required=self.lang,
        )


def parse_compact(raw: str) -> JobFacts:
    return CompactFacts.model_validate_json(_strip_fences(raw)).to_facts()


class ExtractionError(Exception):
    """The LLM did not produce valid facts; callers fall back and record it."""


def extraction_prompt(title: str, location: str | None, text: str) -> str:
    return f"Title: {title}\nLocation: {location or 'n/a'}\n\n{compact_for_llm(text)}"


class LLMExtractor:
    def __init__(self, llm: LLMClient) -> None:
        self.llm = llm

    async def extract(self, title: str, location: str | None, text: str) -> JobFacts:
        user = extraction_prompt(title, location, text)
        error: str | None = None
        for _ in range(2):
            prompt = user if error is None else f"{user}\n\nPrevious output was invalid: {error}"
            raw = await self.llm.complete_json(
                system=EXTRACTION_SYSTEM, user=prompt, schema=COMPACT_SCHEMA, max_tokens=400
            )
            try:
                return parse_compact(raw)
            except ValidationError as exc:
                error = str(exc)[:500]
        raise ExtractionError(f"invalid LLM output after retry: {error}")


def _strip_fences(raw: str) -> str:
    raw = raw.strip()
    if raw.startswith("```"):
        raw = raw.split("\n", 1)[1].rsplit("```", 1)[0]
    return raw
