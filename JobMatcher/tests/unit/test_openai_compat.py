import json
from typing import Any

import httpx
import pytest

from jobmatcher.llm import OpenAICompatClient, from_openai_message, to_openai_messages


def test_messages_round_trip_tool_calls_and_results() -> None:
    messages = [
        {"role": "user", "content": "remote python?"},
        {
            "role": "assistant",
            "content": [
                {"type": "text", "text": "Searching."},
                {"type": "tool_use", "id": "t1", "name": "search_jobs", "input": {"query": "py"}},
            ],
        },
        {
            "role": "user",
            "content": [
                {"type": "tool_result", "tool_use_id": "t1", "content": "[]", "is_error": False}
            ],
        },
    ]
    assert to_openai_messages(messages) == [
        {"role": "user", "content": "remote python?"},
        {
            "role": "assistant",
            "content": "Searching.",
            "tool_calls": [
                {
                    "id": "t1",
                    "type": "function",
                    "function": {"name": "search_jobs", "arguments": '{"query": "py"}'},
                }
            ],
        },
        {"role": "tool", "tool_call_id": "t1", "content": "[]"},
    ]


def test_response_is_converted_to_content_blocks() -> None:
    response = from_openai_message(
        {
            "content": None,
            "tool_calls": [
                {"function": {"name": "get_job", "arguments": '{"job_id": 3}'}},
                {"id": "x", "function": {"name": "get_job", "arguments": "not json"}},
            ],
        }
    )
    assert response == {
        "content": [
            {"type": "tool_use", "id": "call_0", "name": "get_job", "input": {"job_id": 3}},
            {"type": "tool_use", "id": "x", "name": "get_job", "input": {}},
        ]
    }


@pytest.mark.asyncio
async def test_create_message_sends_tools_and_choice() -> None:
    sent: dict[str, Any] = {}

    def handler(request: httpx.Request) -> httpx.Response:
        sent.update(json.loads(request.content))
        return httpx.Response(200, json={"choices": [{"message": {"content": "hi"}}]})

    client = OpenAICompatClient("http://ollama/v1", "qwen2.5:7b")
    client._http = httpx.AsyncClient(
        base_url="http://ollama/v1", transport=httpx.MockTransport(handler)
    )
    result = await client.create_message(
        system="sys",
        messages=[{"role": "user", "content": "q"}],
        tools=[{"name": "search_jobs", "description": "d", "input_schema": {"type": "object"}}],
        tool_choice={"type": "auto"},
    )

    assert result == {"content": [{"type": "text", "text": "hi"}]}
    assert sent["model"] == "qwen2.5:7b"
    assert sent["messages"][0] == {"role": "system", "content": "sys"}
    assert sent["tools"][0]["function"]["parameters"] == {"type": "object"}
    assert sent["tool_choice"] == "auto"

    sent.clear()
    await client.create_message(
        system="sys",
        messages=[{"role": "user", "content": "q"}],
        tools=[{"name": "search_jobs", "description": "d", "input_schema": {}}],
        tool_choice={"type": "none"},
    )
    assert "tools" not in sent and "tool_choice" not in sent


def test_chat_overrides_only_the_chat_client(monkeypatch: pytest.MonkeyPatch) -> None:
    from jobmatcher import llm as llm_module
    from jobmatcher.config import Settings

    settings = Settings(
        llm_base_url="http://ollama:11434/v1",
        llm_model="qwen2.5:7b",
        chat_llm_base_url="https://api.groq.com/openai/v1",
        chat_llm_model="llama-3.3-70b-versatile",
        chat_llm_api_key="gsk_test",
    )
    monkeypatch.setattr(llm_module, "get_settings", lambda: settings)

    extraction = llm_module.get_llm("extraction")
    chat = llm_module.get_llm("chat")
    assert isinstance(extraction, OpenAICompatClient) and isinstance(chat, OpenAICompatClient)
    assert (extraction.model, str(extraction._http.base_url)) == (
        "qwen2.5:7b",
        "http://ollama:11434/v1/",
    )
    assert chat.model == "llama-3.3-70b-versatile"
    assert chat._http.headers["Authorization"] == "Bearer gsk_test"


@pytest.mark.asyncio
async def test_rate_limit_and_tool_use_failed_are_retried(monkeypatch: pytest.MonkeyPatch) -> None:
    from jobmatcher import llm as llm_module

    async def no_sleep(_: float) -> None:
        return None

    monkeypatch.setattr(llm_module.asyncio, "sleep", no_sleep)
    responses = [
        httpx.Response(429, headers={"retry-after": "2"}, json={"error": "rate"}),
        httpx.Response(400, json={"error": {"code": "tool_use_failed"}}),
        httpx.Response(200, json={"choices": [{"message": {"content": "ok"}}]}),
    ]
    client = OpenAICompatClient("http://groq/v1", "m")
    client._http = httpx.AsyncClient(
        base_url="http://groq/v1", transport=httpx.MockTransport(lambda _: responses.pop(0))
    )
    assert await client.complete(system="s", user="u") == "ok"
    assert responses == []


def test_retry_delay_reads_body_hint() -> None:
    from jobmatcher.llm import _retry_delay

    body = {"error": {"message": "Rate limit reached. Please try again in 19.79s. Upgrade"}}
    assert _retry_delay(httpx.Response(429, json=body)) == 19.79
    assert _retry_delay(httpx.Response(429, headers={"retry-after": "3"})) == 3.0
    assert _retry_delay(httpx.Response(429, json={})) == 10.0


def _client(handler: Any, **kwargs: Any) -> OpenAICompatClient:
    http = httpx.AsyncClient(base_url="http://ollama/v1", transport=httpx.MockTransport(handler))
    return OpenAICompatClient("http://ollama/v1", "qwen2.5:7b", http=http, **kwargs)


@pytest.mark.asyncio
async def test_complete_json_sends_schema_as_response_format() -> None:
    sent: list[dict[str, Any]] = []

    def handler(request: httpx.Request) -> httpx.Response:
        sent.append(json.loads(request.content))
        return httpx.Response(200, json={"choices": [{"message": {"content": "{}"}}]})

    schema = {"type": "object"}
    await _client(handler).complete_json(system="s", user="u", schema=schema)
    await _client(handler, structured_output=False).complete_json(
        system="s", user="u", schema=schema
    )

    assert sent[0]["response_format"]["json_schema"]["schema"] == schema
    assert "response_format" not in sent[1]


@pytest.mark.asyncio
async def test_usage_is_metered_and_recorded() -> None:
    from jobmatcher.budget.pricing import TokenUsage
    from jobmatcher.observability import UsageMeter

    records: list[dict[str, Any]] = []

    class Ledger:
        async def record(self, **entry: Any) -> float:
            records.append(entry)
            return 0.0

    body = {
        "choices": [{"message": {"content": "ok"}}],
        "usage": {"prompt_tokens": 120, "completion_tokens": 30},
    }
    meter = UsageMeter()
    client = _client(
        lambda _: httpx.Response(200, json=body),
        ledger=Ledger(),
        meter=meter,
        purpose="chat",
        client_id="c1",
    )
    await client.complete(system="s", user="u")

    assert (meter.input_tokens, meter.output_tokens) == (120, 30)
    assert meter.calls[0].ok and meter.calls[0].purpose == "chat"
    assert records == [
        {"purpose": "chat", "model": "qwen2.5:7b", "usage": TokenUsage(120, 30), "client_id": "c1"}
    ]


@pytest.mark.asyncio
@pytest.mark.parametrize(
    "response",
    [
        httpx.Response(500, text="boom"),
        httpx.Response(200, json={"choices": []}),
        httpx.Response(200, text="not json"),
    ],
)
async def test_provider_failures_become_llm_error(response: httpx.Response) -> None:
    from jobmatcher.errors import LLMError
    from jobmatcher.observability import UsageMeter

    meter = UsageMeter()
    with pytest.raises(LLMError):
        await _client(lambda _: response, meter=meter).complete(system="s", user="u")
    assert [c.ok for c in meter.calls] == [False]


@pytest.mark.asyncio
async def test_network_error_becomes_llm_error() -> None:
    from jobmatcher.errors import LLMError

    def handler(request: httpx.Request) -> httpx.Response:
        raise httpx.ConnectError("refused", request=request)

    with pytest.raises(LLMError):
        await _client(handler).complete(system="s", user="u")


@pytest.mark.asyncio
async def test_reasoning_effort_is_sent_only_when_set() -> None:
    sent: list[dict[str, Any]] = []

    def handler(request: httpx.Request) -> httpx.Response:
        sent.append(json.loads(request.content))
        return httpx.Response(200, json={"choices": [{"message": {"content": "ok"}}]})

    await _client(handler, reasoning_effort="low").complete(system="s", user="u")
    await _client(handler).complete(system="s", user="u")
    assert sent[0]["reasoning_effort"] == "low"
    assert "reasoning_effort" not in sent[1]
