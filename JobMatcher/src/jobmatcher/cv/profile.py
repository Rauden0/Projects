"""CV profile extraction and deterministic CV-to-job comparison."""

import re
from datetime import date

from pydantic import BaseModel, Field

from jobmatcher.ingest.extraction import HeuristicExtractor

_RANGE_RE = re.compile(
    r"(?P<m1>\d{1,2})/(?P<y1>\d{4})\s*[-–—]\s*(?:(?P<m2>\d{1,2})/(?P<y2>\d{4})|present|now|současnost)",
    re.I,
)
_LANG_RE = re.compile(r"\b(English|Czech|German|Slovak|Russian|Polish|French)\b", re.I)


class CvProfile(BaseModel):
    skills: list[str] = Field(default_factory=list)
    years_experience: float = 0.0
    languages: list[str] = Field(default_factory=list)
    city: str | None = None


def _experience_years(text: str, today: date) -> float:
    """Sum of employment ranges written as MM/YYYY – MM/YYYY|Present. Education sections are
    excluded by only reading the text before an 'Education' heading when there is one."""
    head = re.split(r"\n\s*(?:education|vzdělání)\b", text, maxsplit=1, flags=re.I)[0]
    months = 0
    for m in _RANGE_RE.finditer(head):
        start = int(m["y1"]) * 12 + int(m["m1"])
        end = int(m["y2"]) * 12 + int(m["m2"]) if m["y2"] else today.year * 12 + today.month
        months += max(0, end - start)
    return round(months / 12, 1)


async def build_profile(text: str, today: date | None = None) -> CvProfile:
    facts = await HeuristicExtractor().extract("", None, text)
    return CvProfile(
        skills=facts.tech_stack,
        years_experience=_experience_years(text, today or date.today()),
        languages=sorted({m.group(1).capitalize() for m in _LANG_RE.finditer(text)}),
        city=facts.office_city,
    )


class Comparison(BaseModel):
    matched_skills: list[str]
    missing_required: list[str]
    missing_nice_to_have: list[str]
    years_required: int | None
    years_gap: float
    location_fit: str


def _mentions(text: str, term: str) -> bool:
    return bool(re.search(rf"(?<!\w){re.escape(term)}(?!\w)", text, re.I))


# Extracted skills are free-form ("async programming", "rest apis", "testing"), CVs name the
# tools ("asyncio", "FastAPI", "pytest"). Matching word stems bridges most of that gap.
_STOPWORDS = {
    "and", "or", "of", "the", "with", "in", "for", "experience", "knowledge", "skills",
    "development", "programming", "design", "systems", "system",
    # Czech postings
    "pro", "nebo", "jako", "další", "zkušenost", "zkušenosti", "znalost", "znalosti", "práce",
    "vývoj", "služby",
}
_ALIASES = {
    "postgres": "postgresql", "k8s": "kubernetes", "js": "javascript", "ts": "typescript",
    "rest": "api", "apis": "api", "unit": "test", "testing": "test", "ci/cd": "ci",
    "message": "rabbitmq", "queues": "rabbitmq", "queue": "rabbitmq",
    "database": "sql", "databases": "sql", "databáze": "sql", "relational": "sql",
    "relační": "sql",
}


# Extraction sometimes returns soft skills despite the prompt; a CV cannot "lack" these.
_SOFT_SKILLS = {
    "accountability", "communication", "problem solving", "problem-solving", "teamwork",
    "team player", "ownership", "proactivity", "curiosity", "english", "software development",
    "komunikace", "samostatnost", "týmová spolupráce", "angličtina",
}


def _skill_words(skill: str) -> list[str]:
    words = re.findall(r"[\w+#/]+", skill.lower())
    return [_ALIASES.get(w, w) for w in words if len(w) >= 2 and w not in _STOPWORDS]


def _covers(cv_haystack: str, skill: str) -> bool:
    """True when the CV mentions the skill, or at least half of its meaningful words by
    4-letter stem ("async" matches "asyncio", "test" matches "pytest"). Half rather than all,
    because postings pad tools with descriptions: "docker a kontejnerizace",
    "message brokery (rabbitmq, sqs)"."""
    if _mentions(cv_haystack, skill):
        return True
    words = _skill_words(skill)
    hits = sum(w[:4] in cv_haystack for w in words)
    return bool(words) and hits * 2 >= len(words)


def compare(
    profile: CvProfile,
    job_stack: list[str],
    requirements_text: str,
    nice_to_have_text: str,
    years_required: int | None,
    remote_policy: str | None,
    office_city: str | None,
    required_skills: list[str] | None = None,
    nice_to_have_skills: list[str] | None = None,
    cv_text: str = "",
) -> Comparison:
    """Deterministic CV-to-job comparison.

    Uses the skills extracted per section at ingest when available; otherwise attributes the
    job's tech stack to sections by searching the section texts. A skill counts as covered
    when it is in the CV profile or mentioned anywhere in the CV text.
    """

    haystack = " ".join([cv_text.lower(), *profile.skills])

    def covered(skill: str) -> bool:
        return skill.lower() in profile.skills or _covers(haystack, skill)

    if required_skills or nice_to_have_skills:
        required = {s.lower() for s in required_skills or []} - _SOFT_SKILLS
        nice = {s.lower() for s in nice_to_have_skills or []} - required - _SOFT_SKILLS
    else:
        required = {t for t in job_stack if _mentions(requirements_text, t)}
        nice = {t for t in job_stack if t not in required and _mentions(nice_to_have_text, t)}
        # Stack items not attributable to a section count as required: safer to over-report.
        required |= set(job_stack) - required - nice

    if remote_policy and remote_policy.startswith("remote"):
        location_fit = "remote"
    elif office_city and profile.city and profile.city.lower() in office_city.lower():
        location_fit = "same_city"
    elif office_city:
        location_fit = "relocation_or_commute"
    else:
        location_fit = "unknown"

    gap = max(0.0, (years_required or 0) - profile.years_experience)
    return Comparison(
        matched_skills=sorted(s for s in required | nice if covered(s)),
        missing_required=sorted(s for s in required if not covered(s)),
        missing_nice_to_have=sorted(s for s in nice if not covered(s)),
        years_required=years_required,
        years_gap=round(gap, 1),
        location_fit=location_fit,
    )
