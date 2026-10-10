import httpx
import pytest

from jobmatcher.sources import AshbyAdapter, GreenhouseAdapter, LeverAdapter, SourceConfig


def client_returning(payload: object) -> httpx.AsyncClient:
    transport = httpx.MockTransport(lambda request: httpx.Response(200, json=payload))
    return httpx.AsyncClient(transport=transport)


async def collect(adapter):  # type: ignore[no-untyped-def]
    return [p async for p in adapter.list_postings()]


@pytest.mark.asyncio
async def test_greenhouse_unescapes_and_filters_location() -> None:
    payload = {
        "jobs": [
            {
                "id": 1,
                "title": "Python Engineer",
                "absolute_url": "https://x/1",
                "location": {"name": "Brno, Czechia"},
                "content": "&lt;p&gt;Python&lt;/p&gt;",
            },
            {
                "id": 2,
                "title": "Go Engineer",
                "absolute_url": "https://x/2",
                "location": {"name": "Austin, TX"},
                "content": "",
            },
        ]
    }
    async with client_returning(payload) as client:
        adapter = GreenhouseAdapter(
            SourceConfig(kind="greenhouse", company="acme", location_filter="brno"), client
        )
        postings = await collect(adapter)

    assert [p.external_id for p in postings] == ["1"]
    assert postings[0].description_html == "<p>Python</p>"


@pytest.mark.asyncio
async def test_lever_marks_remote_and_joins_lists() -> None:
    payload = [
        {
            "id": "abc",
            "text": "Backend Engineer",
            "hostedUrl": "https://x/abc",
            "categories": {"location": "Prague"},
            "workplaceType": "remote",
            "description": "<p>About</p>",
            "lists": [{"text": "Requirements", "content": "<li>Python</li>"}],
            "additional": "<p>Benefits</p>",
        }
    ]
    async with client_returning(payload) as client:
        adapter = LeverAdapter(
            SourceConfig(kind="lever", company="acme", location_filter="remote"), client
        )
        [posting] = await collect(adapter)

    assert posting.location == "Prague (remote)"
    assert "<h3>Requirements</h3>" in posting.description_html


@pytest.mark.asyncio
async def test_ashby_without_filter_returns_all() -> None:
    payload = {
        "jobs": [
            {"id": "a", "title": "A", "jobUrl": "u", "location": "Berlin", "descriptionHtml": "x"},
            {"id": "b", "title": "B", "jobUrl": "u", "location": None, "isRemote": True},
        ]
    }
    async with client_returning(payload) as client:
        postings = await collect(AshbyAdapter(SourceConfig(kind="ashby", company="acme"), client))

    assert [p.location for p in postings] == ["Berlin", "(remote)"]
