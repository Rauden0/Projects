import asyncio
import logging
import uuid
from collections.abc import AsyncGenerator, AsyncIterator, Callable
from dataclasses import dataclass
from typing import Any, Protocol

from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher import metrics, telemetry
from jobmatcher.agent.answers import AnswerCache, Turn, recording, replay
from jobmatcher.agent.loop import AgentEvent, run_agent
from jobmatcher.agent.tools import Toolbox
from jobmatcher.budget.limits import DuplicateGuard, Slot, StreamSlots
from jobmatcher.llm import LLMClient
from jobmatcher.observability import Stopwatch, UsageMeter, request_id_var
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
) -> AsyncGenerator[AgentEvent]:
    """Runs one chat turn with its own DB session (an SSE stream outlives the request scope),
    traced as one `chat` span, and logs one `chat_turn` summary line when it ends."""
    watch = Stopwatch()
    tool_calls = 0
    outcome = "incomplete"
    async with sessionmaker() as session:
        toolbox = Toolbox(session, embedder, cv_id)
        span = telemetry.start_chat(
            request_id=request_id_var.get(), has_cv=cv_id is not None, history_turns=len(history)
        )
        try:
            agent = run_agent(llm, toolbox, message, history, profile_summary)
            async for event in telemetry.within(span, agent):
                tool_calls += event.type == "tool_call"
                if event.type in ("answer", "error"):
                    outcome = event.type
                yield event
        finally:
            telemetry.end_chat(
                span, outcome=outcome, llm_calls=len(meter.calls), tool_calls=tool_calls
            )
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


TIMEOUT_MESSAGE = "The answer took too long, try again later."


async def with_deadline(
    events: AsyncGenerator[AgentEvent], timeout_s: float
) -> AsyncIterator[AgentEvent]:
    """Ends the turn with an error event once it has run `timeout_s`; closing the agent's
    generator cancels its pending LLM or tool call."""
    deadline = asyncio.get_running_loop().time() + timeout_s
    try:
        while True:
            try:
                async with asyncio.timeout_at(deadline):
                    event = await anext(events)
            except StopAsyncIteration:
                return
            except TimeoutError:
                log.warning("chat turn stopped after %.0fs", timeout_s)
                metrics.recorder.rejected("chat_timeout")
                yield AgentEvent("error", {"message": TIMEOUT_MESSAGE})
                return
            yield event
    finally:
        await events.aclose()


class Gate(Protocol):
    async def admit(self, client_id: str) -> None: ...

    async def admit_cached(self, client_id: str) -> None: ...


@dataclass(frozen=True, slots=True)
class ChatGuards:
    gate: Gate
    slots: StreamSlots
    duplicates: DuplicateGuard
    answers: AnswerCache | None
    timeout_s: float


async def start_turn(
    guards: ChatGuards,
    client_id: str,
    turn: Turn,
    run: Callable[[], AsyncGenerator[AgentEvent]],
) -> AsyncIterator[AgentEvent]:
    """Admits a turn before its first event, so a refusal is an HTTP error and not an event:
    a cached first turn is replayed (it counts as a request, not as tokens); anything else
    must not repeat the client's last message, gets past the budget and the client's limits,
    and takes a stream slot that it holds until the stream ends."""
    key: bytes | None = None
    if guards.answers is not None and turn.cacheable:
        key, cached = await guards.answers.lookup(turn)
        if cached is not None:
            await guards.gate.admit_cached(client_id)
            return replay(cached)
    await guards.gate.admit(client_id)
    guards.duplicates.check(client_id, turn.fingerprint())
    slot = guards.slots.acquire(client_id)
    events: AsyncIterator[AgentEvent] = with_deadline(run(), guards.timeout_s)
    if guards.answers is not None and key is not None:
        events = recording(guards.answers, key, turn, events)
    return _holding(slot, events)


async def _holding(slot: Slot, events: AsyncIterator[AgentEvent]) -> AsyncIterator[AgentEvent]:
    try:
        async for event in events:
            yield event
    finally:
        slot.release()
