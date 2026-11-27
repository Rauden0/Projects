import base64
import hashlib
import hmac
import json
import time
from typing import Any

import httpx
import jwt
import pytest
from cryptography.hazmat.primitives import serialization
from cryptography.hazmat.primitives.asymmetric import rsa
from fastapi.testclient import TestClient
from pydantic import SecretStr

from jobmatcher.api import deps
from jobmatcher.api.auth import (
    JwksKeys,
    OidcAuth,
    SessionAuth,
    build_authenticator,
    session_key,
)
from jobmatcher.api.main import create_app
from jobmatcher.api.routes import chat as chat_route
from jobmatcher.api.routes import jobs as jobs_route
from jobmatcher.config import Settings
from jobmatcher.errors import AuthUnavailable, NotFound
from jobmatcher.search.embeddings import HashingEmbedder

ISSUER = "https://idp.example"
AUDIENCE = "jobmatcher"
KEY = rsa.generate_private_key(public_exponent=65537, key_size=2048)
OTHER_KEY = rsa.generate_private_key(public_exponent=65537, key_size=2048)


def jwks(*keys: rsa.RSAPrivateKey) -> dict[str, Any]:
    entries = []
    for n, key in enumerate(keys):
        entry = json.loads(jwt.algorithms.RSAAlgorithm.to_jwk(key.public_key()))
        entries.append({**entry, "kid": f"k{n}", "use": "sig", "alg": "RS256"})
    return {"keys": entries}


def token(key: rsa.RSAPrivateKey = KEY, kid: str = "k0", **claims: Any) -> str:
    now = int(time.time())
    payload = {"iss": ISSUER, "aud": AUDIENCE, "sub": "user-1", "iat": now, "exp": now + 300}
    payload.update(claims)
    return jwt.encode(payload, key, algorithm="RS256", headers={"kid": kid})


class FakeJwksClient(jwt.PyJWKClient):
    """PyJWKClient with its cache and kid lookup, but the key set served from memory."""

    def __init__(self, uri: str, data: dict[str, Any]) -> None:
        super().__init__(uri, cache_jwk_set=False)
        self.data = data
        self.uris = [uri]

    def fetch_data(self) -> Any:
        return self.data


def discovery(jwks_uri: str | None = f"{ISSUER}/keys") -> httpx.AsyncClient:
    def handler(request: httpx.Request) -> httpx.Response:
        assert str(request.url) == f"{ISSUER}/.well-known/openid-configuration"
        return httpx.Response(200, json={"issuer": ISSUER, "jwks_uri": jwks_uri})

    return httpx.AsyncClient(transport=httpx.MockTransport(handler))


def jwks_keys(data: dict[str, Any] | None = None) -> JwksKeys:
    return JwksKeys(
        ISSUER, http=discovery, jwks_client=lambda uri: FakeJwksClient(uri, data or jwks(KEY))
    )


def oidc_client(protect_search: bool = False) -> TestClient:
    async def fake_session():  # type: ignore[no-untyped-def]
        yield None

    app = create_app()
    auth = OidcAuth(ISSUER, AUDIENCE, jwks_keys(), protects_search=protect_search)
    app.dependency_overrides[deps.get_authenticator] = lambda: auth
    app.dependency_overrides[deps.get_session] = fake_session
    app.dependency_overrides[deps.get_embedder] = lambda: HashingEmbedder(8)
    return TestClient(app)


def bearer(value: str) -> dict[str, str]:
    return {"authorization": f"Bearer {value}"}


def test_session_cookie_round_trips() -> None:
    auth = SessionAuth(b"k" * 32, secure=True)
    assert auth.verify(auth.sign("abc")) == "abc"


@pytest.mark.parametrize("value", ["", "abc", "abc.", "abc.forged", "abc.def.ghi"])
def test_unsigned_or_forged_session_cookie_is_rejected(value: str) -> None:
    assert SessionAuth(b"k" * 32, secure=True).verify(value) is None


def test_session_cookie_signed_with_another_key_is_rejected() -> None:
    signed = SessionAuth(b"a" * 32, secure=True).sign("abc")
    assert SessionAuth(b"b" * 32, secure=True).verify(signed) is None


def test_secure_flag_follows_setting() -> None:
    assert "; Secure" in SessionAuth(b"k" * 32, secure=True).cookie_header("abc")
    assert "Secure" not in SessionAuth(b"k" * 32, secure=False).cookie_header("abc")


def test_session_mode_without_secret_refuses_to_start() -> None:
    with pytest.raises(ValueError, match="SESSION_SECRET is not set"):
        session_key(Settings(session_secret=SecretStr(""), auth_insecure_dev=False))


def test_short_session_secret_refuses_to_start_even_in_dev() -> None:
    with pytest.raises(ValueError, match="at least 32"):
        session_key(Settings(session_secret=SecretStr("short"), auth_insecure_dev=True))


def test_insecure_dev_signs_with_a_random_key() -> None:
    settings = Settings(session_secret=SecretStr(""), auth_insecure_dev=True)
    assert session_key(settings) != session_key(settings)


def test_configured_secret_is_the_key() -> None:
    secret = "s" * 40
    assert session_key(Settings(session_secret=SecretStr(secret))) == secret.encode()


def test_oidc_mode_needs_issuer_and_audience() -> None:
    with pytest.raises(ValueError, match="OIDC_ISSUER"):
        Settings(auth_mode="oidc", oidc_issuer=ISSUER)


def test_oidc_mode_builds_oidc_auth_without_session_secret() -> None:
    settings = Settings(
        auth_mode="oidc", oidc_issuer=ISSUER, oidc_audience=AUDIENCE, auth_protect_search=True
    )
    auth = build_authenticator(settings)
    assert isinstance(auth, OidcAuth) and auth.protects_search


async def test_jwks_keys_discovers_the_key_set_once() -> None:
    clients: list[FakeJwksClient] = []

    def make(uri: str) -> FakeJwksClient:
        clients.append(FakeJwksClient(uri, jwks(OTHER_KEY, KEY)))
        return clients[-1]

    keys = JwksKeys(ISSUER, http=discovery, jwks_client=make)
    await keys.key_for(token(kid="k1"))
    await keys.key_for(token(kid="k1"))
    assert [c.uris for c in clients] == [[f"{ISSUER}/keys"]]


async def test_failed_discovery_is_auth_unavailable() -> None:
    keys = JwksKeys(ISSUER, http=lambda: discovery(None), jwks_client=jwt.PyJWKClient)
    with pytest.raises(AuthUnavailable):
        await keys.key_for(token())


def test_valid_token_is_accepted(monkeypatch: pytest.MonkeyPatch) -> None:
    owners: list[str] = []

    async def record(_: Any, cv_id: Any, owner_id: str) -> None:
        owners.append(owner_id)

    monkeypatch.setattr(chat_route, "delete_cv", record)
    response = oidc_client().delete(
        "/cv/00000000-0000-0000-0000-000000000001", headers=bearer(token())
    )
    assert response.status_code == 204
    assert owners == ["oidc:user-1"]
    assert "set-cookie" not in response.headers


@pytest.mark.parametrize(
    "bad",
    [
        token(exp=int(time.time()) - 600),
        token(aud="someone-else"),
        token(iss="https://evil.example"),
        token(key=OTHER_KEY),
        token(kid="unknown"),
        token(sub=""),
        jwt.encode({"iss": ISSUER, "aud": AUDIENCE, "sub": "x"}, KEY, algorithm="RS256"),
        "not-a-jwt",
    ],
    ids=["expired", "audience", "issuer", "signature", "kid", "empty-sub", "no-exp", "garbage"],
)
def test_invalid_token_is_401(bad: str) -> None:
    response = oidc_client().delete("/cv/00000000-0000-0000-0000-000000000001", headers=bearer(bad))
    assert response.status_code == 401
    assert response.json()["code"] == "unauthenticated"
    assert response.headers["www-authenticate"] == "Bearer"


def test_symmetric_token_signed_with_the_public_key_is_401() -> None:
    def b64(data: bytes) -> bytes:
        return base64.urlsafe_b64encode(data).rstrip(b"=")

    header = b64(json.dumps({"alg": "HS256", "typ": "JWT", "kid": "k0"}).encode())
    claims = {"iss": ISSUER, "aud": AUDIENCE, "sub": "x", "exp": int(time.time()) + 60}
    signing_input = header + b"." + b64(json.dumps(claims).encode())
    public_pem = KEY.public_key().public_bytes(
        serialization.Encoding.PEM, serialization.PublicFormat.SubjectPublicKeyInfo
    )
    signature = b64(hmac.new(public_pem, signing_input, hashlib.sha256).digest())
    forged = (signing_input + b"." + signature).decode()
    response = oidc_client().delete(
        "/cv/00000000-0000-0000-0000-000000000001", headers=bearer(forged)
    )
    assert response.status_code == 401


def test_chat_without_token_is_401_in_oidc_mode() -> None:
    response = oidc_client().post("/chat", json={"message": "hi"})
    assert response.status_code == 401


def test_non_bearer_scheme_is_401() -> None:
    response = oidc_client().post(
        "/chat", json={"message": "hi"}, headers={"authorization": "Basic dXNlcjpwdw=="}
    )
    assert response.status_code == 401


def test_oidc_mode_limits_by_principal(monkeypatch: pytest.MonkeyPatch) -> None:
    clients: list[str] = []

    class Gate:
        async def admit(self, client_id: str) -> None:
            clients.append(client_id)

    async def no_cv(*_: Any) -> str:
        return ""

    monkeypatch.setattr(chat_route, "get_llm", lambda *_, **__: object())
    monkeypatch.setattr(chat_route, "profile_summary", no_cv)
    monkeypatch.setattr(chat_route, "chat_events", lambda *_: _no_events())
    client = oidc_client()
    client.app.dependency_overrides[deps.get_chat_gate] = Gate  # type: ignore[attr-defined]
    client.post("/chat", json={"message": "hi"}, headers=bearer(token()))
    assert clients == ["oidc:user-1"]


async def _no_events():  # type: ignore[no-untyped-def]
    return
    yield


def _job_missing(monkeypatch: pytest.MonkeyPatch) -> None:
    async def missing(_: Any, job_id: int) -> None:
        raise NotFound("job", job_id)

    monkeypatch.setattr(jobs_route.repository, "get_job", missing)


def test_job_catalog_is_public_in_oidc_mode_by_default(monkeypatch: pytest.MonkeyPatch) -> None:
    _job_missing(monkeypatch)
    assert oidc_client().get("/jobs/1").status_code == 404


def test_protected_job_catalog_needs_a_token(monkeypatch: pytest.MonkeyPatch) -> None:
    _job_missing(monkeypatch)
    client = oidc_client(protect_search=True)
    assert client.get("/jobs/1").status_code == 401
    assert client.get("/jobs/1", headers=bearer(token())).status_code == 404


def test_health_stays_public_when_search_is_protected() -> None:
    assert oidc_client(protect_search=True).get("/healthz").status_code == 200
