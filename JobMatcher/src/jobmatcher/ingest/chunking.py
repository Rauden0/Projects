import re

from jobmatcher.domain import Chunk, JobFacts
from jobmatcher.ingest.normalize import (
    boilerplate_facts,
    is_boilerplate_heading,
    is_boilerplate_line,
)

# Rough token estimate: good enough to keep chunks well under the embedding model limit.
_WORDS_PER_CHUNK = 350

_SECTION_PATTERNS: list[tuple[str, re.Pattern[str]]] = [
    (
        "requirements",
        re.compile(
            r"require|qualif|you have|you bring|must have|what we expect|"
            r"požad|co bys měl|očekáv",
            re.I,
        ),
    ),
    ("nice_to_have", re.compile(r"nice to have|bonus|plus|preferred|výhod", re.I)),
    (
        "responsibilities",
        re.compile(
            r"responsib|what you.ll do|your role|day to day|náplň|"
            r"budeš",
            re.I,
        ),
    ),
    ("benefits", re.compile(r"benefit|we offer|what we offer|perks|nabízíme", re.I)),
]


# Benefits, company blurbs and GDPR text match almost any query and say nothing about the
# job, so they are not embedded; only their pay and work-mode lines are kept.
_BOILERPLATE_SECTIONS = {"benefits", "boilerplate"}


def _classify(heading: str) -> str | None:
    for name, pattern in _SECTION_PATTERNS:
        if pattern.search(heading):
            return name
    return "boilerplate" if is_boilerplate_heading(heading) else None


def _carried(section: str | None) -> str | None:
    """An unrecognised heading continues the section, unless that would drop it as
    boilerplate: better to embed some benefits than lose a requirements list."""
    return None if section in _BOILERPLATE_SECTIONS else section


def _searchable(section: str | None, body: str) -> str:
    lines = [line for line in body.splitlines() if not is_boilerplate_line(line)]
    if section in _BOILERPLATE_SECTIONS:
        lines = boilerplate_facts(lines)
    return "\n".join(lines).strip()


def _split_sections(text: str) -> list[tuple[str | None, str]]:
    sections: list[tuple[str | None, str]] = []
    current_name: str | None = "about"
    current: list[str] = []
    for line in text.splitlines():
        stripped = line.strip()
        is_heading = stripped.startswith("## ") or (
            0 < len(stripped) <= 60 and stripped.endswith(":")
        )
        if is_heading:
            if current:
                sections.append((current_name, "\n".join(current).strip()))
            current_name = _classify(stripped) or _carried(current_name)
            current = [stripped.removeprefix("## ")]
        else:
            current.append(line)
    if current:
        sections.append((current_name, "\n".join(current).strip()))
    return [(name, body) for name, body in sections if body]


def chunk_posting(title: str, company: str, text: str) -> list[Chunk]:
    """Split a posting by section, then by size, leaving out boilerplate; every chunk carries
    the title and company so it is self-explanatory when retrieved on its own."""
    header = f"{title} — {company}"
    chunks: list[Chunk] = []
    for section, body in _split_sections(text):
        words = _searchable(section, body).split()
        for start in range(0, len(words), _WORDS_PER_CHUNK):
            piece = " ".join(words[start : start + _WORDS_PER_CHUNK])
            chunks.append(Chunk(ord=len(chunks), section=section, content=f"{header}\n{piece}"))
    if not chunks:
        chunks.append(Chunk(ord=0, section=None, content=header))
    return chunks


PROFILE_SECTION = "profile"


def profile_chunk(title: str, company: str, facts: JobFacts, ord: int) -> Chunk | None:
    """A short synthetic chunk of what the job is (title, seniority, skills): it matches
    "senior python backend" queries that long prose chunks dilute."""
    skills = list(dict.fromkeys([*facts.required_skills, *facts.tech_stack]))
    traits = [t for t in (facts.seniority, facts.remote_policy, facts.office_city) if t]
    if not (skills or traits):
        return None
    lines = [f"{title} — {company}", ", ".join(traits), ", ".join(skills)]
    return Chunk(ord=ord, section=PROFILE_SECTION, content="\n".join(x for x in lines if x))
