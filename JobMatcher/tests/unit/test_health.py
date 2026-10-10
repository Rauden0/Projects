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
