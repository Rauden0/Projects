from typing import Any

from fastapi.testclient import TestClient

from jobmatcher.agent import session as chat_session
from jobmatcher.agent.loop import AgentEvent
from jobmatcher.api import deps
from jobmatcher.api.main import create_app
from jobmatcher.api.routes import chat as chat_route
from jobmatcher.errors import BudgetExceeded, ClientLimited
from jobmatcher.search.embeddings import HashingEmbedder


class FakeGate:
    def __init__(self, error: Exception | None = None) -> None:
        self.error = error
        self.clients: list[str] = []

    async def admit(self, client_id: str) -> None:
        self.clients.append(client_id)
        if self.error is not None:
            raise self.error


def make_client(gate: FakeGate | None = None) -> TestClient:
    async def fake_session():  # type: ignore[no-untyped-def]
        yield None

    app = create_app()
    app.dependency_overrides[deps.get_session] = fake_session
    app.dependency_overrides[deps.get_embedder] = lambda: HashingEmbedder(8)
    app.dependency_overrides[deps.get_chat_gate] = lambda: gate or FakeGate()
    return TestClient(app)


def test_chat_without_llm_key_is_503(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    monkeypatch.setattr(chat_route, "get_llm", lambda *_, **__: None)
    assert make_client().post("/chat", json={"message": "hi"}).status_code == 503


def test_chat_streams_agent_events(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    async def fake_agent(*_: Any, **__: Any):  # type: ignore[no-untyped-def]
        yield AgentEvent("tool_call", {"name": "search_jobs", "input": {"query": "python"}})
        yield AgentEvent("answer", {"text": "Zkus Heureku.", "unverified_urls": []})

    monkeypatch.setattr(chat_route, "get_llm", lambda *_, **__: object())
    monkeypatch.setattr(chat_session, "run_agent", fake_agent)

    response = make_client().post("/chat", json={"message": "python v Brně?"})

    assert response.status_code == 200
    assert response.headers["content-type"].startswith("text/event-stream")
    assert response.text.splitlines()[:2] == [
        "event: tool_call",
        'data: {"name": "search_jobs", "input": {"query": "python"}}',
    ]
    assert 'data: {"text": "Zkus Heureku."' in response.text


def test_rate_limited_client_gets_429_with_retry_after() -> None:
    gate = FakeGate(ClientLimited("too many requests", 12))
    response = make_client(gate).post("/chat", json={"message": "hi"})
    assert response.status_code == 429
    assert response.headers["retry-after"] == "12"


def test_client_is_keyed_by_peer_address_not_header() -> None:
    gate = FakeGate(ClientLimited("too many requests", 12))
    make_client(gate).post("/chat", json={"message": "hi"}, headers={"X-Client-Id": "martin"})
    assert gate.clients == ["testclient"]


def test_exhausted_budget_is_503() -> None:
    gate = FakeGate(BudgetExceeded("chat", 40.0, 40.0))
    response = make_client(gate).post("/chat", json={"message": "hi"})
    assert response.status_code == 503
    assert "budget" in response.json()["detail"]


def test_oversized_history_is_422() -> None:
    history = [{"role": "user", "content": "x" * 10_000}] * 3
    response = make_client().post("/chat", json={"message": "hi", "history": history})
    assert response.status_code == 422


def test_history_within_total_limit_is_accepted(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    monkeypatch.setattr(chat_route, "get_llm", lambda *_, **__: None)
    history = [{"role": "user", "content": "x" * 2_000}] * 10
    response = make_client().post("/chat", json={"message": "hi", "history": history})
    assert response.status_code == 503
