"""Prometheus metrics for the API, the worker and the outbox relay.

`prometheus_client` is optional (the `metrics` extra). Without it every recorder here is a
no-op, the API has no `/metrics` route and `serve_from_env` does nothing, so the core install
keeps working. Label values are bounded: routes are templates, models and purposes come from
configuration, outcomes from fixed sets.
"""

import logging
import os
from datetime import UTC, datetime
from typing import Protocol

from fastapi import FastAPI
from starlette.requests import Request
from starlette.responses import Response
from starlette.types import ASGIApp, Message, Receive, Scope, Send

from jobmatcher.observability import Stopwatch

log = logging.getLogger(__name__)

PORT_ENV = "METRICS_PORT"
PATH = "/metrics"
UNMATCHED_ROUTE = "<unmatched>"
_HTTP_BUCKETS = (0.01, 0.025, 0.05, 0.1, 0.2, 0.3, 0.5, 1.0, 2.0, 5.0, 10.0, 20.0, 30.0, 60.0)
_LLM_BUCKETS = (0.1, 0.25, 0.5, 1.0, 2.0, 5.0, 10.0, 20.0, 30.0, 60.0, 120.0)


class _Inc(Protocol):
    def inc(self, amount: float = 1) -> None: ...


class _Observe(Protocol):
    def observe(self, amount: float) -> None: ...


class _Set(Protocol):
    def set(self, value: float) -> None: ...


class _Labeled[T](Protocol):
    def labels(self, *labelvalues: str) -> T: ...


class Recorder:
    """The no-op recorder used when prometheus_client is not installed."""

    enabled = False

    def http_request(
        self, method: str, route: str, status: int, duration_s: float, first_byte_s: float | None
    ) -> None:
        pass

    def chat_turn(self, outcome: str) -> None:
        pass

    def llm_call(
        self,
        *,
        provider: str,
        model: str,
        purpose: str,
        outcome: str,
        duration_s: float,
        input_tokens: int,
        output_tokens: int,
        cost_usd: float,
    ) -> None:
        pass

    def ingest_message(self, outcome: str) -> None:
        pass

    def cache_lookup(self, cache: str, hit: bool) -> None:
        pass

    def rejected(self, reason: str) -> None:
        pass

    def outbox_batch(self, published: int, oldest_created_at: datetime | None) -> None:
        pass

    def render(self) -> tuple[bytes, str]:
        return b"", "text/plain"

    def serve(self, port: int) -> None:
        pass


class _Prometheus(Recorder):
    enabled = True

    def __init__(self) -> None:
        import prometheus_client as prom

        self._prom = prom
        self._http_duration: _Labeled[_Observe] = prom.Histogram(
            "jobmatcher_http_request_duration_seconds",
            "HTTP request duration until the last body byte (an SSE stream until it ends)",
            ["method", "route", "status"],
            buckets=_HTTP_BUCKETS,
        )
        self._http_first_byte: _Labeled[_Observe] = prom.Histogram(
            "jobmatcher_http_first_byte_seconds",
            "Time to the first body byte (for /chat: the first SSE event)",
            ["method", "route"],
            buckets=_HTTP_BUCKETS,
        )
        self._chat_turns: _Labeled[_Inc] = prom.Counter(
            "jobmatcher_chat_turns", "Chat turns by outcome", ["outcome"]
        )
        self._llm_calls: _Labeled[_Inc] = prom.Counter(
            "jobmatcher_llm_calls", "LLM calls", ["purpose", "model", "provider", "outcome"]
        )
        self._llm_duration: _Labeled[_Observe] = prom.Histogram(
            "jobmatcher_llm_call_duration_seconds",
            "LLM call duration, budget check and ledger write included",
            ["purpose", "model"],
            buckets=_LLM_BUCKETS,
        )
        self._llm_tokens: _Labeled[_Inc] = prom.Counter(
            "jobmatcher_llm_tokens",
            "LLM tokens (input includes cached)",
            ["purpose", "model", "kind"],
        )
        self._llm_cost: _Labeled[_Inc] = prom.Counter(
            "jobmatcher_llm_cost_usd", "LLM cost at list prices", ["purpose", "model"]
        )
        self._ingest: _Labeled[_Inc] = prom.Counter(
            "jobmatcher_ingest_messages", "job.discovered messages by outcome", ["outcome"]
        )
        self._cache: _Labeled[_Inc] = prom.Counter(
            "jobmatcher_cache_lookups", "Cache lookups by cache and result", ["cache", "result"]
        )
        self._rejected: _Labeled[_Inc] = prom.Counter(
            "jobmatcher_rejected_requests", "Requests refused by an abuse guard", ["reason"]
        )
        self._outbox_published: _Inc = prom.Counter(
            "jobmatcher_outbox_published", "Outbox rows published to RabbitMQ"
        )
        self._outbox_lag: _Set = prom.Gauge(
            "jobmatcher_outbox_lag_seconds",
            "Age of the oldest unpublished outbox row at the last relay batch (0 when empty)",
        )

    def http_request(
        self, method: str, route: str, status: int, duration_s: float, first_byte_s: float | None
    ) -> None:
        self._http_duration.labels(method, route, str(status)).observe(duration_s)
        if first_byte_s is not None:
            self._http_first_byte.labels(method, route).observe(first_byte_s)

    def chat_turn(self, outcome: str) -> None:
        self._chat_turns.labels(outcome).inc()

    def llm_call(
        self,
        *,
        provider: str,
        model: str,
        purpose: str,
        outcome: str,
        duration_s: float,
        input_tokens: int,
        output_tokens: int,
        cost_usd: float,
    ) -> None:
        self._llm_calls.labels(purpose, model, provider, outcome).inc()
        self._llm_duration.labels(purpose, model).observe(duration_s)
        self._llm_tokens.labels(purpose, model, "input").inc(input_tokens)
        self._llm_tokens.labels(purpose, model, "output").inc(output_tokens)
        self._llm_cost.labels(purpose, model).inc(cost_usd)

    def ingest_message(self, outcome: str) -> None:
        self._ingest.labels(outcome).inc()

    def cache_lookup(self, cache: str, hit: bool) -> None:
        self._cache.labels(cache, "hit" if hit else "miss").inc()

    def rejected(self, reason: str) -> None:
        self._rejected.labels(reason).inc()

    def outbox_batch(self, published: int, oldest_created_at: datetime | None) -> None:
        self._outbox_published.inc(published)
        lag = 0.0 if oldest_created_at is None else _age_s(oldest_created_at)
        self._outbox_lag.set(lag)

    def render(self) -> tuple[bytes, str]:
        return self._prom.generate_latest(), self._prom.CONTENT_TYPE_LATEST

    def serve(self, port: int) -> None:
        self._prom.start_http_server(port)


def _age_s(created_at: datetime) -> float:
    return max(0.0, (datetime.now(UTC) - created_at).total_seconds())


def _build() -> Recorder:
    try:
        return _Prometheus()
    except ImportError:
        return Recorder()


recorder = _build()


def serve_from_env() -> None:
    """Starts a scrape endpoint on METRICS_PORT for processes without an HTTP server."""
    port = os.environ.get(PORT_ENV, "")
    if not port or not recorder.enabled:
        return
    recorder.serve(int(port))
    log.info("metrics on :%s%s", port, PATH)


def instrument(app: FastAPI) -> None:
    """Adds the request metrics middleware and `/metrics` when prometheus_client is installed."""
    if not recorder.enabled:
        return
    app.add_middleware(MetricsMiddleware)
    app.add_route(PATH, metrics_endpoint, include_in_schema=False)


async def metrics_endpoint(request: Request) -> Response:
    # Anything that came through the TLS proxy carries X-Forwarded-For; Prometheus scrapes the
    # container directly, so the public site never exposes model names and spend.
    if "x-forwarded-for" in request.headers:
        return Response(status_code=404)
    body, content_type = recorder.render()
    return Response(body, media_type=content_type)


class MetricsMiddleware:
    def __init__(self, app: ASGIApp) -> None:
        self.app = app

    async def __call__(self, scope: Scope, receive: Receive, send: Send) -> None:
        if scope["type"] != "http":
            await self.app(scope, receive, send)
            return
        watch = Stopwatch()
        status = 500
        first_byte_s: float | None = None

        async def observe(message: Message) -> None:
            nonlocal status, first_byte_s
            if message["type"] == "http.response.start":
                status = message["status"]
            elif message["type"] == "http.response.body" and first_byte_s is None:
                if message.get("body"):
                    first_byte_s = watch.elapsed_s
            await send(message)

        try:
            await self.app(scope, receive, observe)
        finally:
            recorder.http_request(
                scope["method"], route_of(scope), status, watch.elapsed_s, first_byte_s
            )


def route_of(scope: Scope) -> str:
    route = scope.get("route")
    path = getattr(route, "path", None)
    return path if isinstance(path, str) else UNMATCHED_ROUTE
