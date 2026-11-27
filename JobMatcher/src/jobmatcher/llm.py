"""Thin wrapper over the LLM provider so the rest of the code (and tests) never import the SDK."""

import asyncio
import json
import re
from typing import Any, Protocol

import httpx

from jobmatcher.budget.guard import BudgetGuard, BudgetPolicy, Purpose
from jobmatcher.budget.ledger import UsageLedger
from jobmatcher.budget.pricing import TokenUsage
from jobmatcher.config import get_settings
from jobmatcher.db.session import get_sessionmaker

MAX_RATE_LIMIT_WAIT_S = 60.0


class LLMClient(Protocol):
    caches_prompts: bool

    async def complete(self, *, system: str, user: str, max_tokens: int = 1024) -> str: ...

    async def complete_json(
        self, *, system: str, user: str, schema: dict[str, Any], max_tokens: int = 512
    ) -> str: ...

    async def create_message(
        self,
        *,
        system: str,
        messages: list[dict[str, Any]],
        tools: list[dict[str, Any]],
        tool_choice: dict[str, str] | None = None,
        max_tokens: int = 4096,
    ) -> Any: ...


EPHEMERAL = {"type": "ephemeral"}


class AnthropicClient:
    caches_prompts = True

    def __init__(
        self,
        api_key: str,
        model: str,
        *,
        purpose: Purpose = "extraction",
        effort: str = "low",
        ledger: UsageLedger | None = None,
        guard: BudgetGuard | None = None,
        client_id: str | None = None,
        sdk: Any = None,
    ) -> None:
        if sdk is None:
            from anthropic import AsyncAnthropic

            sdk = AsyncAnthropic(api_key=api_key)
        self._client = sdk
        self.model = model
        self.purpose = purpose
        self.effort = effort
        self.ledger = ledger
        self.guard = guard
        self.client_id = client_id

    async def complete(self, *, system: str, user: str, max_tokens: int = 1024) -> str:
        response = await self._send(
            max_tokens=max_tokens,
            system=system,
            thinking={"type": "disabled"},
            output_config={"effort": self.effort},
            messages=[{"role": "user", "content": user}],
        )
        return _text(response)

    async def complete_json(
        self, *, system: str, user: str, schema: dict[str, Any], max_tokens: int = 512
    ) -> str:
        response = await self._send(
            max_tokens=max_tokens,
            system=system,
            thinking={"type": "disabled"},
            output_config={
                "effort": self.effort,
                "format": {"type": "json_schema", "schema": schema},
            },
            messages=[{"role": "user", "content": user}],
        )
        return _text(response)

    async def create_message(
        self,
        *,
        system: str,
        messages: list[dict[str, Any]],
        tools: list[dict[str, Any]],
        tool_choice: dict[str, str] | None = None,
        max_tokens: int = 4096,
    ) -> Any:
        return await self._send(
            max_tokens=max_tokens,
            system=[{"type": "text", "text": system, "cache_control": EPHEMERAL}],
            messages=messages,
            tools=_cached_tools(tools),
            tool_choice=tool_choice or {"type": "auto"},
            output_config={"effort": self.effort},
            cache_control=EPHEMERAL,
        )

    async def _send(self, **params: Any) -> Any:
        if self.guard is not None:
            await self.guard.check(self.purpose)
        response = await self._client.messages.create(model=self.model, **params)
        if self.ledger is not None:
            await self.ledger.record(
                purpose=self.purpose,
                model=self.model,
                usage=usage_of(response),
                client_id=self.client_id,
            )
        return response


def usage_of(response: Any) -> TokenUsage:
    usage = response.usage
    return TokenUsage(
        input_tokens=usage.input_tokens,
        output_tokens=usage.output_tokens,
        cache_read_tokens=getattr(usage, "cache_read_input_tokens", None) or 0,
        cache_write_tokens=getattr(usage, "cache_creation_input_tokens", None) or 0,
    )


def _text(response: Any) -> str:
    return "".join(block.text for block in response.content if block.type == "text")


def _cached_tools(tools: list[dict[str, Any]]) -> list[dict[str, Any]]:
    if not tools:
        return tools
    return [*tools[:-1], {**tools[-1], "cache_control": EPHEMERAL}]


class OpenAICompatClient:
    """Chat Completions client (Ollama, Groq, Gemini, ...).

    The rest of the code speaks Anthropic-style messages and content blocks; this class
    translates them to the OpenAI wire format and back, so the agent loop stays unchanged.
    """

    caches_prompts = False

    def __init__(self, base_url: str, model: str, api_key: str = "", timeout: float = 180) -> None:
        headers = {"Authorization": f"Bearer {api_key}"} if api_key else {}
        self._http = httpx.AsyncClient(base_url=base_url, headers=headers, timeout=timeout)
        self.model = model

    async def _chat(self, payload: dict[str, Any], attempts: int = 5) -> dict[str, Any]:
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
                delay = _retry_delay(response)
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

    async def complete_json(
        self, *, system: str, user: str, schema: dict[str, Any], max_tokens: int = 512
    ) -> str:
        instructions = f"{system}\nReturn only JSON matching this schema:\n{json.dumps(schema)}"
        return await self.complete(system=instructions, user=user, max_tokens=max_tokens)

    async def create_message(
        self,
        *,
        system: str,
        messages: list[dict[str, Any]],
        tools: list[dict[str, Any]],
        tool_choice: dict[str, str] | None = None,
        max_tokens: int = 4096,
    ) -> dict[str, Any]:
        payload: dict[str, Any] = {
            "max_tokens": max_tokens,
            "messages": [{"role": "system", "content": system}, *to_openai_messages(messages)],
        }
        # Some providers (Groq) reject a response that still calls a tool when tool_choice is
        # "none", so on the final round the tools are not offered at all.
        choice = (tool_choice or {"type": "auto"})["type"]
        if tools and choice != "none":
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
            payload["tool_choice"] = choice
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
            if texts:
                out.append({"role": "user", "content": "\n".join(texts)})
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


def get_llm(purpose: Purpose = "extraction", client_id: str | None = None) -> LLMClient | None:
    s = get_settings()
    provider, base_url, model = s.llm_provider, s.llm_base_url, s.llm_model
    api_key = s.llm_api_key.get_secret_value()
    if purpose == "chat":
        provider = s.chat_llm_provider or provider
        base_url = s.chat_llm_base_url or base_url
        api_key = s.chat_llm_api_key.get_secret_value() or api_key
        model = s.chat_llm_model or model

    if provider == "openai_compat":
        return OpenAICompatClient(base_url, model, api_key, s.llm_timeout_s)
    if not api_key:
        return None
    ledger = UsageLedger(get_sessionmaker())
    policy = BudgetPolicy(s.llm_monthly_budget_usd, s.llm_extraction_budget_share)
    return AnthropicClient(
        api_key,
        model,
        purpose=purpose,
        effort=s.llm_effort,
        ledger=ledger,
        guard=BudgetGuard(ledger, policy),
        client_id=client_id,
    )


def _retry_delay(response: httpx.Response) -> float:
    """Seconds to wait after a 429: the Retry-After header, else the 'try again in 19.8s'
    hint some providers only put in the message body, else 10."""
    header = response.headers.get("retry-after")
    if header:
        return float(header)
    match = re.search(r"try again in ([\d.]+)s", response.text)
    return float(match.group(1)) if match else 10.0
