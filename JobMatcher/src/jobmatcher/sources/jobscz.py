"""Jobs.cz, the largest Czech job board.

There is no public API, so this adapter reads the server-rendered search and detail pages.
It is written for personal use: it only visits paths allowed by robots.txt (``/prace/``,
``/rpd/``), sends one request at a time with a pause between them, and caps the number of
result pages. Ads that redirect to an employer's own JavaScript career site keep only the
title and card details, because their description is not in the HTML.
"""

import asyncio
import html
import logging
import re
from collections.abc import AsyncIterator
from html.parser import HTMLParser
from urllib.parse import urlsplit

from jobmatcher.sources.base import RawPosting, SourceAdapter

log = logging.getLogger(__name__)

BASE = "https://www.jobs.cz"
DEFAULT_MAX_PAGES = 5
REQUEST_DELAY_S = 1.0

_CARD_RE = re.compile(r'<article[^>]*class="SearchResultCard"[^>]*>(.*?)</article>', re.S)
_ID_RE = re.compile(r'data-jobad-id="(\d+)"')
_TITLE_RE = re.compile(r'SearchResultCard__titleLink"\s*>(.*?)</a>', re.S)
_FOOTER_ITEM_RE = re.compile(r'class="SearchResultCard__footerItem"\s*>(.*?)</li>', re.S)
_TAG_RE = re.compile(r'class="Tag[^"]*"\s*>(.*?)</span>', re.S)


def _text(fragment: str) -> str:
    return " ".join(html.unescape(re.sub(r"<[^>]+>", " ", fragment)).split())


def parse_search_page(page: str) -> list[dict[str, str]]:
    """Cards on a search results page: id, title, company, location, tags."""
    cards = []
    for match in _CARD_RE.finditer(page):
        card = match.group(1)
        ad_id = _ID_RE.search(card)
        title = _TITLE_RE.search(card)
        if not (ad_id and title):
            continue
        footer = [_text(item) for item in _FOOTER_ITEM_RE.findall(card)]
        cards.append(
            {
                "id": ad_id.group(1),
                "title": _text(title.group(1)),
                "company": footer[0] if footer else "",
                "location": footer[1] if len(footer) > 1 else "",
                "tags": ", ".join(_text(t) for t in _TAG_RE.findall(card)),
            }
        )
    return cards


class _RichTextExtractor(HTMLParser):
    """Collects the inner HTML of the element marked data-test="jd-body-richtext"."""

    def __init__(self) -> None:
        super().__init__(convert_charrefs=False)
        self.depth = 0
        self.parts: list[str] = []

    def handle_starttag(self, tag: str, attrs: list[tuple[str, str | None]]) -> None:
        if self.depth:
            self.depth += 1
            self.parts.append(self.get_starttag_text() or "")
        elif ("data-test", "jd-body-richtext") in attrs:
            self.depth = 1

    def handle_endtag(self, tag: str) -> None:
        if self.depth:
            self.depth -= 1
            if self.depth:
                self.parts.append(f"</{tag}>")

    def handle_data(self, data: str) -> None:
        if self.depth:
            self.parts.append(data)

    def handle_entityref(self, name: str) -> None:
        if self.depth:
            self.parts.append(f"&{name};")

    def handle_charref(self, name: str) -> None:
        if self.depth:
            self.parts.append(f"&#{name};")


def parse_detail_page(page: str) -> str | None:
    extractor = _RichTextExtractor()
    extractor.feed(page)
    body = "".join(extractor.parts).strip()
    return body or None


class JobsCzAdapter(SourceAdapter):
    kind = "jobscz"

    async def _get(self, url: str, **params: object) -> tuple[str, str]:
        await asyncio.sleep(REQUEST_DELAY_S)
        response = await self.client.get(url, params=params, follow_redirects=True)
        response.raise_for_status()
        return response.text, str(response.url)

    async def list_postings(self) -> AsyncIterator[RawPosting]:
        cfg = self.config
        locality = f"{cfg.locality.strip('/')}/" if cfg.locality else ""
        search_url = f"{BASE}/prace/{locality}"
        for page_no in range(1, (cfg.max_pages or DEFAULT_MAX_PAGES) + 1):
            page, _ = await self._get(search_url, **{"q[]": cfg.search_text or "", "page": page_no})
            cards = parse_search_page(page)
            if not cards:
                break
            for card in cards:
                if not self.wants_location(card["location"]):
                    continue
                yield await self._posting(card)

    async def _posting(self, card: dict[str, str]) -> RawPosting:
        detail_url = f"{BASE}/rpd/{card['id']}/"
        description: str | None = None
        url = detail_url
        try:
            page, final_url = await self._get(detail_url)
            if urlsplit(final_url).netloc == "www.jobs.cz":
                description = parse_detail_page(page)
                url = final_url.split("?")[0]
            # Employer career sites carry the ad id in the query string; the /rpd/ link
            # redirects there and stays stable, so it is the one stored.
        except Exception:
            log.warning("jobs.cz detail %s unavailable, keeping card only", card["id"])
        if description is None:
            description = f"<p>{html.escape(card['title'])}</p><p>{html.escape(card['tags'])}</p>"
        return RawPosting(
            kind=self.kind,
            company=card["company"] or "unknown",
            source=self.config.company,
            external_id=card["id"],
            url=url,
            title=card["title"],
            location=card["location"] or None,
            description_html=description,
        )
