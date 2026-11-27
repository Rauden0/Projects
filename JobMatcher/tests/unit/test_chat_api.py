import uuid
from typing import Any

import pytest
from fastapi.testclient import TestClient

from jobmatcher.agent import session as chat_session
from jobmatcher.agent.loop import AgentEvent
from jobmatcher.api import deps
from jobmatcher.api.main import create_app
from jobmatcher.api.routes import chat as chat_route
from jobmatcher.errors import BudgetExceeded, ClientLimited, NotFound
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


@pytest.fixture
def llm(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(chat_route, "get_llm", lambda *_, **__: object())


def test_chat_without_llm_key_is_503_and_not_counted(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    monkeypatch.setattr(chat_route, "get_llm", lambda *_, **__: None)
    gate = FakeGate()
    assert make_client(gate).post("/chat", json={"message": "hi"}).status_code == 503
    assert gate.clients == []


@pytest.mark.usefixtures("llm")
def test_unknown_cv_is_404_and_not_counted(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    async def missing(_: Any, cv_id: uuid.UUID | None, owner_id: str) -> str:
        raise NotFound("cv", cv_id)

    monkeypatch.setattr(chat_route, "profile_summary", missing)
    gate = FakeGate()
    response = make_client(gate).post("/chat", json={"message": "hi", "cv_id": str(uuid.uuid4())})
    assert response.status_code == 404
    assert gate.clients == []


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


@pytest.mark.usefixtures("llm")
def test_rate_limited_client_gets_429_with_retry_after() -> None:
    gate = FakeGate(ClientLimited("too many requests", 12))
    response = make_client(gate).post("/chat", json={"message": "hi"})
    assert response.status_code == 429
    assert response.headers["retry-after"] == "12"


@pytest.mark.usefixtures("llm")
def test_client_is_keyed_by_peer_address_not_header() -> None:
    gate = FakeGate(ClientLimited("too many requests", 12))
    make_client(gate).post("/chat", json={"message": "hi"}, headers={"X-Client-Id": "martin"})
    assert gate.clients == ["testclient"]


@pytest.mark.usefixtures("llm")
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


def test_delete_cv_is_204(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    deleted: list[tuple[uuid.UUID, str]] = []

    async def fake_delete(_: Any, cv_id: uuid.UUID, owner_id: str) -> None:
        deleted.append((cv_id, owner_id))

    monkeypatch.setattr(chat_route, "delete_cv", fake_delete)
    cv_id = uuid.uuid4()
    response = make_client().delete(f"/cv/{cv_id}")
    assert response.status_code == 204
    assert deleted[0][0] == cv_id
    assert deleted[0][1].startswith("session:")


def test_delete_unknown_cv_is_404(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    async def missing(_: Any, cv_id: uuid.UUID, owner_id: str) -> None:
        raise NotFound("cv", cv_id)

    monkeypatch.setattr(chat_route, "delete_cv", missing)
    assert make_client().delete(f"/cv/{uuid.uuid4()}").status_code == 404


def test_chat_issues_a_signed_http_only_session_cookie(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    monkeypatch.setattr(chat_route, "get_llm", lambda *_, **__: None)
    response = make_client().post("/chat", json={"message": "hi"})
    cookie = response.headers["set-cookie"]
    assert cookie.startswith("jm_session=")
    assert "HttpOnly" in cookie and "SameSite=Lax" in cookie


def test_known_session_cookie_is_not_reissued(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    monkeypatch.setattr(chat_route, "get_llm", lambda *_, **__: None)
    client = make_client()
    client.post("/chat", json={"message": "hi"})
    assert "set-cookie" not in client.post("/chat", json={"message": "hi"}).headers


def test_session_principal_is_stable_across_requests(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    owners: list[str] = []

    async def record(_: Any, cv_id: uuid.UUID, owner_id: str) -> None:
        owners.append(owner_id)

    monkeypatch.setattr(chat_route, "delete_cv", record)
    client = make_client()
    client.delete(f"/cv/{uuid.uuid4()}")
    client.delete(f"/cv/{uuid.uuid4()}")
    other = make_client()
    other.delete(f"/cv/{uuid.uuid4()}")
    assert owners[0] == owners[1] != owners[2]


def test_form_post_is_rejected_as_csrf() -> None:
    response = make_client().post(
        "/chat", content='{"message": "hi"}', headers={"content-type": "text/plain"}
    )
    assert response.status_code == 403
    assert response.json()["code"] == "csrf_rejected"


def test_post_without_content_type_is_rejected_as_csrf() -> None:
    response = make_client().post("/cv", content=b'{"text": "x"}')
    assert response.status_code == 403


def test_cross_site_delete_is_rejected() -> None:
    response = make_client().delete(f"/cv/{uuid.uuid4()}", headers={"sec-fetch-site": "cross-site"})
    assert response.status_code == 403
