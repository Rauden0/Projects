"""OpenTelemetry spans for chat turns, LLM calls and tool calls.

Nothing is traced until `configure_tracing` gets an OTLP endpoint: the tracer is a no-op and the
SDK is never imported, so the core install needs only `opentelemetry-api`. The GenAI semantic
conventions are still in Development status, so every `gen_ai.*` name lives in this module.
Prompts, completions and tool arguments or results go on spans only with `capture_content`.
"""

import json
from collections.abc import AsyncIterator, Iterator
from contextlib import contextmanager
from dataclasses import dataclass
from typing import Protocol
from urllib.parse import urlsplit

from opentelemetry import context, trace
from opentelemetry.trace import NoOpTracer, Span, SpanKind, StatusCode, Tracer, TracerProvider
from opentelemetry.util.types import Attributes

from jobmatcher.errors import JobMatcherError, LLMError

# GenAI semantic conventions (Development status).
OPERATION = "gen_ai.operation.name"
PROVIDER = "gen_ai.provider.name"
AGENT_NAME = "gen_ai.agent.name"
REQUEST_MODEL = "gen_ai.request.model"
REQUEST_MAX_TOKENS = "gen_ai.request.max_tokens"
RESPONSE_MODEL = "gen_ai.response.model"
RESPONSE_ID = "gen_ai.response.id"
FINISH_REASONS = "gen_ai.response.finish_reasons"
INPUT_TOKENS = "gen_ai.usage.input_tokens"
OUTPUT_TOKENS = "gen_ai.usage.output_tokens"
CACHE_READ_TOKENS = "gen_ai.usage.cache_read.input_tokens"
CACHE_WRITE_TOKENS = "gen_ai.usage.cache_write.input_tokens"
SYSTEM_INSTRUCTIONS = "gen_ai.system_instructions"
INPUT_MESSAGES = "gen_ai.input.messages"
OUTPUT_MESSAGES = "gen_ai.output.messages"
TOOL_NAME = "gen_ai.tool.name"
TOOL_CALL_ID = "gen_ai.tool.call.id"
TOOL_TYPE = "gen_ai.tool.type"
TOOL_ARGUMENTS = "gen_ai.tool.call.arguments"
TOOL_RESULT = "gen_ai.tool.call.result"
# General conventions (Stable).
ERROR_TYPE = "error.type"
HTTP_STATUS = "http.response.status_code"
SERVER_ADDRESS = "server.address"
SERVER_PORT = "server.port"
# This project.
PURPOSE = "jobmatcher.purpose"
REQUEST_ID = "jobmatcher.request_id"
CHAT_HAS_CV = "jobmatcher.chat.has_cv"
CHAT_HISTORY_TURNS = "jobmatcher.chat.history_turns"
CHAT_OUTCOME = "jobmatcher.chat.outcome"
CHAT_LLM_CALLS = "jobmatcher.chat.llm_calls"
CHAT_TOOL_CALLS = "jobmatcher.chat.tool_calls"
TOOL_RESULT_COUNT = "jobmatcher.tool.result_count"

AGENT = "jobmatcher"
OTHER_ERROR = "_OTHER"
_OPENAI_COMPAT_HOSTS = {
    "api.groq.com": "groq",
    "api.openai.com": "openai",
    "generativelanguage.googleapis.com": "gcp.gemini",
}
_OLLAMA_PORT = 11434


@dataclass(frozen=True, slots=True)
class _Tracing:
    tracer: Tracer
    capture_content: bool


_tracing = _Tracing(NoOpTracer(), capture_content=False)


def configure_tracing(endpoint: str, *, service_name: str, capture_content: bool) -> None:
    """Export spans over OTLP/HTTP to `endpoint`; an empty endpoint leaves tracing off."""
    if not endpoint or not isinstance(trace.get_tracer_provider(), trace.ProxyTracerProvider):
        return
    provider = otlp_provider(endpoint, service_name)
    trace.set_tracer_provider(provider)
    install(provider, capture_content=capture_content)


def install(provider: TracerProvider, *, capture_content: bool = False) -> None:
    global _tracing
    _tracing = _Tracing(provider.get_tracer(AGENT), capture_content)


def otlp_provider(endpoint: str, service_name: str) -> TracerProvider:
    try:
        from opentelemetry.exporter.otlp.proto.http.trace_exporter import OTLPSpanExporter
        from opentelemetry.sdk.resources import SERVICE_NAME, Resource
        from opentelemetry.sdk.trace import TracerProvider as SdkTracerProvider
        from opentelemetry.sdk.trace.export import BatchSpanProcessor
    except ImportError as exc:
        raise ImportError(
            "OTEL_EXPORTER_OTLP_ENDPOINT is set but the OpenTelemetry SDK is missing: "
            "pip install 'jobmatcher[otel]'"
        ) from exc
    provider = SdkTracerProvider(resource=Resource.create({SERVICE_NAME: service_name}))
    provider.add_span_processor(BatchSpanProcessor(OTLPSpanExporter(endpoint=traces_url(endpoint))))
    return provider


def traces_url(endpoint: str) -> str:
    """The per-signal URL the exporter derives from OTEL_EXPORTER_OTLP_ENDPOINT itself, which it
    uses verbatim when the endpoint is passed in code."""
    return f"{endpoint.rstrip('/')}/v1/traces"


def provider_of(base_url: str) -> str:
    """`gen_ai.provider.name` for an OpenAI-compatible endpoint."""
    url = urlsplit(base_url)
    if url.port == _OLLAMA_PORT or url.hostname == "ollama":
        return "ollama"
    return _OPENAI_COMPAT_HOSTS.get(url.hostname or "", "openai_compat")


class Usage(Protocol):
    @property
    def input_tokens(self) -> int: ...
    @property
    def output_tokens(self) -> int: ...
    @property
    def cache_read_tokens(self) -> int: ...
    @property
    def cache_write_tokens(self) -> int: ...


class LlmSpan:
    def __init__(self, span: Span) -> None:
        self._span = span

    def prompt(self, messages: object, system: object = None) -> None:
        if not _tracing.capture_content:
            return
        if system is not None:
            self._span.set_attribute(SYSTEM_INSTRUCTIONS, _json(system))
        self._span.set_attribute(INPUT_MESSAGES, _json(messages))

    def response(
        self,
        usage: Usage,
        *,
        model: str | None,
        response_id: str | None,
        finish_reason: str | None,
        output: object,
    ) -> None:
        cached = usage.cache_read_tokens + usage.cache_write_tokens
        attributes: dict[str, str | int | list[str]] = {
            INPUT_TOKENS: usage.input_tokens + cached,
            OUTPUT_TOKENS: usage.output_tokens,
            CACHE_READ_TOKENS: usage.cache_read_tokens,
            CACHE_WRITE_TOKENS: usage.cache_write_tokens,
        }
        if model:
            attributes[RESPONSE_MODEL] = model
        if response_id:
            attributes[RESPONSE_ID] = response_id
        if finish_reason:
            attributes[FINISH_REASONS] = [finish_reason]
        if _tracing.capture_content:
            attributes[OUTPUT_MESSAGES] = _json(output)
        self._span.set_attributes(attributes)


@contextmanager
def llm_call(
    provider: str,
    model: str,
    *,
    purpose: str,
    max_tokens: int | None = None,
    base_url: str | None = None,
) -> Iterator[LlmSpan]:
    """A `chat {model}` span around one provider call, budget check and ledger write included,
    so a call refused by the budget shows up too."""
    attributes: dict[str, str | int] = {
        OPERATION: "chat",
        PROVIDER: provider,
        REQUEST_MODEL: model,
        PURPOSE: purpose,
    }
    if max_tokens is not None:
        attributes[REQUEST_MAX_TOKENS] = max_tokens
    if base_url:
        attributes.update(_server(base_url))
    with _span(f"chat {model}", SpanKind.CLIENT, attributes) as span:
        yield LlmSpan(span)


class ToolSpan:
    def __init__(self, span: Span) -> None:
        self._span = span

    def result(self, output: object, *, error: str | None) -> None:
        if isinstance(output, list):
            self._span.set_attribute(TOOL_RESULT_COUNT, len(output))
        if error is not None:
            self._span.set_attribute(ERROR_TYPE, error)
            self._span.set_status(StatusCode.ERROR, error)
        elif _tracing.capture_content:
            self._span.set_attribute(TOOL_RESULT, _json(output))


@contextmanager
def tool_call(name: str, call_id: str, arguments: object) -> Iterator[ToolSpan]:
    attributes: dict[str, str] = {
        OPERATION: "execute_tool",
        TOOL_NAME: name,
        TOOL_CALL_ID: call_id,
        TOOL_TYPE: "function",
    }
    if _tracing.capture_content:
        attributes[TOOL_ARGUMENTS] = _json(arguments)
    with _span(f"execute_tool {name}", SpanKind.INTERNAL, attributes) as span:
        yield ToolSpan(span)


def start_chat(*, request_id: str | None, has_cv: bool, history_turns: int) -> Span:
    """The root span of one chat turn. The caller ends it with `end_chat` and runs the agent
    through `within`, because the turn is an async generator that yields between steps."""
    attributes: dict[str, str | int | bool] = {
        OPERATION: "invoke_agent",
        AGENT_NAME: AGENT,
        CHAT_HAS_CV: has_cv,
        CHAT_HISTORY_TURNS: history_turns,
    }
    if request_id:
        attributes[REQUEST_ID] = request_id
    return _tracing.tracer.start_span("chat", kind=SpanKind.INTERNAL, attributes=attributes)


def end_chat(span: Span, *, outcome: str, llm_calls: int, tool_calls: int) -> None:
    span.set_attributes(
        {CHAT_OUTCOME: outcome, CHAT_LLM_CALLS: llm_calls, CHAT_TOOL_CALLS: tool_calls}
    )
    if outcome == "error":
        span.set_attribute(ERROR_TYPE, OTHER_ERROR)
        span.set_status(StatusCode.ERROR)
    span.end()


async def within[T](span: Span, events: AsyncIterator[T]) -> AsyncIterator[T]:
    """Iterate `events` with `span` as the current span while the producer runs, and only then:
    holding it across a `yield` would leak it into the consumer and detach in the wrong task
    when the stream is closed from elsewhere."""
    while True:
        token = context.attach(trace.set_span_in_context(span))
        try:
            item = await anext(events)
        except StopAsyncIteration:
            return
        finally:
            context.detach(token)
        yield item


@contextmanager
def _span(name: str, kind: SpanKind, attributes: Attributes) -> Iterator[Span]:
    # The SDK's own exception handling would put the message on the span, and provider error
    # messages can quote the prompt or the generated output.
    with _tracing.tracer.start_as_current_span(
        name,
        kind=kind,
        attributes=attributes,
        record_exception=False,
        set_status_on_exception=False,
    ) as span:
        try:
            yield span
        except JobMatcherError as exc:
            _fail(span, exc)
            raise


def _fail(span: Span, exc: JobMatcherError) -> None:
    error = type(exc).__name__
    span.set_attribute(ERROR_TYPE, error)
    if isinstance(exc, LLMError) and exc.status_code is not None:
        span.set_attribute(HTTP_STATUS, exc.status_code)
    span.set_status(StatusCode.ERROR, error)
    if _tracing.capture_content:
        span.record_exception(exc)


def _server(base_url: str) -> dict[str, str | int]:
    url = urlsplit(base_url)
    server: dict[str, str | int] = {SERVER_ADDRESS: url.hostname or ""}
    if url.port is not None:
        server[SERVER_PORT] = url.port
    return server


def _json(value: object) -> str:
    return json.dumps(value, ensure_ascii=False, default=_plain)


def _plain(value: object) -> object:
    """SDK response blocks are Pydantic models; anything else is shown as its repr."""
    dump = getattr(value, "model_dump", None)
    return dump() if callable(dump) else repr(value)
