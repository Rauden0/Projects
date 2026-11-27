"""Thin wrapper over the LLM providers so the rest of the code (and tests) never import an SDK.

Every call is checked against the budget, timed, traced as a `chat {model}` span, logged as
`llm_call` with its token usage, written to the usage ledger and, when the caller passes one,
added to a `UsageMeter`. Provider failures surface as `LLMError`.
"""

import asyncio
import json
import logging
import re
from datetime import UTC, datetime
from email.utils import parsedate_to_datetime
from functools import lru_cache
from typing import Any, Protocol

import httpx

from jobmatcher import telemetry
from jobmatcher.budget.guard import BudgetGuard, BudgetPolicy, Purpose
from jobmatcher.budget.ledger import UsageLedger
from jobmatcher.budget.pricing import TokenUsage
from jobmatcher.config import get_settings
from jobmatcher.db.session import get_sessionmaker
from jobmatcher.errors import LLMError
from jobmatcher.observability import LlmCall, Stopwatch, UsageMeter

log = logging.getLogger(__name__)

MAX_RATE_LIMIT_WAIT_S = 60.0
EPHEMERAL = {"type": "ephemeral"}


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


class UsageRecorder(Protocol):
    async def record(
        self, *, purpose: str, model: str, usage: TokenUsage, client_id: str | None = None
    ) -> float: ...


class _Observed:
    def __init__(
        self,
        model: str,
        purpose: Purpose,
        ledger: UsageRecorder | None,
        guard: BudgetGuard | None,
        client_id: str | None,
        meter: UsageMeter | None,
    ) -> None:
        self.model = model
        self.purpose = purpose
        self.ledger = ledger
        self.guard = guard
        self.client_id = client_id
        self.meter = meter

    async def _before(self) -> None:
        if self.guard is not None:
            await self.guard.check(self.purpose)

    async def _after(self, watch: Stopwatch, usage: TokenUsage | None) -> None:
        tokens = usage or TokenUsage(0, 0)
        call = LlmCall(
            purpose=self.purpose,
            model=self.model,
            latency_s=watch.elapsed_s,
            input_tokens=tokens.input_tokens + tokens.cache_read_tokens + tokens.cache_write_tokens,
            output_tokens=tokens.output_tokens,
            ok=usage is not None,
        )
        latency_ms = round(call.latency_s * 1000)
        log.info(
            "llm_call purpose=%s model=%s ok=%s latency_ms=%d input_tokens=%d output_tokens=%d",
            call.purpose,
            call.model,
            call.ok,
            latency_ms,
            call.input_tokens,
            call.output_tokens,
            extra={
                "event": "llm_call",
                "purpose": call.purpose,
                "model": call.model,
                "ok": call.ok,
                "latency_ms": latency_ms,
                "input_tokens": call.input_tokens,
                "output_tokens": call.output_tokens,
            },
        )
        if self.meter is not None:
            self.meter.add(call)
        if usage is not None and self.ledger is not None:
            await self.ledger.record(
                purpose=self.purpose, model=self.model, usage=usage, client_id=self.client_id
            )


def _anthropic_errors() -> tuple[type[Exception], ...]:
    try:
        from anthropic import APIError
    except ImportError:
        return ()
    return (APIError,)


class AnthropicClient(_Observed):
    caches_prompts = True

    def __init__(
        self,
        api_key: str,
        model: str,
        *,
        purpose: Purpose = "extraction",
        effort: str = "low",
        ledger: UsageRecorder | None = None,
        guard: BudgetGuard | None = None,
        client_id: str | None = None,
        meter: UsageMeter | None = None,
        sdk: Any = None,
    ) -> None:
        super().__init__(model, purpose, ledger, guard, client_id, meter)
        if sdk is None:
            from anthropic import AsyncAnthropic

            sdk = AsyncAnthropic(api_key=api_key)
        self._client = sdk
        self.effort = effort
        self._errors = _anthropic_errors()

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
        with telemetry.llm_call(
            "anthropic", self.model, purpose=self.purpose, max_tokens=params.get("max_tokens")
        ) as span:
            await self._before()
            span.prompt(params["messages"], system=params.get("system"))
            watch = Stopwatch()
            try:
                response = await self._client.messages.create(model=self.model, **params)
            except self._errors as exc:
                await self._after(watch, None)
                status = getattr(exc, "status_code", None)
                raise LLMError(f"anthropic: {exc}"[:500], status) from exc
            usage = usage_of(response)
            span.response(
                usage,
                model=getattr(response, "model", None),
                response_id=getattr(response, "id", None),
                finish_reason=getattr(response, "stop_reason", None),
                output=response.content,
            )
            await self._after(watch, usage)
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


class OpenAICompatClient(_Observed):
    """Chat Completions client (Ollama, Groq, Gemini, ...).

    The rest of the code speaks Anthropic-style messages and content blocks; this class
    translates them to the OpenAI wire format and back, so the agent loop stays unchanged.
    """

    caches_prompts = False

    def __init__(
        self,
        base_url: str,
        model: str,
        api_key: str = "",
        timeout: float = 180,
        *,
        purpose: Purpose = "extraction",
        structured_output: bool = True,
        reasoning_effort: str = "",
        ledger: UsageRecorder | None = None,
        guard: BudgetGuard | None = None,
        client_id: str | None = None,
        meter: UsageMeter | None = None,
        http: httpx.AsyncClient | None = None,
    ) -> None:
        super().__init__(model, purpose, ledger, guard, client_id, meter)
        self.base_url = base_url
        self._http = http or _http_client(base_url, api_key, timeout)
        self.structured_output = structured_output
        self.reasoning_effort = reasoning_effort

    async def _chat(self, payload: dict[str, Any], attempts: int = 5) -> dict[str, Any]:
        with telemetry.llm_call(
            telemetry.provider_of(self.base_url),
            self.model,
            purpose=self.purpose,
            max_tokens=payload.get("max_tokens"),
            base_url=self.base_url,
        ) as span:
            await self._before()
            span.prompt(payload["messages"])
            watch = Stopwatch()
            try:
                extra = {"reasoning_effort": self.reasoning_effort} if self.reasoning_effort else {}
                body = await self._post_with_retries(
                    {"model": self.model, **extra, **payload}, attempts
                )
                choice: dict[str, Any] = body["choices"][0]
                message: dict[str, Any] = choice["message"]
            except LLMError:
                await self._after(watch, None)
                raise
            except (httpx.HTTPError, ValueError, KeyError, IndexError, TypeError) as exc:
                await self._after(watch, None)
                raise LLMError(f"{type(exc).__name__} from LLM: {exc}"[:500]) from exc
            usage = _openai_usage(body)
            span.response(
                usage,
                model=body.get("model"),
                response_id=body.get("id"),
                finish_reason=choice.get("finish_reason"),
                output=[message],
            )
            await self._after(watch, usage)
        return message

    async def _post_with_retries(self, payload: dict[str, Any], attempts: int) -> Any:
        for attempt in range(attempts):
            response = await self._http.post("/chat/completions", json=payload)
            retryable = attempt < attempts - 1
            # Groq validates generated tool arguments against the schema and answers 400
            # "tool_use_failed" when the model produced invalid ones; sampling again usually
            # succeeds.
            if response.status_code == 400 and "tool_use_failed" in response.text and retryable:
                continue
            # Free tiers have tight per-minute token limits; waiting is better than failing.
            if response.status_code == 429 and retryable:
                delay = _retry_delay(response)
                if delay <= MAX_RATE_LIMIT_WAIT_S:
                    await asyncio.sleep(delay + 0.5)
                    continue
            if response.is_error:
                raise LLMError(
                    f"{response.status_code} from LLM: {response.text[:500]}",
                    response.status_code,
                )
            return response.json()
        raise LLMError(f"no answer after {attempts} attempts")

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
        payload: dict[str, Any] = {
            "max_tokens": max_tokens,
            "messages": [
                {"role": "system", "content": instructions},
                {"role": "user", "content": user},
            ],
        }
        if self.structured_output:
            payload["response_format"] = {
                "type": "json_schema",
                "json_schema": {"name": "output", "schema": schema},
            }
        message = await self._chat(payload)
        return str(message.get("content") or "")

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


@lru_cache
def _http_client(base_url: str, api_key: str, timeout: float) -> httpx.AsyncClient:
    headers = {"Authorization": f"Bearer {api_key}"} if api_key else {}
    return httpx.AsyncClient(base_url=base_url, headers=headers, timeout=timeout)


def _openai_usage(body: dict[str, Any]) -> TokenUsage:
    usage = body.get("usage") or {}
    cached = int((usage.get("prompt_tokens_details") or {}).get("cached_tokens") or 0)
    return TokenUsage(
        input_tokens=int(usage.get("prompt_tokens") or 0) - cached,
        output_tokens=int(usage.get("completion_tokens") or 0),
        cache_read_tokens=cached,
    )


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


def get_llm(
    purpose: Purpose = "extraction",
    client_id: str | None = None,
    meter: UsageMeter | None = None,
) -> LLMClient | None:
    """The configured client for a purpose, or None when Anthropic has no API key."""
    s = get_settings()
    provider, base_url, model = s.llm_provider, s.llm_base_url, s.llm_model
    api_key = s.llm_api_key.get_secret_value()
    if purpose == "chat":
        provider = s.chat_llm_provider or provider
        base_url = s.chat_llm_base_url or base_url
        api_key = s.chat_llm_api_key.get_secret_value() or api_key
        model = s.chat_llm_model or model

    ledger = UsageLedger(get_sessionmaker())
    guard = BudgetGuard(
        ledger, BudgetPolicy(s.llm_monthly_budget_usd, s.llm_extraction_budget_share)
    )
    if provider == "openai_compat":
        return OpenAICompatClient(
            base_url,
            model,
            api_key,
            s.llm_timeout_s,
            purpose=purpose,
            structured_output=s.llm_structured_output,
            reasoning_effort=s.llm_reasoning_effort,
            ledger=ledger,
            guard=guard,
            client_id=client_id,
            meter=meter,
        )
    if not api_key:
        return None
    return AnthropicClient(
        api_key,
        model,
        purpose=purpose,
        effort=s.llm_effort,
        ledger=ledger,
        guard=guard,
        client_id=client_id,
        meter=meter,
    )


def _retry_delay(response: httpx.Response) -> float:
    """Seconds to wait after a 429: the Retry-After header, else the 'try again in 19.8s'
    hint some providers only put in the message body, else 10."""
    header = _retry_after_s(response.headers.get("retry-after", ""))
    if header is not None:
        return header
    match = re.search(r"try again in ([\d.]+)s", response.text)
    return float(match.group(1)) if match else 10.0


def _retry_after_s(value: str) -> float | None:
    """Retry-After is either delay-seconds or an HTTP date."""
    if not value:
        return None
    try:
        return max(0.0, float(value))
    except ValueError:
        pass
    try:
        when = parsedate_to_datetime(value)
    except (TypeError, ValueError):
        return None
    if when.tzinfo is None:
        when = when.replace(tzinfo=UTC)
    return max(0.0, (when - datetime.now(UTC)).total_seconds())
