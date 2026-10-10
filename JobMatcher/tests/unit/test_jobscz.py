import httpx
import pytest

from jobmatcher.sources import jobscz
from jobmatcher.sources.base import SourceConfig
from jobmatcher.sources.jobscz import JobsCzAdapter, parse_detail_page, parse_search_page

# Trimmed copies of the real markup (attributes and nesting kept, icons and styling dropped).
SEARCH_PAGE = """
<div class="Stack">
<article class="SearchResultCard">
  <header class="SearchResultCard__header">
    <h2 data-test-ad-title="Python Developer" class="SearchResultCard__title">
      <a data-jobad-id="2001" data-link="jd-detail"
         href="https://www.jobs.cz/rpd/2001/?searchId=x&amp;rps=233"
         class="link-primary SearchResultCard__titleLink">  Python Developer &amp; Data
      </a>
    </h2>
  </header>
  <div class="SearchResultCard__body">
    <span class="Tag Tag--neutral Tag--small Tag--subtle">Možnost práce z domova</span>
  </div>
  <footer class="SearchResultCard__footer"><ul class="SearchResultCard__footerList">
    <li class="SearchResultCard__footerItem"><svg></svg><span>Acme s.r.o.</span></li>
    <li class="SearchResultCard__footerItem"><svg></svg>Brno – Trnitá</li>
  </ul></footer>
</article>
<article class="SearchResultCard">
  <header><h2><a data-jobad-id="2002" class="link-primary SearchResultCard__titleLink">Java Dev</a>
  </h2></header>
  <footer><ul>
    <li class="SearchResultCard__footerItem">Bank</li>
    <li class="SearchResultCard__footerItem">Praha</li>
  </ul></footer>
</article>
</div>
"""

DETAIL_PAGE = """
<html><body><div class="JobDescriptionHeading"><h1>Python Developer</h1></div>
<div data-jobad="body" data-test="jd-body-richtext" class="RichContent mb-1400">
  <p class="typography">Hledáme <strong>Python</strong> vývojáře.</p>
  <ul><li>FastAPI</li><li>PostgreSQL&nbsp;16</li></ul>
</div>
<div class="JobDescriptionBenefits">Benefits</div>
</body></html>
"""


def test_parse_search_page() -> None:
    cards = parse_search_page(SEARCH_PAGE)
    assert cards[0] == {
        "id": "2001",
        "title": "Python Developer & Data",
        "company": "Acme s.r.o.",
        "location": "Brno – Trnitá",
        "tags": "Možnost práce z domova",
    }
    assert [c["id"] for c in cards] == ["2001", "2002"]


def test_parse_detail_page_returns_only_the_description() -> None:
    body = parse_detail_page(DETAIL_PAGE)
    assert body is not None
    assert "<strong>Python</strong>" in body and "<li>FastAPI</li>" in body
    assert "&nbsp;" in body
    assert "Benefits" not in body and "<h1>" not in body
    assert parse_detail_page("<html></html>") is None


@pytest.mark.asyncio
async def test_adapter_filters_location_and_falls_back_for_external_sites(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    monkeypatch.setattr(jobscz, "REQUEST_DELAY_S", 0)

    def handler(request: httpx.Request) -> httpx.Response:
        if request.url.path.startswith("/prace/"):
            page = request.url.params["page"]
            return httpx.Response(200, text=SEARCH_PAGE if page == "1" else "<html></html>")
        if request.url.path == "/rpd/2001/":
            return httpx.Response(200, text=DETAIL_PAGE)
        return httpx.Response(404)

    config = SourceConfig(
        kind="jobscz", company="python-brno", search_text="python", locality="brno",
        location_filter="brno",
    )
    async with httpx.AsyncClient(transport=httpx.MockTransport(handler)) as client:
        postings = [p async for p in JobsCzAdapter(config, client).list_postings()]

    [posting] = postings  # the Prague card is filtered out
    assert posting.company == "Acme s.r.o." and posting.source == "python-brno"
    assert posting.url == "https://www.jobs.cz/rpd/2001/"
    assert "FastAPI" in posting.description_html
