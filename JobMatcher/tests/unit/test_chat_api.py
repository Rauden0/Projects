import asyncio
import dataclasses
import inspect
import uuid
from typing import Any

import pytest
from fastapi.testclient import TestClient

from jobmatcher.agent import session as chat_session
from jobmatcher.agent.answers import Turn, answer_key
from jobmatcher.agent.loop import AgentEvent
from jobmatcher.agent.session import ChatGuards
from jobmatcher.api import deps
from jobmatcher.api.main import create_app
from jobmatcher.api.routes import chat as chat_route
from jobmatcher.budget.limits import StreamSlots
from jobmatcher.errors import BudgetExceeded, ClientLimited, NotFound
from jobmatcher.search.embeddings import HashingEmbedder


class FakeGate:
    def __init__(self, error: Exception | None = None) -> None:
        self.error = error
        self.clients: list[str] = []
        self.cached: list[str] = []

    async def admit(self, client_id: str) -> None:
        self.clients.append(client_id)
        if self.error is not None:
            raise self.error

    async def admit_cached(self, client_id: str) -> None:
        self.cached.append(client_id)


class FakeAnswers:
    def __init__(self) -> None:
        self.rows: dict[bytes, list[AgentEvent]] = {}
        self.lookups: list[Turn] = []

    async def lookup(self, turn: Turn) -> tuple[bytes, list[AgentEvent] | None]:
        self.lookups.append(turn)
        key = answer_key(turn, index_version=1)
        return key, self.rows.get(key)

    async def store(self, key: bytes, turn: Turn, events: list[AgentEvent]) -> None:
        self.rows[key] = events


def make_client(
    gate: FakeGate | None = None,
    answers: FakeAnswers | None = None,
    slots: StreamSlots | None = None,
    timeout_s: float | None = None,
) -> TestClient:
    async def fake_session():  # type: ignore[no-untyped-def]
        yield None

    app = create_app()
    app.dependency_overrides[deps.get_session] = fake_session
    app.dependency_overrides[deps.get_embedder] = lambda: HashingEmbedder(8)
    app.dependency_overrides[deps.get_chat_gate] = lambda: gate or FakeGate()
    app.dependency_overrides[deps.get_answer_cache] = lambda: answers
    if slots is not None:
        app.dependency_overrides[deps.get_stream_slots] = lambda: slots
    if timeout_s is not None:
        guards = deps.get_chat_guards

        def short(*args: Any, **kwargs: Any) -> ChatGuards:
            return dataclasses.replace(guards(*args, **kwargs), timeout_s=timeout_s)

        short.__signature__ = inspect.signature(guards)  # type: ignore[attr-defined]
        app.dependency_overrides[deps.get_chat_guards] = short
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


def _record_upload(monkeypatch: pytest.MonkeyPatch) -> list[Any]:
    extractors: list[Any] = []

    async def fake_create(_: Any, __: Any, text: str, owner_id: str, extractor: Any) -> Any:
        extractors.append((owner_id, extractor))
        return uuid.uuid4(), chat_route.CvProfile(source="heuristic")

    monkeypatch.setattr(chat_route, "create_cv", fake_create)
    return extractors


def test_upload_cv_uses_the_extraction_llm(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    purposes: list[str] = []

    def fake_get_llm(purpose: str, **_: Any) -> object:
        purposes.append(purpose)
        return object()

    monkeypatch.setattr(chat_route, "get_llm", fake_get_llm)
    uploads = _record_upload(monkeypatch)
    response = make_client().post("/cv", json={"text": "Python developer " * 5})
    assert response.status_code == 201
    assert response.json()["profile"]["source"] == "heuristic"
    assert purposes == ["extraction"]
    owner, extractor = uploads[0]
    assert owner.startswith("session:")
    assert isinstance(extractor, chat_route.LLMCvExtractor)


def test_upload_cv_without_llm_or_when_disabled_uses_heuristics(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    monkeypatch.setattr(chat_route, "get_llm", lambda *_, **__: None)
    uploads = _record_upload(monkeypatch)
    make_client().post("/cv", json={"text": "Python developer " * 5})
    monkeypatch.setattr(chat_route, "get_llm", lambda *_, **__: object())
    monkeypatch.setenv("CV_LLM_EXTRACTION", "false")
    chat_route.get_settings.cache_clear()
    try:
        make_client().post("/cv", json={"text": "Python developer " * 5})
    finally:
        monkeypatch.delenv("CV_LLM_EXTRACTION")
        chat_route.get_settings.cache_clear()
    assert [extractor for _, extractor in uploads] == [None, None]


def test_cv_uploads_are_rate_limited_per_client(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    monkeypatch.setattr(chat_route, "get_llm", lambda *_, **__: None)
    _record_upload(monkeypatch)
    client = make_client()
    statuses = [
        client.post("/cv", json={"text": "Python developer " * 5}).status_code for _ in range(6)
    ]
    assert statuses == [201] * 5 + [429]
    response = client.post("/cv", json={"text": "Python developer " * 5})
    assert response.json()["code"] == "rate_limited"
    assert 0 < int(response.headers["retry-after"]) <= 60


def test_cv_deletes_are_rate_limited(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    async def fake_delete(_: Any, cv_id: uuid.UUID, owner_id: str) -> None:
        return None

    monkeypatch.setattr(chat_route, "delete_cv", fake_delete)
    client = make_client()
    statuses = [client.delete(f"/cv/{uuid.uuid4()}").status_code for _ in range(11)]
    assert statuses == [204] * 10 + [429]


class ModelLLM:
    model = "qwen2.5:7b"


def _scripted_agent(monkeypatch: pytest.MonkeyPatch, *events: AgentEvent) -> list[str]:
    runs: list[str] = []

    async def fake_agent(_: Any, __: Any, message: str, *___: Any, **____: Any):  # type: ignore[no-untyped-def]
        runs.append(message)
        for event in events:
            yield event

    monkeypatch.setattr(chat_route, "get_llm", lambda *_, **__: ModelLLM())
    monkeypatch.setattr(chat_session, "run_agent", fake_agent)
    return runs


TOOL_CALL = AgentEvent("tool_call", {"name": "search_jobs", "input": {"query": "python"}})
ANSWER = AgentEvent("answer", {"text": "Zkus Heureku.", "unverified_urls": [], "rounds": 2})


def test_first_turn_is_replayed_from_the_answer_cache(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    runs = _scripted_agent(monkeypatch, TOOL_CALL, ANSWER)
    gate, answers = FakeGate(), FakeAnswers()
    client = make_client(gate, answers)
    first = client.post("/chat", json={"message": "Python v Brně?"})
    again = make_client(gate, answers).post("/chat", json={"message": "python  v brně? "})
    assert runs == ["Python v Brně?"]
    assert '"cached": true' not in first.text
    assert again.text.count('"cached": true') == 2
    assert again.text.splitlines()[0] == "event: tool_call"
    assert gate.clients == ["testclient"]
    assert gate.cached == ["testclient"]


def test_turn_with_history_bypasses_the_answer_cache(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    runs = _scripted_agent(monkeypatch, ANSWER)
    answers = FakeAnswers()
    history = [{"role": "user", "content": "ahoj"}, {"role": "assistant", "content": "ahoj"}]
    client = make_client(answers=answers)
    for message in ("a", "b"):
        client.post("/chat", json={"message": message, "history": history})
    assert runs == ["a", "b"]
    assert answers.lookups == [] and answers.rows == {}


@pytest.mark.parametrize(
    "events",
    [
        [TOOL_CALL, AgentEvent("error", {"message": "agent did not finish"})],
        [AgentEvent("answer", {"text": "x https://fake.example", "unverified_urls": ["u"]})],
        [AgentEvent("answer", {"text": "", "unverified_urls": []})],
    ],
)
def test_failed_or_unverified_turns_are_not_cached(monkeypatch, events) -> None:  # type: ignore[no-untyped-def]
    _scripted_agent(monkeypatch, *events)
    answers = FakeAnswers()
    make_client(answers=answers).post("/chat", json={"message": "python"})
    assert answers.rows == {}


def test_cv_answers_are_keyed_by_cv_and_owner_others_shared() -> None:
    cv_id = uuid.uuid4()
    mine = Turn("python", cv_id, "session:a", "profile", model="m")
    theirs = Turn("python", cv_id, "session:b", "profile", model="m")
    other_profile = Turn("python", cv_id, "session:a", "changed", model="m")
    assert len({answer_key(t, 1) for t in (mine, theirs, other_profile)}) == 3
    shared_a = Turn("python", None, "session:a", "", model="m")
    shared_b = Turn("Python ", None, "session:b", "", model="m")
    assert answer_key(shared_a, 1) == answer_key(shared_b, 1)
    assert answer_key(shared_a, 1) != answer_key(shared_a, 2)
    assert answer_key(shared_a, 1) != answer_key(dataclasses.replace(shared_a, model="n"), 1)


def test_same_message_again_right_away_is_429(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    runs = _scripted_agent(monkeypatch, AgentEvent("error", {"message": "x"}))
    client = make_client()
    assert client.post("/chat", json={"message": "python"}).status_code == 200
    again = client.post("/chat", json={"message": "python"})
    assert again.status_code == 429
    assert client.post("/chat", json={"message": "java"}).status_code == 200
    assert runs == ["python", "java"]


def test_client_over_its_stream_slots_is_429_and_full_server_503(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    _scripted_agent(monkeypatch, ANSWER)
    slots = StreamSlots(total=2, per_client=1, hold_s=60)
    held = slots.acquire("testclient")
    assert make_client(slots=slots).post("/chat", json={"message": "a"}).status_code == 429
    held.release()
    slots.acquire("other")
    slots.acquire("third")
    busy = make_client(slots=slots).post("/chat", json={"message": "b"})
    assert busy.status_code == 503
    assert busy.json()["code"] == "server_busy"
    assert busy.headers["retry-after"] == "10"


def test_stream_releases_its_slot_when_done(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    _scripted_agent(monkeypatch, ANSWER)
    slots = StreamSlots(total=1, per_client=1, hold_s=60)
    client = make_client(slots=slots)
    for message in ("a", "b"):
        assert client.post("/chat", json={"message": message}).status_code == 200
    assert len(slots) == 0


def test_slow_turn_ends_with_a_timeout_error(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    async def slow_agent(*_: Any, **__: Any):  # type: ignore[no-untyped-def]
        yield TOOL_CALL
        await asyncio.sleep(10)
        yield ANSWER

    monkeypatch.setattr(chat_route, "get_llm", lambda *_, **__: ModelLLM())
    monkeypatch.setattr(chat_session, "run_agent", slow_agent)
    response = make_client(timeout_s=0.2).post("/chat", json={"message": "python"})
    lines = response.text.splitlines()
    assert lines[0] == "event: tool_call"
    assert "event: error" in lines
    assert "Zkus" not in response.text
