import logging
import uuid
from collections.abc import AsyncIterator
from typing import Any

from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher.agent.loop import AgentEvent, run_agent
from jobmatcher.agent.tools import Toolbox
from jobmatcher.llm import LLMClient
from jobmatcher.observability import Stopwatch, UsageMeter
from jobmatcher.search.embeddings import Embedder

log = logging.getLogger(__name__)


async def chat_events(
    llm: LLMClient,
    meter: UsageMeter,
    sessionmaker: async_sessionmaker[AsyncSession],
    embedder: Embedder,
    cv_id: uuid.UUID | None,
    message: str,
    history: list[dict[str, Any]],
    profile_summary: str,
) -> AsyncIterator[AgentEvent]:
    """Runs one chat turn with its own DB session (an SSE stream outlives the request scope)
    and logs one `chat_turn` summary line when it ends."""
    watch = Stopwatch()
    tool_calls = 0
    outcome = "incomplete"
    async with sessionmaker() as session:
        toolbox = Toolbox(session, embedder, cv_id)
        try:
            async for event in run_agent(llm, toolbox, message, history, profile_summary):
                tool_calls += event.type == "tool_call"
                if event.type in ("answer", "error"):
                    outcome = event.type
                yield event
        finally:
            log.info(
                "chat_turn outcome=%s llm_calls=%d tool_calls=%d input_tokens=%d "
                "output_tokens=%d duration_ms=%d",
                outcome,
                len(meter.calls),
                tool_calls,
                meter.input_tokens,
                meter.output_tokens,
                watch.elapsed_s * 1000,
                extra={
                    "event": "chat_turn",
                    "outcome": outcome,
                    "llm_calls": len(meter.calls),
                    "tool_calls": tool_calls,
                    "input_tokens": meter.input_tokens,
                    "output_tokens": meter.output_tokens,
                    "duration_ms": round(watch.elapsed_s * 1000),
                },
            )
