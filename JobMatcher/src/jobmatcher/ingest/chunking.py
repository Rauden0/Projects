import re
from dataclasses import dataclass

# Rough token estimate: good enough to keep chunks well under the embedding model limit.
_WORDS_PER_CHUNK = 350

_SECTION_PATTERNS: list[tuple[str, re.Pattern[str]]] = [
    ("requirements", re.compile(r"require|qualif|you have|you bring|must have|what we expect|"
                                r"požad|co bys měl|očekáv", re.I)),
    ("nice_to_have", re.compile(r"nice to have|bonus|plus|preferred|výhod", re.I)),
    ("responsibilities", re.compile(r"responsib|what you.ll do|your role|day to day|náplň|"
                                    r"budeš", re.I)),
    ("benefits", re.compile(r"benefit|we offer|what we offer|perks|nabízíme", re.I)),
]


@dataclass(frozen=True)
class Chunk:
    ord: int
    section: str | None
    content: str


def _classify(heading: str) -> str | None:
    for name, pattern in _SECTION_PATTERNS:
        if pattern.search(heading):
            return name
    return None


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
            current_name = _classify(stripped) or current_name
            current = [stripped.removeprefix("## ")]
        else:
            current.append(line)
    if current:
        sections.append((current_name, "\n".join(current).strip()))
    return [(name, body) for name, body in sections if body]


def chunk_posting(title: str, company: str, text: str) -> list[Chunk]:
    """Split a posting by section, then by size; every chunk carries the title and company
    so it is self-explanatory when retrieved on its own."""
    header = f"{title} — {company}"
    chunks: list[Chunk] = []
    for section, body in _split_sections(text):
        words = body.split()
        for start in range(0, len(words), _WORDS_PER_CHUNK):
            piece = " ".join(words[start : start + _WORDS_PER_CHUNK])
            chunks.append(Chunk(ord=len(chunks), section=section, content=f"{header}\n{piece}"))
    if not chunks:
        chunks.append(Chunk(ord=0, section=None, content=header))
    return chunks
