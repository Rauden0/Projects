import httpx
import pytest

from jobmatcher.sources import (
    AshbyAdapter,
    GreenhouseAdapter,
    LeverAdapter,
    SourceConfig,
    WorkdayAdapter,
)


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


@pytest.mark.asyncio
async def test_workday_filters_location_before_fetching_detail() -> None:
    requested: list[str] = []

    def handler(request: httpx.Request) -> httpx.Response:
        requested.append(f"{request.method} {request.url.path}")
        if request.method == "POST":
            return httpx.Response(
                200,
                json={
                    "jobPostings": [
                        {"externalPath": "/job/Brno/Python-Dev_R-1", "locationsText": "Brno"},
                        {"externalPath": "/job/Austin/Go-Dev_R-2", "locationsText": "Austin"},
                    ]
                },
            )
        return httpx.Response(
            200,
            json={
                "jobPostingInfo": {
                    "title": "Python Developer",
                    "jobReqId": "R-1",
                    "location": "Brno",
                    "jobDescription": "<p>Python</p>",
                    "externalUrl": "https://acme.wd3.myworkdayjobs.com/careers/job/R-1",
                }
            },
        )

    config = SourceConfig(
        kind="workday",
        company="acme",
        host="acme.wd3.myworkdayjobs.com",
        site="careers",
        search_text="Python",
        location_filter="brno",
    )
    async with httpx.AsyncClient(transport=httpx.MockTransport(handler)) as client:
        [posting] = await collect(WorkdayAdapter(config, client))

    assert posting.external_id == "R-1" and posting.location == "Brno"
    assert requested == [
        "POST /wday/cxs/acme/careers/jobs",
        "GET /wday/cxs/acme/careers/job/Brno/Python-Dev_R-1",
    ]
