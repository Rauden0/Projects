from fastapi.testclient import TestClient

from jobmatcher.api.main import create_app


def test_healthz() -> None:
    client = TestClient(create_app())
    response = client.get("/healthz")
    assert response.status_code == 200
    assert response.json() == {"status": "ok"}


def test_index_serves_the_chat_page() -> None:
    response = TestClient(create_app()).get("/")
    assert response.status_code == 200
    assert "Job Matcher" in response.text and "/chat" in response.text


def test_large_responses_are_gzipped() -> None:
    response = TestClient(create_app()).get("/", headers={"Accept-Encoding": "gzip"})
    assert response.headers["content-encoding"] == "gzip"


def test_readyz_reports_unavailable_dependencies(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    from jobmatcher.api.routes import health

    class DeadEngine:
        def connect(self) -> None:
            raise OSError("connection refused")

    async def dead_broker(*_: object, **__: object) -> None:
        raise OSError("connection refused")

    monkeypatch.setattr(health, "get_engine", lambda: DeadEngine())
    monkeypatch.setattr(health.aio_pika, "connect", dead_broker)

    response = TestClient(create_app()).get("/readyz")

    assert response.status_code == 503
    assert response.json() == {"database": "unavailable", "broker": "unavailable"}
