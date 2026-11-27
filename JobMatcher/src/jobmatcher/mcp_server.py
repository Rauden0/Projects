"""MCP server for the agent's read-only tools, so Claude Desktop, an IDE or any other MCP
client can search the job index without going through /chat.

    jobmatcher-mcp                  # stdio
    jobmatcher-mcp --http           # streamable HTTP on http://127.0.0.1:8001/mcp

Off loopback, --http needs MCP_HTTP_TOKEN and every request `Authorization: Bearer <token>`.
"""

import argparse
import hmac
import ipaddress
import json
import logging
from collections.abc import AsyncIterator, Callable
from contextlib import AbstractAsyncContextManager, asynccontextmanager
from typing import Any, Protocol

import anyio
import uvicorn
from mcp.server import Server, ServerRequestContext
from mcp.server.stdio import stdio_server
from mcp.shared.exceptions import MCPError
from mcp.types import (
    INVALID_PARAMS,
    CallToolRequestParams,
    CallToolResult,
    ListToolsResult,
    PaginatedRequestParams,
    TextContent,
    Tool,
    ToolAnnotations,
)
from pydantic import ValidationError
from sqlalchemy.exc import SQLAlchemyError
from starlette.responses import JSONResponse
from starlette.types import ASGIApp, Receive, Scope, Send

from jobmatcher.agent.tools import Toolbox, as_untrusted_data
from jobmatcher.config import configure_logging, get_settings
from jobmatcher.db.session import get_sessionmaker
from jobmatcher.errors import ToolError
from jobmatcher.observability import Stopwatch
from jobmatcher.search.embeddings import Embedder, get_embedder

log = logging.getLogger(__name__)

# compare_with_cv stays with the agent: it needs a CV uploaded through the API in the same
# conversation, and CV data has no business leaving it.
EXPOSED = {"search_jobs": "Search jobs", "get_job": "Get job"}

INSTRUCTIONS = """Search open job postings, most of them in Czech.
Start with search_jobs and put the occupation and skills into the query; use filters only for
constraints the user stated. Call get_job only with a job_id from a search result. Link jobs
by the exact url the tools return, never invent one."""

READ_ONLY = ToolAnnotations(read_only_hint=True, open_world_hint=False)
MIN_TOKEN_CHARS = 32


class Tools(Protocol):
    async def call(self, name: str, raw_input: dict[str, Any]) -> Any: ...


ToolsFactory = Callable[[], AbstractAsyncContextManager[Tools]]


def database_tools(embedder: Embedder) -> ToolsFactory:
    @asynccontextmanager
    async def open_tools() -> AsyncIterator[Toolbox]:
        async with get_sessionmaker()() as session:
            yield Toolbox(session, embedder, cv_id=None)

    return open_tools


def tool_list() -> list[Tool]:
    return [
        Tool(
            name=d["name"],
            title=EXPOSED[d["name"]],
            description=d["description"],
            input_schema=d["input_schema"],
            annotations=READ_ONLY,
        )
        for d in Toolbox.definitions()
        if d["name"] in EXPOSED
    ]


def build_server(open_tools: ToolsFactory) -> Server:
    tools = tool_list()
    schemas = {t.name: t.input_schema for t in tools}

    async def list_tools(
        _: ServerRequestContext, params: PaginatedRequestParams | None
    ) -> ListToolsResult:
        return ListToolsResult(tools=tools)

    async def call_tool(_: ServerRequestContext, params: CallToolRequestParams) -> CallToolResult:
        if params.name not in schemas:
            raise MCPError(INVALID_PARAMS, f"unknown tool {params.name!r}")
        watch = Stopwatch()
        result = await _call(open_tools, params.name, params.arguments or {})
        log.info(
            "mcp_tool_call tool=%s is_error=%s duration_ms=%d",
            params.name,
            result.is_error,
            watch.elapsed_s * 1000,
            extra={
                "event": "mcp_tool_call",
                "tool": params.name,
                "is_error": result.is_error,
                "duration_ms": round(watch.elapsed_s * 1000),
            },
        )
        return result

    return Server(
        "jobmatcher",
        instructions=INSTRUCTIONS,
        get_tool_input_schema=schemas.get,
        on_list_tools=list_tools,
        on_call_tool=call_tool,
    )


async def _call(open_tools: ToolsFactory, name: str, args: dict[str, Any]) -> CallToolResult:
    try:
        async with open_tools() as tools:
            return _ok(await tools.call(name, args))
    except ToolError as exc:
        return _error(str(exc))
    except ValidationError as exc:
        # A tool execution error, not a protocol error, so the model can fix its arguments.
        return _error(_describe(exc))
    except (SQLAlchemyError, OSError) as exc:
        log.warning("mcp_tool_call database unavailable: %s", exc)
        return _error("the job index is unavailable, try again later")


def _ok(output: Any) -> CallToolResult:
    # Posting text comes from whoever published the ad and goes on into another model.
    text = json.dumps(as_untrusted_data(output), default=str, ensure_ascii=False)
    data = json.loads(text)
    return CallToolResult(
        content=[TextContent(type="text", text=text)],
        structured_content=data if isinstance(data, dict) else {"result": data},
    )


def _error(message: str) -> CallToolResult:
    return CallToolResult(content=[TextContent(type="text", text=message)], is_error=True)


def _describe(exc: ValidationError) -> str:
    # Field names and messages only: pydantic's default text repeats the rejected input.
    return "; ".join(
        f"{'.'.join(str(p) for p in e['loc']) or 'input'}: {e['msg']}" for e in exc.errors()
    )


async def _serve_stdio(server: Server) -> None:
    async with stdio_server() as (read, write):
        await server.run(read, write, server.create_initialization_options())


class BearerTokenMiddleware:
    def __init__(self, app: ASGIApp, token: str) -> None:
        self.app = app
        self.expected = f"Bearer {token}".encode()

    async def __call__(self, scope: Scope, receive: Receive, send: Send) -> None:
        if scope["type"] == "http" and not self._authorized(scope):
            response = JSONResponse(
                {"error": "unauthorized"}, status_code=401, headers={"WWW-Authenticate": "Bearer"}
            )
            await response(scope, receive, send)
            return
        await self.app(scope, receive, send)

    def _authorized(self, scope: Scope) -> bool:
        header = dict(scope["headers"]).get(b"authorization", b"")
        return hmac.compare_digest(header, self.expected)


def is_loopback(host: str) -> bool:
    if host == "localhost":
        return True
    try:
        return ipaddress.ip_address(host).is_loopback
    except ValueError:
        return False


def http_app(server: Server, host: str, token: str) -> ASGIApp:
    if token and len(token) < MIN_TOKEN_CHARS:
        raise ValueError(f"MCP_HTTP_TOKEN must be at least {MIN_TOKEN_CHARS} characters")
    if not token and not is_loopback(host):
        raise ValueError(f"--host {host} is reachable from other machines: set MCP_HTTP_TOKEN")
    app = server.streamable_http_app(stateless_http=True, host=host)
    return BearerTokenMiddleware(app, token) if token else app


def main() -> None:
    parser = argparse.ArgumentParser(
        prog="jobmatcher-mcp", description="MCP server for the job search tools"
    )
    parser.add_argument("--http", action="store_true", help="serve streamable HTTP, not stdio")
    parser.add_argument("--host", default="127.0.0.1")
    parser.add_argument("--port", type=int, default=8001)
    args = parser.parse_args()

    configure_logging()
    server = build_server(database_tools(get_embedder()))
    if args.http:
        token = get_settings().mcp_http_token.get_secret_value()
        try:
            app = http_app(server, args.host, token)
        except ValueError as exc:
            parser.error(str(exc))
        uvicorn.run(app, host=args.host, port=args.port, log_config=None)
    else:
        anyio.run(_serve_stdio, server)


if __name__ == "__main__":
    main()
