"""Thin wrapper over the LLM provider so the rest of the code (and tests) never import the SDK."""

import asyncio
import json
from typing import Any, Literal, Protocol

import httpx

from jobmatcher.config import get_settings

MAX_RATE_LIMIT_WAIT_S = 30.0


class LLMClient(Protocol):
    async def complete(self, *, system: str, user: str, max_tokens: int = 1024) -> str: ...

    async def create_message(
        self,
        *,
        system: str,
        messages: list[dict[str, Any]],
        tools: list[dict[str, Any]],
        tool_choice: dict[str, str] | None = None,
        max_tokens: int = 2048,
    ) -> Any: ...


class AnthropicClient:
    def __init__(self, api_key: str, model: str) -> None:
        from anthropic import AsyncAnthropic  # optional dependency

        self._client = AsyncAnthropic(api_key=api_key)
        self.model = model

    async def complete(self, *, system: str, user: str, max_tokens: int = 1024) -> str:
        response = await self._client.messages.create(
            model=self.model,
            max_tokens=max_tokens,
            system=system,
            messages=[{"role": "user", "content": user}],
        )
        return "".join(block.text for block in response.content if block.type == "text")

    async def create_message(
        self,
        *,
        system: str,
        messages: list[dict[str, Any]],
        tools: list[dict[str, Any]],
        tool_choice: dict[str, str] | None = None,
        max_tokens: int = 2048,
    ) -> Any:
        return await self._client.messages.create(
            model=self.model,
            max_tokens=max_tokens,
            system=system,
            messages=messages,
            tools=tools,
            tool_choice=tool_choice or {"type": "auto"},
        )


class OpenAICompatClient:
    """Chat Completions client (Ollama, Groq, Gemini, ...).

    The rest of the code speaks Anthropic-style messages and content blocks; this class
    translates them to the OpenAI wire format and back, so the agent loop stays unchanged.
    """

    def __init__(self, base_url: str, model: str, api_key: str = "", timeout: float = 180) -> None:
        headers = {"Authorization": f"Bearer {api_key}"} if api_key else {}
        self._http = httpx.AsyncClient(base_url=base_url, headers=headers, timeout=timeout)
        self.model = model

    async def _chat(self, payload: dict[str, Any], attempts: int = 3) -> dict[str, Any]:
        for attempt in range(attempts):
            response = await self._http.post(
                "/chat/completions", json={"model": self.model, **payload}
            )
            # Groq validates generated tool arguments against the schema and answers 400
            # "tool_use_failed" when the model produced invalid ones; sampling again usually
            # succeeds.
            retryable = attempt < attempts - 1
            if response.status_code == 400 and "tool_use_failed" in response.text and retryable:
                continue
            # Free tiers have tight per-minute token limits; waiting is better than failing.
            if response.status_code == 429 and retryable:
                delay = float(response.headers.get("retry-after", "10"))
                if delay <= MAX_RATE_LIMIT_WAIT_S:
                    await asyncio.sleep(delay + 0.5)
                    continue
            if response.is_error:
                raise httpx.HTTPStatusError(
                    f"{response.status_code} from LLM: {response.text[:500]}",
                    request=response.request,
                    response=response,
                )
            break
        message: dict[str, Any] = response.json()["choices"][0]["message"]
        return message

    async def complete(self, *, system: str, user: str, max_tokens: int = 1024) -> str:
        message = await self._chat(
            {
                "max_tokens": max_tokens,
                "messages": [
                    {"role": "system", "content": system},
                    {"role": "user", "content": user},
                ],
            }
        )
        return str(message.get("content") or "")

    async def create_message(
        self,
        *,
        system: str,
        messages: list[dict[str, Any]],
        tools: list[dict[str, Any]],
        tool_choice: dict[str, str] | None = None,
        max_tokens: int = 2048,
    ) -> dict[str, Any]:
        payload: dict[str, Any] = {
            "max_tokens": max_tokens,
            "messages": [{"role": "system", "content": system}, *to_openai_messages(messages)],
        }
        if tools:
            payload["tools"] = [
                {
                    "type": "function",
                    "function": {
                        "name": t["name"],
                        "description": t["description"],
                        "parameters": t["input_schema"],
                    },
                }
                for t in tools
            ]
            payload["tool_choice"] = (tool_choice or {"type": "auto"})["type"]
        return from_openai_message(await self._chat(payload))


def to_openai_messages(messages: list[dict[str, Any]]) -> list[dict[str, Any]]:
    out: list[dict[str, Any]] = []
    for msg in messages:
        content = msg["content"]
        if isinstance(content, str):
            out.append({"role": msg["role"], "content": content})
            continue
        texts = [b["text"] for b in content if b["type"] == "text"]
        tool_uses = [b for b in content if b["type"] == "tool_use"]
        tool_results = [b for b in content if b["type"] == "tool_result"]
        if tool_results:
            out.extend(
                {"role": "tool", "tool_call_id": r["tool_use_id"], "content": r["content"]}
                for r in tool_results
            )
            continue
        entry: dict[str, Any] = {"role": msg["role"], "content": "\n".join(texts) or None}
        if tool_uses:
            entry["tool_calls"] = [
                {
                    "id": u["id"],
                    "type": "function",
                    "function": {"name": u["name"], "arguments": json.dumps(u["input"])},
                }
                for u in tool_uses
            ]
        out.append(entry)
    return out


def from_openai_message(message: dict[str, Any]) -> dict[str, Any]:
    blocks: list[dict[str, Any]] = []
    if message.get("content"):
        blocks.append({"type": "text", "text": message["content"]})
    for i, call in enumerate(message.get("tool_calls") or []):
        arguments = call["function"].get("arguments") or "{}"
        try:
            parsed = json.loads(arguments) if isinstance(arguments, str) else arguments
        except json.JSONDecodeError:
            parsed = {}
        blocks.append(
            {
                "type": "tool_use",
                # Some local servers omit ids; the loop needs one to pair results with calls.
                "id": call.get("id") or f"call_{i}",
                "name": call["function"]["name"],
                "input": parsed,
            }
        )
    return {"content": blocks}


def get_llm(purpose: Literal["extraction", "chat"] = "extraction") -> LLMClient | None:
    s = get_settings()
    provider, base_url, api_key, model = s.llm_provider, s.llm_base_url, s.llm_api_key, s.llm_model
    if purpose == "chat":
        provider = s.chat_llm_provider or provider
        base_url = s.chat_llm_base_url or base_url
        api_key = s.chat_llm_api_key or api_key
        model = s.chat_llm_model or model

    if provider == "openai_compat":
        return OpenAICompatClient(base_url, model, api_key, s.llm_timeout_s)
    if not api_key:
        return None
    return AnthropicClient(api_key, model)
