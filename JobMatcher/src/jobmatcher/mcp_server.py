"""MCP server exposing the agent's tools, so any MCP client (Claude Desktop, Claude Code, an
IDE) can search the index and compare jobs with a CV without going through /chat.

The tools are the same Toolbox the built-in agent uses: one implementation, two front ends.

    jobmatcher-mcp                 # stdio, for Claude Desktop / Claude Code
    jobmatcher-mcp --http          # streamable HTTP on :8001/mcp
"""

import argparse
import uuid
from collections.abc import AsyncIterator, Callable
from contextlib import AbstractAsyncContextManager, asynccontextmanager
from functools import lru_cache
from typing import Any

from mcp.server.mcpserver import MCPServer
from mcp.server.mcpserver.exceptions import ToolError as MCPToolError
from mcp.types import ToolAnnotations

from jobmatcher.agent.tools import Toolbox, ToolError
from jobmatcher.db.session import get_sessionmaker
from jobmatcher.search.embeddings import Embedder, get_embedder
from jobmatcher.search.hybrid import SearchFilters

ToolboxFactory = Callable[[uuid.UUID | None], AbstractAsyncContextManager[Toolbox]]

INSTRUCTIONS = """Search open job postings and compare them with a CV.
Use search_jobs first; filters are only for hard constraints (city, remote, years), skills
go into the query. Link jobs by the url the tools return, never invent one. To compare with
a CV, the user needs a cv_id from POST /cv of the Job Matcher API."""

READ_ONLY = ToolAnnotations(readOnlyHint=True, openWorldHint=False)


@lru_cache
def _embedder() -> Embedder:
    # Loading the e5 model takes seconds; do it once per process, not per call.
    return get_embedder()


@asynccontextmanager
async def _db_toolbox(cv_id: uuid.UUID | None) -> AsyncIterator[Toolbox]:
    async with get_sessionmaker()() as session:
        yield Toolbox(session, _embedder(), cv_id)


def build_server(toolbox_factory: ToolboxFactory = _db_toolbox) -> MCPServer:
    server = MCPServer("jobmatcher", instructions=INSTRUCTIONS)

    async def call(cv_id: uuid.UUID | None, name: str, args: dict[str, Any]) -> Any:
        async with toolbox_factory(cv_id) as toolbox:
            try:
                return await toolbox.call(name, args)
            except ToolError as exc:
                # Reported to the client as an is_error result, like in the agent loop.
                raise MCPToolError(str(exc)) from exc

    @server.tool(annotations=READ_ONLY)
    async def search_jobs(
        query: str,
        filters: SearchFilters | None = None,
        k: int = 5,
        cv_id: uuid.UUID | None = None,
    ) -> list[dict[str, Any]]:
        """Hybrid search over open job postings with optional hard filters. Returns ranked
        jobs with a snippet; with cv_id each job also gets a short CV fit (missing required
        skills, years gap)."""
        args = {"query": query, "k": k, **(filters.model_dump(exclude_none=True) if filters
                                              else {})}
        result: list[dict[str, Any]] = await call(cv_id, "search_jobs", args)
        return result

    @server.tool(annotations=READ_ONLY)
    async def get_job(job_id: int) -> dict[str, Any]:
        """Full posting text and extracted facts (seniority, years, remote policy, salary,
        skills) for one job."""
        result: dict[str, Any] = await call(None, "get_job", {"job_id": job_id})
        return result

    @server.tool(annotations=READ_ONLY)
    async def compare_with_cv(job_id: int, cv_id: uuid.UUID) -> dict[str, Any]:
        """Compare one job with an uploaded CV: matched skills, missing required and
        nice-to-have skills, years gap and location fit."""
        result: dict[str, Any] = await call(cv_id, "compare_with_cv", {"job_id": job_id})
        return result

    return server


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--http", action="store_true", help="serve streamable HTTP, not stdio")
    parser.add_argument("--port", type=int, default=8001)
    args = parser.parse_args()
    server = build_server()
    if args.http:
        server.run("streamable-http", host="0.0.0.0", port=args.port)
    else:
        server.run("stdio")


if __name__ == "__main__":
    main()
