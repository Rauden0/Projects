"""Who is calling: an anonymous session cookie (AUTH_MODE=session) or an OIDC bearer JWT
(AUTH_MODE=oidc). The principal id owns the CVs the caller uploads; see docs/adr/0003."""

import asyncio
import base64
import hashlib
import hmac
import logging
import secrets
from collections.abc import Callable
from typing import Any, Protocol

import httpx
import jwt
from fastapi import Request
from starlette.concurrency import run_in_threadpool

from jobmatcher.config import Settings
from jobmatcher.errors import AuthUnavailable, Unauthenticated

log = logging.getLogger(__name__)

COOKIE = "jm_session"
COOKIE_MAX_AGE_S = 180 * 24 * 3600
ISSUED_COOKIE = "issued_cookie"
MIN_SECRET_CHARS = 32
# Asymmetric only: a symmetric algorithm would let anyone holding the public JWKS sign tokens.
ALGORITHMS = ["RS256", "RS384", "RS512", "PS256", "PS384", "PS512", "ES256", "ES384", "EdDSA"]
CLOCK_LEEWAY_S = 30


class Authenticator(Protocol):
    protects_search: bool
    limits_by_principal: bool

    async def authenticate(self, request: Request) -> str | None: ...


class SessionAuth:
    """Issues a random id in an HMAC-signed HttpOnly cookie to every caller without one."""

    protects_search = False
    # Anyone can drop the cookie and get a fresh identity, so limits stay on the address.
    limits_by_principal = False

    def __init__(self, key: bytes, secure: bool) -> None:
        self.key = key
        self.secure = secure

    async def authenticate(self, request: Request) -> str:
        session_id = self.verify(request.cookies.get(COOKIE, ""))
        if session_id is None:
            session_id = secrets.token_urlsafe(24)
            request.state.issued_cookie = self.cookie_header(session_id)
        return f"session:{session_id}"

    def sign(self, session_id: str) -> str:
        return f"{session_id}.{self._signature(session_id)}"

    def verify(self, value: str) -> str | None:
        session_id, _, signature = value.partition(".")
        if not session_id or not hmac.compare_digest(signature, self._signature(session_id)):
            return None
        return session_id

    def cookie_header(self, session_id: str) -> str:
        attributes = f"Max-Age={COOKIE_MAX_AGE_S}; Path=/; HttpOnly; SameSite=Lax"
        secure = "; Secure" if self.secure else ""
        return f"{COOKIE}={self.sign(session_id)}; {attributes}{secure}"

    def _signature(self, session_id: str) -> str:
        digest = hmac.new(self.key, f"session:{session_id}".encode(), hashlib.sha256).digest()
        return base64.urlsafe_b64encode(digest).rstrip(b"=").decode()


class SigningKeys(Protocol):
    async def key_for(self, token: str) -> Any: ...


class JwksKeys:
    """The issuer's signing keys, located through OIDC discovery; PyJWKClient caches the set
    and refetches it when a token names an unknown key id (key rotation)."""

    def __init__(
        self,
        issuer: str,
        http: Callable[[], httpx.AsyncClient] = lambda: httpx.AsyncClient(timeout=5),
        jwks_client: Callable[[str], jwt.PyJWKClient] = lambda uri: jwt.PyJWKClient(
            uri, lifespan=300, timeout=5
        ),
    ) -> None:
        self.issuer = issuer.rstrip("/")
        self.http = http
        self.jwks_client = jwks_client
        self._client: jwt.PyJWKClient | None = None
        self._lock = asyncio.Lock()

    async def key_for(self, token: str) -> Any:
        client = await self._discover()
        try:
            signing_key = await run_in_threadpool(client.get_signing_key_from_jwt, token)
        except jwt.PyJWKClientConnectionError as exc:
            raise AuthUnavailable(f"cannot fetch the issuer's signing keys: {exc}") from exc
        return signing_key.key

    async def _discover(self) -> jwt.PyJWKClient:
        async with self._lock:
            if self._client is None:
                self._client = self.jwks_client(await self._jwks_uri())
            return self._client

    async def _jwks_uri(self) -> str:
        url = f"{self.issuer}/.well-known/openid-configuration"
        try:
            async with self.http() as http:
                response = await http.get(url)
                response.raise_for_status()
                jwks_uri = response.json()["jwks_uri"]
        except (httpx.HTTPError, ValueError, KeyError, TypeError) as exc:
            raise AuthUnavailable(f"OIDC discovery at {url} failed: {exc!r}") from exc
        if not isinstance(jwks_uri, str):
            raise AuthUnavailable(f"OIDC discovery at {url} returned no jwks_uri")
        return jwks_uri


class OidcAuth:
    limits_by_principal = True

    def __init__(
        self, issuer: str, audience: str, keys: SigningKeys, protects_search: bool = False
    ) -> None:
        self.issuer = issuer
        self.audience = audience
        self.keys = keys
        self.protects_search = protects_search

    async def authenticate(self, request: Request) -> str | None:
        header = request.headers.get("authorization")
        if header is None:
            return None
        scheme, _, token = header.partition(" ")
        if scheme.lower() != "bearer" or not token.strip():
            raise Unauthenticated("expected an Authorization: Bearer token")
        return f"oidc:{await self._subject(token.strip())}"

    async def _subject(self, token: str) -> str:
        try:
            claims = jwt.decode(
                token,
                await self.keys.key_for(token),
                algorithms=ALGORITHMS,
                audience=self.audience,
                issuer=self.issuer,
                leeway=CLOCK_LEEWAY_S,
                options={"require": ["exp", "iss", "aud", "sub"]},
            )
        except jwt.PyJWTError as exc:
            raise Unauthenticated(f"invalid token: {exc}") from exc
        subject = claims["sub"]
        if not isinstance(subject, str) or not subject:
            raise Unauthenticated("invalid token: empty subject")
        return subject


def session_key(settings: Settings) -> bytes:
    secret = settings.session_secret.get_secret_value()
    if len(secret) >= MIN_SECRET_CHARS:
        return secret.encode()
    if secret:
        raise ValueError(f"SESSION_SECRET must be at least {MIN_SECRET_CHARS} characters")
    if not settings.auth_insecure_dev:
        raise ValueError(
            "SESSION_SECRET is not set; generate one with "
            "`python -c 'import secrets; print(secrets.token_urlsafe(32))'`, "
            "or set AUTH_INSECURE_DEV=true for local development"
        )
    log.warning("SESSION_SECRET is not set: sessions are signed with a key that dies on restart")
    return secrets.token_bytes(32)


def build_authenticator(settings: Settings) -> Authenticator:
    if settings.auth_mode == "oidc":
        keys = JwksKeys(settings.oidc_issuer)
        return OidcAuth(
            settings.oidc_issuer, settings.oidc_audience, keys, settings.auth_protect_search
        )
    return SessionAuth(session_key(settings), secure=settings.session_cookie_secure)
