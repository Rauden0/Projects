import uuid
from typing import Any

import httpx
import pytest
from fastapi.testclient import TestClient

from jobmatcher.api import deps
from jobmatcher.api.captcha import VERIFY_URL, CaptchaRejected, CaptchaUnavailable, Turnstile
from jobmatcher.api.main import create_app
from jobmatcher.api.routes import chat as chat_route
from jobmatcher.cv.profile import CvProfile

CV = {"text": "Python developer " * 5}


def turnstile(answer: dict[str, Any] | None, seen: list[dict[str, str]]) -> Turnstile:
    def handle(request: httpx.Request) -> httpx.Response:
        assert str(request.url) == VERIFY_URL
        seen.append(dict(httpx.QueryParams(request.content.decode())))
        if answer is None:
            raise httpx.ConnectError("unreachable")
        return httpx.Response(200, json=answer)

    return Turnstile(
        "s3cret", http=lambda: httpx.AsyncClient(transport=httpx.MockTransport(handle))
    )


async def test_accepted_token_passes_with_secret_and_address() -> None:
    seen: list[dict[str, str]] = []
    await turnstile({"success": True}, seen).verify("tok", "10.0.0.1")
    assert seen == [{"secret": "s3cret", "response": "tok", "remoteip": "10.0.0.1"}]


@pytest.mark.parametrize("token", ["", "x" * 3000])
async def test_missing_or_oversized_token_is_rejected_without_a_call(token: str) -> None:
    seen: list[dict[str, str]] = []
    with pytest.raises(CaptchaRejected):
        await turnstile({"success": True}, seen).verify(token, None)
    assert seen == []


async def test_refused_token_and_unreachable_cloudflare() -> None:
    with pytest.raises(CaptchaRejected):
        await turnstile({"success": False, "error-codes": ["invalid-input-response"]}, []).verify(
            "tok", None
        )
    with pytest.raises(CaptchaUnavailable):
        await turnstile(None, []).verify("tok", None)


def upload_client(monkeypatch: pytest.MonkeyPatch, captcha: Turnstile | None) -> TestClient:
    async def fake_session():  # type: ignore[no-untyped-def]
        yield None

    async def fake_create(*_: Any) -> Any:
        return uuid.uuid4(), CvProfile(source="heuristic")

    monkeypatch.setattr(chat_route, "get_llm", lambda *_, **__: None)
    monkeypatch.setattr(chat_route, "create_cv", fake_create)
    app = create_app()
    app.dependency_overrides[deps.get_session] = fake_session
    app.dependency_overrides[deps.get_captcha] = lambda: captcha
    return TestClient(app)


def test_cv_upload_needs_an_accepted_token_when_turnstile_is_on(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    seen: list[dict[str, str]] = []
    client = upload_client(monkeypatch, turnstile({"success": True}, seen))
    missing = client.post("/cv", json=CV)
    assert (missing.status_code, missing.json()["code"]) == (403, "captcha_failed")
    ok = client.post("/cv", json=CV, headers={"x-turnstile-token": "tok"})
    assert ok.status_code == 201
    assert seen[0]["remoteip"] == "testclient"


def test_cv_upload_with_cloudflare_down_is_503(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    client = upload_client(monkeypatch, turnstile(None, []))
    response = client.post("/cv", json=CV, headers={"x-turnstile-token": "tok"})
    assert response.status_code == 503


def test_cv_upload_without_turnstile_needs_no_token(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    assert upload_client(monkeypatch, None).post("/cv", json=CV).status_code == 201


def test_page_loads_the_widget_only_with_a_site_key(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    plain = TestClient(create_app()).get("/")
    assert "challenges.cloudflare.com" not in plain.headers["content-security-policy"]
    monkeypatch.setenv("TURNSTILE_SITE_KEY", "0x4AAAA-site_key")
    deps.get_settings.cache_clear()
    try:
        page = TestClient(create_app()).get("/")
    finally:
        monkeypatch.delenv("TURNSTILE_SITE_KEY")
        deps.get_settings.cache_clear()
    assert 'content="0x4AAAA-site_key"' in page.text
    csp = page.headers["content-security-policy"]
    assert "frame-src https://challenges.cloudflare.com" in csp
    assert "https://challenges.cloudflare.com 'sha256-" in csp
