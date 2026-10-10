from typing import Any

import pytest

from jobmatcher.agent import loop
from jobmatcher.agent.loop import run_agent
from jobmatcher.agent.tools import ToolError


class ScriptedModel:
    """Returns predefined responses and records what it was sent."""

    def __init__(self, *responses: list[dict[str, Any]]) -> None:
        self.responses = list(responses)
        self.calls: list[dict[str, Any]] = []

    async def complete(self, **_: Any) -> str:
        raise NotImplementedError

    async def create_message(self, **kwargs: Any) -> dict[str, Any]:
        self.calls.append(kwargs)
        return {"content": self.responses.pop(0)}


class FakeToolbox:
    def __init__(self) -> None:
        self.seen_urls: set[str] = set()
        self.calls: list[tuple[str, dict[str, Any]]] = []

    @staticmethod
    def definitions() -> list[dict[str, Any]]:
        return [{"name": "search_jobs", "description": "", "input_schema": {}}]

    async def call(self, name: str, raw: dict[str, Any]) -> Any:
        self.calls.append((name, raw))
        if name == "get_job":
            raise ToolError("job 99 does not exist")
        self.seen_urls.add("https://jobs.example/1")
        return [{"job_id": 1, "url": "https://jobs.example/1"}]


def tool_use(id_: str, name: str, **input_: Any) -> dict[str, Any]:
    return {"type": "tool_use", "id": id_, "name": name, "input": input_}


async def collect(model: ScriptedModel, toolbox: FakeToolbox) -> list[loop.AgentEvent]:
    return [e async for e in run_agent(model, toolbox, "remote python?", [], "python, 2y")]  # type: ignore[arg-type]


@pytest.mark.asyncio
async def test_tool_round_then_answer() -> None:
    model = ScriptedModel(
        [tool_use("t1", "search_jobs", query="python", remote_policy=["remote_region"])],
        [{"type": "text", "text": "Try https://jobs.example/1 and https://made.up/2."}],
    )
    toolbox = FakeToolbox()
    events = await collect(model, toolbox)

    assert [e.type for e in events] == ["tool_call", "tool_result", "answer"]
    [(name, args)] = toolbox.calls
    assert name == "search_jobs" and args["remote_policy"] == ["remote_region"]
    assert events[-1].data["unverified_urls"] == ["https://made.up/2"]

    tool_result = model.calls[1]["messages"][-1]["content"][0]
    assert tool_result["tool_use_id"] == "t1" and tool_result["is_error"] is False


@pytest.mark.asyncio
async def test_tool_errors_are_returned_to_model() -> None:
    model = ScriptedModel(
        [tool_use("t1", "get_job", job_id=99)],
        [{"type": "text", "text": "That job does not exist."}],
    )
    events = await collect(model, FakeToolbox())
    assert events[1].data["is_error"] is True
    assert model.calls[1]["messages"][-1]["content"][0]["content"] == "job 99 does not exist"


@pytest.mark.asyncio
async def test_parallel_call_limit() -> None:
    model = ScriptedModel(
        [tool_use(f"t{i}", "search_jobs", query=str(i)) for i in range(5)],
        [{"type": "text", "text": "done"}],
    )
    toolbox = FakeToolbox()
    events = await collect(model, toolbox)
    assert len(toolbox.calls) == loop.MAX_PARALLEL_CALLS
    errors = [e for e in events if e.type == "tool_result" and e.data["is_error"]]
    assert len(errors) == 5 - loop.MAX_PARALLEL_CALLS


@pytest.mark.asyncio
async def test_last_round_disables_tools(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(loop, "MAX_ROUNDS", 1)
    model = ScriptedModel(
        [tool_use("t1", "search_jobs", query="python")],
        [{"type": "text", "text": "answer"}],
    )
    await collect(model, FakeToolbox())
    assert [c["tool_choice"] for c in model.calls] == [{"type": "auto"}, {"type": "none"}]


@pytest.mark.asyncio
async def test_llm_failure_becomes_error_event() -> None:
    class FailingModel(ScriptedModel):
        async def create_message(self, **kwargs: Any) -> dict[str, Any]:
            raise RuntimeError("400 from LLM")

    events = await collect(FailingModel(), FakeToolbox())
    assert [e.type for e in events] == ["error"]
    assert "400 from LLM" in events[0].data["message"]
