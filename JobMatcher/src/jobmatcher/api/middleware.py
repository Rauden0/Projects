import logging
import re
from typing import Any

from starlette.types import ASGIApp, Message, Receive, Scope, Send

from jobmatcher.api.auth import ISSUED_COOKIE
from jobmatcher.observability import Stopwatch, new_request_id, request_id_var

log = logging.getLogger("jobmatcher.access")

HEADER = "x-request-id"
_VALID_ID = re.compile(r"^[\w.-]{1,64}$")


class RequestIdMiddleware:
    """Tags every request with an id (the caller's X-Request-ID if sane, else a new one), puts
    it in the log context and the response headers, and logs one access line per request."""

    def __init__(self, app: ASGIApp) -> None:
        self.app = app

    async def __call__(self, scope: Scope, receive: Receive, send: Send) -> None:
        if scope["type"] != "http":
            await self.app(scope, receive, send)
            return

        incoming = dict(scope["headers"]).get(HEADER.encode(), b"").decode("latin-1")
        request_id = incoming if _VALID_ID.match(incoming) else new_request_id()
        request_id_var.set(request_id)
        watch = Stopwatch()
        status = 500

        async def send_with_id(message: Message) -> None:
            nonlocal status
            if message["type"] == "http.response.start":
                status = message["status"]
                message.setdefault("headers", [])
                message["headers"].append((HEADER.encode(), request_id.encode()))
            await send(message)

        try:
            await self.app(scope, receive, send_with_id)
        finally:
            log.info(
                "%s %s %d %dms",
                scope["method"],
                scope["path"],
                status,
                watch.elapsed_s * 1000,
                extra={
                    "event": "http_request",
                    "method": scope["method"],
                    "path": scope["path"],
                    "status": status,
                    "duration_ms": round(watch.elapsed_s * 1000),
                },
            )


class SessionCookieMiddleware:
    """Sends the Set-Cookie header for a session that `SessionAuth` issued during the request,
    on every response including error responses."""

    def __init__(self, app: ASGIApp) -> None:
        self.app = app

    async def __call__(self, scope: Scope, receive: Receive, send: Send) -> None:
        if scope["type"] != "http":
            await self.app(scope, receive, send)
            return
        state: dict[str, Any] = scope.setdefault("state", {})

        async def send_with_cookie(message: Message) -> None:
            cookie = state.get(ISSUED_COOKIE)
            if message["type"] == "http.response.start" and isinstance(cookie, str):
                message.setdefault("headers", [])
                message["headers"].append((b"set-cookie", cookie.encode("latin-1")))
            await send(message)

        await self.app(scope, receive, send_with_cookie)


# The page sets its own CSP; JSON responses get one that allows nothing. /docs loads Swagger
# UI from a CDN and keeps FastAPI's defaults.
API_CSP = "default-src 'none'; frame-ancestors 'none'"
NO_CSP_PATHS = ("/docs", "/redoc")


class SecurityHeadersMiddleware:
    def __init__(self, app: ASGIApp) -> None:
        self.app = app

    async def __call__(self, scope: Scope, receive: Receive, send: Send) -> None:
        if scope["type"] != "http":
            await self.app(scope, receive, send)
            return
        with_csp = not str(scope["path"]).startswith(NO_CSP_PATHS)

        async def send_with_headers(message: Message) -> None:
            if message["type"] == "http.response.start":
                headers = message.setdefault("headers", [])
                present = {name.lower() for name, _ in headers}
                for name, value in _security_headers(with_csp):
                    if name not in present:
                        headers.append((name, value))
            await send(message)

        await self.app(scope, receive, send_with_headers)


def _security_headers(with_csp: bool) -> list[tuple[bytes, bytes]]:
    headers = [
        (b"x-content-type-options", b"nosniff"),
        (b"referrer-policy", b"no-referrer"),
        (b"x-frame-options", b"DENY"),
    ]
    if with_csp:
        headers.append((b"content-security-policy", API_CSP.encode()))
    return headers
