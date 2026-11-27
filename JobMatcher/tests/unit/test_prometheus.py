from datetime import UTC, datetime, timedelta
from types import SimpleNamespace

import httpx
import pytest
from fastapi import FastAPI
from prometheus_client import REGISTRY

from jobmatcher import metrics, telemetry
from jobmatcher.errors import BudgetExceeded


def sample(name: str, **labels: str) -> float:
    return REGISTRY.get_sample_value(name, labels) or 0.0


def test_llm_call_records_calls_tokens_and_cost() -> None:
    labels = {"purpose": "chat", "model": "claude-haiku-5-5"}
    calls = sample("jobmatcher_llm_calls_total", **labels, provider="anthropic", outcome="ok")
    tokens_in = sample("jobmatcher_llm_tokens_total", **labels, kind="input")
    cost = sample("jobmatcher_llm_cost_usd_total", **labels)

    with telemetry.llm_call("anthropic", "claude-haiku-5-5", purpose="chat") as span:
        usage = SimpleNamespace(
            input_tokens=1000, output_tokens=200, cache_read_tokens=0, cache_write_tokens=0
        )
        span.response(usage, model=None, response_id=None, finish_reason=None, output=[])

    ok = sample("jobmatcher_llm_calls_total", **labels, provider="anthropic", outcome="ok")
    assert ok == calls + 1
    assert sample("jobmatcher_llm_tokens_total", **labels, kind="input") == tokens_in + 1000
    assert sample("jobmatcher_llm_cost_usd_total", **labels) == pytest.approx(cost + 0.0002)
    assert sample("jobmatcher_llm_call_duration_seconds_count", **labels) >= 1


def test_llm_call_refused_by_budget_counts_as_budget_exceeded() -> None:
    labels = {"purpose": "chat", "model": "m", "provider": "ollama"}
    before = sample("jobmatcher_llm_calls_total", **labels, outcome="budget_exceeded")
    with (
        pytest.raises(BudgetExceeded),
        telemetry.llm_call("ollama", "m", purpose="chat"),
    ):
        raise BudgetExceeded("chat", 5.0, 5.0)
    after = sample("jobmatcher_llm_calls_total", **labels, outcome="budget_exceeded")
    assert after == before + 1


def test_llm_call_without_a_response_counts_as_error() -> None:
    labels = {"purpose": "extraction", "model": "m", "provider": "ollama", "outcome": "error"}
    before = sample("jobmatcher_llm_calls_total", **labels)
    with telemetry.llm_call("ollama", "m", purpose="extraction"):
        pass
    assert sample("jobmatcher_llm_calls_total", **labels) == before + 1


def test_end_chat_counts_the_outcome() -> None:
    before = sample("jobmatcher_chat_turns_total", outcome="answer")
    span = telemetry.start_chat(request_id=None, has_cv=False, history_turns=0)
    telemetry.end_chat(span, outcome="answer", llm_calls=1, tool_calls=0)
    assert sample("jobmatcher_chat_turns_total", outcome="answer") == before + 1


def test_outbox_lag_is_the_age_of_the_oldest_row() -> None:
    metrics.recorder.outbox_batch(3, datetime.now(UTC) - timedelta(seconds=90))
    assert 89 < sample("jobmatcher_outbox_lag_seconds") < 100
    metrics.recorder.outbox_batch(0, None)
    assert sample("jobmatcher_outbox_lag_seconds") == 0


def instrumented_app() -> FastAPI:
    app = FastAPI()

    @app.get("/items/{item_id}")
    async def item(item_id: int) -> dict[str, int]:
        return {"id": item_id}

    metrics.instrument(app)
    return app


async def test_middleware_labels_requests_by_route_template() -> None:
    labels = {"method": "GET", "route": "/items/{item_id}", "status": "200"}
    before = sample("jobmatcher_http_request_duration_seconds_count", **labels)
    unmatched = {"method": "GET", "route": metrics.UNMATCHED_ROUTE, "status": "404"}
    before_unmatched = sample("jobmatcher_http_request_duration_seconds_count", **unmatched)
    transport = httpx.ASGITransport(app=instrumented_app())
    async with httpx.AsyncClient(transport=transport, base_url="http://t") as client:
        await client.get("/items/1")
        await client.get("/items/2")
        await client.get("/nope/3")
    assert sample("jobmatcher_http_request_duration_seconds_count", **labels) == before + 2
    after_unmatched = sample("jobmatcher_http_request_duration_seconds_count", **unmatched)
    assert after_unmatched == before_unmatched + 1
    first_byte = {"method": "GET", "route": "/items/{item_id}"}
    assert sample("jobmatcher_http_first_byte_seconds_count", **first_byte) >= 2


async def test_metrics_endpoint_serves_the_registry_but_not_through_the_proxy() -> None:
    transport = httpx.ASGITransport(app=instrumented_app())
    async with httpx.AsyncClient(transport=transport, base_url="http://t") as client:
        direct = await client.get("/metrics")
        proxied = await client.get("/metrics", headers={"X-Forwarded-For": "203.0.113.9"})
    assert direct.status_code == 200
    assert "jobmatcher_llm_calls_total" in direct.text
    assert proxied.status_code == 404


def test_without_prometheus_client_nothing_is_mounted(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(metrics, "recorder", metrics.Recorder())
    app = FastAPI()
    metrics.instrument(app)
    metrics.serve_from_env()
    metrics.recorder.chat_turn("answer")
    assert all(getattr(r, "path", None) != metrics.PATH for r in app.routes)
