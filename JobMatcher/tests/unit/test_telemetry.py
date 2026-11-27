import json
import os
import subprocess
import sys
from collections.abc import AsyncIterator, Iterator
from contextlib import asynccontextmanager
from pathlib import Path
from types import SimpleNamespace
from typing import Any

import httpx
import pytest
from opentelemetry import trace
from opentelemetry.sdk.trace import ReadableSpan, TracerProvider
from opentelemetry.sdk.trace.export import SimpleSpanProcessor
from opentelemetry.sdk.trace.export.in_memory_span_exporter import InMemorySpanExporter
from opentelemetry.trace import SpanKind, StatusCode
from test_agent_loop import FakeToolbox, ScriptedModel, tool_use

from jobmatcher import telemetry
from jobmatcher.agent import session as chat_session
from jobmatcher.errors import LLMError
from jobmatcher.llm import AnthropicClient, OpenAICompatClient
from jobmatcher.observability import UsageMeter, request_id_var
from jobmatcher.search.embeddings import HashingEmbedder

SRC = Path(__file__).parents[2] / "src"
CV_TEXT = "Jana Nováková, zdravotní sestra, Brno"
POSTING_TEXT = "Hledáme řidiče do Ostravy"


def install(capture_content: bool) -> Iterator[InMemorySpanExporter]:
    exporter = InMemorySpanExporter()
    provider = TracerProvider()
    provider.add_span_processor(SimpleSpanProcessor(exporter))
    telemetry.install(provider, capture_content=capture_content)
    yield exporter
    telemetry.install(trace.NoOpTracerProvider())


@pytest.fixture
def spans() -> Iterator[InMemorySpanExporter]:
    yield from install(capture_content=False)


@pytest.fixture
def content_spans() -> Iterator[InMemorySpanExporter]:
    yield from install(capture_content=True)


class ScriptedSdk:
    """The part of the Anthropic SDK that AnthropicClient uses, answering from a ScriptedModel."""

    def __init__(self, model: ScriptedModel) -> None:
        self.messages = self
        self.model = model

    async def create(self, **params: Any) -> SimpleNamespace:
        reply = await self.model.create_message(**params)
        calls_tool = any(block["type"] == "tool_use" for block in reply["content"])
        return SimpleNamespace(
            id=f"msg_{len(self.model.calls)}",
            model="claude-haiku-5-5-20261001",
            stop_reason="tool_use" if calls_tool else "end_turn",
            content=reply["content"],
            usage=SimpleNamespace(
                input_tokens=100,
                output_tokens=20,
                cache_read_input_tokens=900,
                cache_creation_input_tokens=50,
            ),
        )


@asynccontextmanager
async def no_session() -> AsyncIterator[None]:
    yield None


async def chat_turn(
    monkeypatch: pytest.MonkeyPatch, model: ScriptedModel, message: str = "remote python?"
) -> tuple[list[str], list[bool]]:
    """Runs one turn through `chat_events` with a real AnthropicClient over a scripted SDK.
    Returns the event types and, per event, whether a span was current in the consumer."""
    monkeypatch.setattr(chat_session, "Toolbox", lambda *_: FakeToolbox())
    meter = UsageMeter()
    llm = AnthropicClient(
        "key", "claude-haiku-5-5", purpose="chat", meter=meter, sdk=ScriptedSdk(model)
    )
    events = chat_session.chat_events(
        llm,
        meter,
        no_session,  # type: ignore[arg-type]
        HashingEmbedder(8),
        None,
        message,
        [{"role": "user", "content": "hi"}, {"role": "assistant", "content": "hello"}],
        CV_TEXT,
    )
    types, span_was_current = [], []
    async for event in events:
        types.append(event.type)
        span_was_current.append(trace.get_current_span().get_span_context().is_valid)
    return types, span_was_current


def by_name(exporter: InMemorySpanExporter) -> dict[str, list[ReadableSpan]]:
    named: dict[str, list[ReadableSpan]] = {}
    for span in exporter.get_finished_spans():
        named.setdefault(span.name, []).append(span)
    return named


def attributes(span: ReadableSpan) -> dict[str, Any]:
    return dict(span.attributes or {})


def all_values(exporter: InMemorySpanExporter) -> str:
    finished = exporter.get_finished_spans()
    values = [json.dumps(attributes(s), ensure_ascii=False, default=str) for s in finished]
    values += [s.status.description or "" for s in finished]
    values += [e.name + json.dumps(dict(e.attributes or {})) for s in finished for e in s.events]
    return "\n".join(values)


def two_round_model() -> ScriptedModel:
    return ScriptedModel(
        [tool_use("t1", "search_jobs", query="python")],
        [{"type": "text", "text": "Try https://jobs.example/1"}],
    )


async def test_agent_turn_is_one_trace_with_llm_and_tool_spans_under_chat(
    monkeypatch: pytest.MonkeyPatch, spans: InMemorySpanExporter
) -> None:
    token = request_id_var.set("req-1")
    try:
        types, span_was_current = await chat_turn(monkeypatch, two_round_model())
    finally:
        request_id_var.reset(token)

    assert types == ["tool_call", "tool_result", "answer"]
    assert [s.name for s in spans.get_finished_spans()] == [
        "chat claude-haiku-5-5",
        "execute_tool search_jobs",
        "chat claude-haiku-5-5",
        "chat",
    ]
    named = by_name(spans)
    [root] = named["chat"]
    children = [*named["chat claude-haiku-5-5"], *named["execute_tool search_jobs"]]
    assert root.parent is None
    assert all(c.parent is not None and c.parent.span_id == root.context.span_id for c in children)
    assert {s.context.trace_id for s in spans.get_finished_spans()} == {root.context.trace_id}
    assert span_was_current == [False, False, False]
    assert attributes(root) == {
        "gen_ai.operation.name": "invoke_agent",
        "gen_ai.agent.name": "jobmatcher",
        "jobmatcher.request_id": "req-1",
        "jobmatcher.chat.has_cv": False,
        "jobmatcher.chat.history_turns": 2,
        "jobmatcher.chat.outcome": "answer",
        "jobmatcher.chat.llm_calls": 2,
        "jobmatcher.chat.tool_calls": 1,
    }
    assert root.status.status_code is StatusCode.UNSET


async def test_llm_span_carries_genai_request_and_usage_attributes(
    monkeypatch: pytest.MonkeyPatch, spans: InMemorySpanExporter
) -> None:
    await chat_turn(monkeypatch, two_round_model())

    first, second = by_name(spans)["chat claude-haiku-5-5"]
    assert first.kind is SpanKind.CLIENT
    assert attributes(first) == {
        "gen_ai.operation.name": "chat",
        "gen_ai.provider.name": "anthropic",
        "gen_ai.request.model": "claude-haiku-5-5",
        "gen_ai.request.max_tokens": 4096,
        "jobmatcher.purpose": "chat",
        "gen_ai.response.model": "claude-haiku-5-5-20261001",
        "gen_ai.response.id": "msg_1",
        "gen_ai.response.finish_reasons": ("tool_use",),
        "gen_ai.usage.input_tokens": 1050,
        "gen_ai.usage.output_tokens": 20,
        "gen_ai.usage.cache_read.input_tokens": 900,
        "gen_ai.usage.cache_write.input_tokens": 50,
    }
    assert attributes(second)["gen_ai.response.finish_reasons"] == ("end_turn",)


async def test_tool_span_names_the_call_and_marks_rejected_calls(
    monkeypatch: pytest.MonkeyPatch, spans: InMemorySpanExporter
) -> None:
    model = ScriptedModel(
        [tool_use(f"t{i}", "search_jobs", query=str(i)) for i in range(4)],
        [{"type": "text", "text": "done"}],
    )
    await chat_turn(monkeypatch, model)

    tools = by_name(spans)["execute_tool search_jobs"]
    assert [s.kind for s in tools] == [SpanKind.INTERNAL] * 4
    assert attributes(tools[0]) == {
        "gen_ai.operation.name": "execute_tool",
        "gen_ai.tool.name": "search_jobs",
        "gen_ai.tool.call.id": "t0",
        "gen_ai.tool.type": "function",
        "jobmatcher.tool.result_count": 1,
    }
    assert [s.status.status_code for s in tools] == [StatusCode.UNSET] * 3 + [StatusCode.ERROR]
    assert attributes(tools[3])["error.type"] == "too_many_calls"


async def test_tool_error_sets_error_type_without_the_message(
    monkeypatch: pytest.MonkeyPatch, spans: InMemorySpanExporter
) -> None:
    model = ScriptedModel(
        [tool_use("t1", "get_job", job_id=99)],
        [{"type": "text", "text": "That job does not exist."}],
    )
    await chat_turn(monkeypatch, model)

    [tool] = by_name(spans)["execute_tool get_job"]
    assert tool.status.status_code is StatusCode.ERROR
    assert attributes(tool)["error.type"] == "ToolError"
    assert "job 99" not in all_values(spans)


def failing_ollama(body: str) -> OpenAICompatClient:
    client = OpenAICompatClient("http://ollama:11434/v1", "qwen2.5:7b", purpose="chat")
    client._http = httpx.AsyncClient(
        base_url=client.base_url,
        transport=httpx.MockTransport(lambda _: httpx.Response(500, text=body)),
    )
    return client


async def test_failed_llm_call_sets_error_status_without_the_message(
    spans: InMemorySpanExporter,
) -> None:
    llm = failing_ollama(f"model crashed while reading: {CV_TEXT}")
    with pytest.raises(LLMError):
        await llm.complete(system="s", user=CV_TEXT)

    [span] = spans.get_finished_spans()
    assert span.name == "chat qwen2.5:7b"
    assert span.status.status_code is StatusCode.ERROR
    assert span.status.description == "LLMError"
    assert attributes(span) == {
        "gen_ai.operation.name": "chat",
        "gen_ai.provider.name": "ollama",
        "gen_ai.request.model": "qwen2.5:7b",
        "gen_ai.request.max_tokens": 1024,
        "jobmatcher.purpose": "chat",
        "server.address": "ollama",
        "server.port": 11434,
        "error.type": "LLMError",
        "http.response.status_code": 500,
    }
    assert span.events == ()
    assert CV_TEXT not in all_values(spans)


async def test_chat_span_is_an_error_when_the_agent_fails(
    monkeypatch: pytest.MonkeyPatch, spans: InMemorySpanExporter
) -> None:
    class FailingModel(ScriptedModel):
        async def create_message(self, **kwargs: Any) -> dict[str, Any]:
            raise LLMError("529 overloaded", 529)

    types, _ = await chat_turn(monkeypatch, FailingModel())

    assert types == ["error"]
    named = by_name(spans)
    [llm_span] = named["chat claude-haiku-5-5"]
    [root] = named["chat"]
    assert attributes(llm_span)["http.response.status_code"] == 529
    assert root.status.status_code is StatusCode.ERROR
    assert attributes(root)["jobmatcher.chat.outcome"] == "error"


async def test_openai_compat_usage_counts_cached_tokens_as_input(
    spans: InMemorySpanExporter,
) -> None:
    body = {
        "id": "chatcmpl-7",
        "model": "qwen2.5:7b-instruct",
        "choices": [{"message": {"content": "{}"}, "finish_reason": "stop"}],
        "usage": {
            "prompt_tokens": 120,
            "completion_tokens": 30,
            "prompt_tokens_details": {"cached_tokens": 20},
        },
    }
    client = OpenAICompatClient("https://api.groq.com/openai/v1", "openai/gpt-oss-120b")
    client._http = httpx.AsyncClient(
        base_url=client.base_url,
        transport=httpx.MockTransport(lambda _: httpx.Response(200, json=body)),
    )
    await client.complete_json(system="s", user=POSTING_TEXT, schema={"type": "object"})

    [span] = spans.get_finished_spans()
    assert {k: v for k, v in attributes(span).items() if not k.startswith("gen_ai.request")} == {
        "gen_ai.operation.name": "chat",
        "gen_ai.provider.name": "groq",
        "jobmatcher.purpose": "extraction",
        "server.address": "api.groq.com",
        "gen_ai.response.model": "qwen2.5:7b-instruct",
        "gen_ai.response.id": "chatcmpl-7",
        "gen_ai.response.finish_reasons": ("stop",),
        "gen_ai.usage.input_tokens": 120,
        "gen_ai.usage.output_tokens": 30,
        "gen_ai.usage.cache_read.input_tokens": 20,
        "gen_ai.usage.cache_write.input_tokens": 0,
    }


CONTENT_ATTRIBUTES = {
    "gen_ai.system_instructions",
    "gen_ai.input.messages",
    "gen_ai.output.messages",
    "gen_ai.tool.call.arguments",
    "gen_ai.tool.call.result",
}


async def test_spans_carry_no_content_by_default(
    monkeypatch: pytest.MonkeyPatch, spans: InMemorySpanExporter
) -> None:
    await chat_turn(monkeypatch, two_round_model(), message=f"{POSTING_TEXT}?")

    keys = {k for s in spans.get_finished_spans() for k in attributes(s)}
    assert keys.isdisjoint(CONTENT_ATTRIBUTES)
    text = all_values(spans)
    for content in (CV_TEXT, POSTING_TEXT, "jobs.example", "python"):
        assert content not in text


async def test_capture_content_puts_prompts_and_tool_io_on_spans(
    monkeypatch: pytest.MonkeyPatch, content_spans: InMemorySpanExporter
) -> None:
    await chat_turn(monkeypatch, two_round_model(), message=f"{POSTING_TEXT}?")

    named = by_name(content_spans)
    first_llm = attributes(named["chat claude-haiku-5-5"][0])
    [tool] = named["execute_tool search_jobs"]
    assert CV_TEXT in first_llm["gen_ai.system_instructions"]
    assert POSTING_TEXT in first_llm["gen_ai.input.messages"]
    assert json.loads(first_llm["gen_ai.output.messages"])[0]["name"] == "search_jobs"
    assert json.loads(attributes(tool)["gen_ai.tool.call.arguments"]) == {"query": "python"}
    assert "https://jobs.example/1" in attributes(tool)["gen_ai.tool.call.result"]


def test_spans_are_not_recorded_without_an_endpoint() -> None:
    telemetry.configure_tracing("", service_name="jobmatcher", capture_content=True)

    assert isinstance(trace.get_tracer_provider(), trace.ProxyTracerProvider)
    span = telemetry.start_chat(request_id="r", has_cv=True, history_turns=0)
    assert not span.is_recording()
    with telemetry.llm_call("ollama", "qwen2.5:7b", purpose="chat") as llm_span:
        llm_span.prompt([{"role": "user", "content": CV_TEXT}])
    with telemetry.tool_call("get_job", "t1", {"job_id": 1}) as tool_span:
        tool_span.result({"title": POSTING_TEXT}, error=None)
    telemetry.end_chat(span, outcome="answer", llm_calls=1, tool_calls=1)


def test_default_settings_import_no_sdk_and_trace_nothing(tmp_path: Path) -> None:
    script = """
import sys
from opentelemetry import trace
import jobmatcher.agent.session, jobmatcher.api.main
from jobmatcher import config, telemetry

config.configure_tracing()
with telemetry.llm_call("ollama", "qwen2.5:7b", purpose="chat"):
    pass
assert isinstance(trace.get_tracer_provider(), trace.ProxyTracerProvider)
loaded = sorted(m for m in sys.modules if m.startswith("opentelemetry.sdk"))
assert not loaded, loaded
"""
    env = {**os.environ, "PYTHONPATH": str(SRC), "OTEL_EXPORTER_OTLP_ENDPOINT": ""}
    result = subprocess.run(
        [sys.executable, "-c", script], cwd=tmp_path, env=env, capture_output=True, text=True
    )
    assert result.returncode == 0, result.stderr


def test_otlp_provider_exports_under_the_service_name() -> None:
    provider = telemetry.otlp_provider("http://phoenix:6006/", "jobmatcher-worker")
    try:
        assert isinstance(provider, TracerProvider)
        assert provider.resource.attributes["service.name"] == "jobmatcher-worker"
    finally:
        provider.shutdown()


def test_traces_url_appends_the_signal_path_like_the_env_variable_does() -> None:
    assert telemetry.traces_url("http://phoenix:6006") == "http://phoenix:6006/v1/traces"
    assert telemetry.traces_url("http://phoenix:6006/") == "http://phoenix:6006/v1/traces"


@pytest.mark.parametrize(
    ("base_url", "provider"),
    [
        ("http://localhost:11434/v1", "ollama"),
        ("http://ollama:11434/v1", "ollama"),
        ("http://host.docker.internal:11434/v1", "ollama"),
        ("https://api.groq.com/openai/v1", "groq"),
        ("https://generativelanguage.googleapis.com/v1beta/openai", "gcp.gemini"),
        ("https://api.openai.com/v1", "openai"),
        ("http://vllm.internal:8000/v1", "openai_compat"),
    ],
)
def test_provider_name_of_openai_compatible_endpoints(base_url: str, provider: str) -> None:
    assert telemetry.provider_of(base_url) == provider
