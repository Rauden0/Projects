import json
import uuid
from collections.abc import AsyncIterator
from contextlib import asynccontextmanager
from typing import Any

import pytest

pytest.importorskip("mcp")

from mcp import Client  # noqa: E402

from jobmatcher.agent.tools import ToolError  # noqa: E402
from jobmatcher.mcp_server import build_server  # noqa: E402

CV_ID = uuid.UUID("00000000-0000-0000-0000-000000000001")


class FakeToolbox:
    def __init__(self, calls: list[tuple[uuid.UUID | None, str, dict[str, Any]]],
                 cv_id: uuid.UUID | None) -> None:
        self.calls = calls
        self.cv_id = cv_id

    async def call(self, name: str, raw: dict[str, Any]) -> Any:
        self.calls.append((self.cv_id, name, raw))
        if name == "get_job":
            raise ToolError("job 99 does not exist")
        if name == "compare_with_cv":
            return {"job_id": raw["job_id"], "missing_required": ["kubernetes"]}
        return [{"job_id": 1, "url": "https://jobs.example/1"}]


@pytest.fixture
def calls() -> list[tuple[uuid.UUID | None, str, dict[str, Any]]]:
    return []


@pytest.fixture
def server(calls: list[tuple[uuid.UUID | None, str, dict[str, Any]]]) -> Any:
    @asynccontextmanager
    async def factory(cv_id: uuid.UUID | None) -> AsyncIterator[Any]:
        yield FakeToolbox(calls, cv_id)

    return build_server(factory)


async def test_lists_the_agent_tools_as_read_only(server: Any) -> None:
    async with Client(server) as client:
        tools = {t.name: t for t in (await client.list_tools()).tools}

    assert set(tools) == {"search_jobs", "get_job", "compare_with_cv"}
    assert all(t.annotations and t.annotations.read_only_hint for t in tools.values())
    # Filter enums come from SearchFilters, so clients see the allowed values.
    assert "remote_global" in json.dumps(tools["search_jobs"].input_schema)


async def test_search_passes_query_and_filters_to_toolbox(server: Any, calls: list[Any]) -> None:
    async with Client(server) as client:
        result = await client.call_tool(
            "search_jobs",
            {"query": "python backend", "filters": {"city": "Brno"}, "cv_id": str(CV_ID)},
        )

    assert not result.is_error
    assert calls == [(CV_ID, "search_jobs", {"query": "python backend", "k": 5, "city": "Brno"})]
    assert "https://jobs.example/1" in json.dumps(result.structured_content)


async def test_compare_uses_given_cv(server: Any, calls: list[Any]) -> None:
    async with Client(server) as client:
        result = await client.call_tool("compare_with_cv", {"job_id": 7, "cv_id": str(CV_ID)})

    assert not result.is_error
    assert calls == [(CV_ID, "compare_with_cv", {"job_id": 7})]


async def test_tool_error_is_reported_not_raised(server: Any) -> None:
    async with Client(server) as client:
        result = await client.call_tool("get_job", {"job_id": 99})

    assert result.is_error
    assert "job 99 does not exist" in result.content[0].text
