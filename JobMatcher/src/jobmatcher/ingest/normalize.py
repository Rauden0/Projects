import hashlib
import re
from html.parser import HTMLParser

_BLOCK_TAGS = {"p", "div", "br", "li", "ul", "ol", "h1", "h2", "h3", "h4", "h5", "h6", "tr"}
_HEADING_TAGS = {"h1", "h2", "h3", "h4", "h5", "h6", "strong", "b"}


class _TextExtractor(HTMLParser):
    def __init__(self) -> None:
        super().__init__()
        self.parts: list[str] = []

    def handle_starttag(self, tag: str, attrs: list[tuple[str, str | None]]) -> None:
        if tag in _BLOCK_TAGS:
            self.parts.append("\n")
        if tag == "li":
            self.parts.append("- ")
        if tag in {"h1", "h2", "h3", "h4", "h5", "h6"}:
            self.parts.append("## ")

    def handle_endtag(self, tag: str) -> None:
        if tag in _BLOCK_TAGS or tag in _HEADING_TAGS - {"strong", "b"}:
            self.parts.append("\n")

    def handle_data(self, data: str) -> None:
        self.parts.append(data)


def html_to_text(html: str) -> str:
    """Convert posting HTML to plain text, keeping headings and list items on their own lines."""
    parser = _TextExtractor()
    parser.feed(html)
    text = "".join(parser.parts).replace("\xa0", " ")
    lines = (re.sub(r"[ \t]+", " ", line).strip() for line in text.splitlines())
    return re.sub(r"\n{3,}", "\n\n", "\n".join(lines)).strip()


def content_hash(title: str, location: str | None, text: str) -> bytes:
    """Stable hash of what matters for re-indexing; whitespace and case insensitive."""
    normalized = " ".join(f"{title}\n{location or ''}\n{text}".lower().split())
    return hashlib.sha256(normalized.encode()).digest()
