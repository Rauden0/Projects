import logging
import re

from starlette.types import ASGIApp, Message, Receive, Scope, Send

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
