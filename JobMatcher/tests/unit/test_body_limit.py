import httpx
from fastapi import FastAPI, Request
from starlette.types import Message

from jobmatcher.api.main import create_app
from jobmatcher.api.middleware import BodyLimitMiddleware


def echo_app(max_bytes: int) -> FastAPI:
    app = FastAPI()

    @app.post("/echo")
    async def echo(request: Request) -> dict[str, int]:
        return {"size": len(await request.body())}

    app.add_middleware(BodyLimitMiddleware, max_bytes=max_bytes)
    return app


def client(app: FastAPI) -> httpx.AsyncClient:
    return httpx.AsyncClient(transport=httpx.ASGITransport(app=app), base_url="http://test")


async def test_body_within_the_limit_reaches_the_app() -> None:
    async with client(echo_app(10)) as http:
        response = await http.post("/echo", content=b"x" * 10)
    assert response.json() == {"size": 10}


async def test_declared_length_over_the_limit_is_413() -> None:
    async with client(echo_app(10)) as http:
        response = await http.post("/echo", content=b"x" * 11)
    assert response.status_code == 413
    assert response.json()["code"] == "payload_too_large"


async def _send_chunks(app: FastAPI, chunks: list[bytes]) -> tuple[int, bytes]:
    messages: list[Message] = [
        {"type": "http.request", "body": c, "more_body": i < len(chunks) - 1}
        for i, c in enumerate(chunks)
    ]
    sent: list[Message] = []

    async def receive() -> Message:
        return messages.pop(0) if messages else {"type": "http.disconnect"}

    async def send(message: Message) -> None:
        sent.append(message)

    scope = {
        "type": "http",
        "asgi": {"version": "3.0"},
        "http_version": "1.1",
        "method": "POST",
        "scheme": "http",
        "path": "/echo",
        "raw_path": b"/echo",
        "root_path": "",
        "query_string": b"",
        "headers": [],
        "client": ("127.0.0.1", 1),
        "server": ("test", 80),
    }
    await app(scope, receive, send)
    body = b"".join(m.get("body", b"") for m in sent if m["type"] == "http.response.body")
    return sent[0]["status"], body


async def test_chunked_body_over_the_limit_is_413() -> None:
    status, _ = await _send_chunks(echo_app(10), [b"x" * 8, b"x" * 8])
    assert status == 413


async def test_chunked_body_within_the_limit_is_replayed_whole() -> None:
    status, body = await _send_chunks(echo_app(10), [b"x" * 4, b"x" * 4])
    assert (status, body) == (200, b'{"size":8}')


async def test_app_refuses_an_oversized_chat_before_parsing() -> None:
    async with client(create_app()) as http:
        response = await http.post(
            "/chat",
            content=b"{" + b" " * 300_000 + b"}",
            headers={"content-type": "application/json"},
        )
    assert response.status_code == 413
    assert response.headers["x-request-id"]
