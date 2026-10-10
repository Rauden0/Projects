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
        tool_choice={"type": "none"},
    )

    assert result == {"content": [{"type": "text", "text": "hi"}]}
    assert sent["model"] == "qwen2.5:7b"
    assert sent["messages"][0] == {"role": "system", "content": "sys"}
    assert sent["tools"][0]["function"]["parameters"] == {"type": "object"}
    assert sent["tool_choice"] == "none"


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
        "qwen2.5:7b", "http://ollama:11434/v1/"
    )
    assert chat.model == "llama-3.3-70b-versatile"
    assert chat._http.headers["Authorization"] == "Bearer gsk_test"
