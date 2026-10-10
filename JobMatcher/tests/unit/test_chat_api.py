from typing import Any

from fastapi.testclient import TestClient

from jobmatcher.agent.loop import AgentEvent
from jobmatcher.api import deps
from jobmatcher.api.main import create_app
from jobmatcher.api.routes import chat as chat_route
from jobmatcher.search.embeddings import HashingEmbedder


def make_client() -> TestClient:
    async def fake_session():  # type: ignore[no-untyped-def]
        yield None

    app = create_app()
    app.dependency_overrides[deps.get_session] = fake_session
    app.dependency_overrides[deps.get_embedder] = lambda: HashingEmbedder(8)
    return TestClient(app)


def test_chat_without_llm_key_is_503(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    monkeypatch.setattr(chat_route, "get_llm", lambda: None)
    assert make_client().post("/chat", json={"message": "hi"}).status_code == 503


def test_chat_streams_agent_events(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    async def fake_agent(*_: Any, **__: Any):  # type: ignore[no-untyped-def]
        yield AgentEvent("tool_call", {"name": "search_jobs", "input": {"query": "python"}})
        yield AgentEvent("answer", {"text": "Zkus Heureku.", "unverified_urls": []})

    monkeypatch.setattr(chat_route, "get_llm", lambda: object())
    monkeypatch.setattr(chat_route, "run_agent", fake_agent)

    response = make_client().post("/chat", json={"message": "python v Brně?"})

    assert response.status_code == 200
    assert response.headers["content-type"].startswith("text/event-stream")
    assert response.text.splitlines()[:2] == [
        "event: tool_call",
        'data: {"name": "search_jobs", "input": {"query": "python"}}',
    ]
    assert 'data: {"text": "Zkus Heureku."' in response.text
