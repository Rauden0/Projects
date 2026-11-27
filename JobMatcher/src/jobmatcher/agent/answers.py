"""Replays a first chat turn (no history) asked again while nothing it depends on changed: the
message, the CV profile and its owner, the model, the prompt and tools, the job index. See
docs/adr/0004."""

import hashlib
import json
import logging
import uuid
from collections.abc import AsyncIterator, Callable
from dataclasses import dataclass, field
from datetime import UTC, datetime, timedelta
from typing import Any, Protocol

from sqlalchemy.exc import SQLAlchemyError
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher import metrics
from jobmatcher.agent.loop import MAX_CALLS_PER_ROUND, MAX_ROUNDS, SYSTEM, AgentEvent
from jobmatcher.agent.tools import Toolbox
from jobmatcher.caching import normalize_text
from jobmatcher.db import cache_store

log = logging.getLogger(__name__)

# Bump when the graph changes what a turn answers without changing the prompt or the tools.
GRAPH_VERSION = 1
PROMPT_VERSION = hashlib.sha256(
    json.dumps(
        [GRAPH_VERSION, SYSTEM, Toolbox.definitions(), MAX_ROUNDS, MAX_CALLS_PER_ROUND],
        sort_keys=True,
    ).encode()
).hexdigest()


@dataclass(frozen=True, slots=True)
class Turn:
    message: str
    cv_id: uuid.UUID | None
    owner_id: str
    profile_summary: str
    history: list[dict[str, Any]] = field(default_factory=list)
    # The provider client and model; None when unknown, which disables the cache.
    model: str | None = None

    def fingerprint(self) -> str:
        material = [normalize_text(self.message).casefold(), str(self.cv_id), self.history]
        return hashlib.sha256(json.dumps(material, sort_keys=True).encode()).hexdigest()

    @property
    def cacheable(self) -> bool:
        return not self.history and self.model is not None


def answer_key(turn: Turn, index_version: int | None) -> bytes:
    """A CV-based answer is scoped to the CV and its owner, so it never reaches anyone else;
    an answer without a CV depends on nothing personal and is shared."""
    material = [
        PROMPT_VERSION,
        turn.model,
        normalize_text(turn.message).casefold(),
        str(turn.cv_id) if turn.cv_id else None,
        turn.owner_id if turn.cv_id else None,
        hashlib.sha256(turn.profile_summary.encode()).hexdigest(),
        index_version,
    ]
    return hashlib.sha256(json.dumps(material).encode()).digest()


def cacheable_events(events: list[AgentEvent]) -> bool:
    """Only a finished turn whose answer cites nothing the tools did not return."""
    if not events or events[-1].type != "answer":
        return False
    answer = events[-1].data
    return bool(answer.get("text")) and not answer.get("unverified_urls")


class AnswerCache(Protocol):
    async def lookup(self, turn: Turn) -> tuple[bytes, list[AgentEvent] | None]: ...

    async def store(self, key: bytes, turn: Turn, events: list[AgentEvent]) -> None: ...


class PostgresAnswerCache:
    """Rows in `chat_answer_cache`, shared by replicas and surviving restarts; deleting a CV
    deletes its answers (foreign key), and the retention job deletes expired rows."""

    def __init__(
        self,
        sessionmaker: async_sessionmaker[AsyncSession],
        ttl: timedelta,
        now: Callable[[], datetime] = lambda: datetime.now(UTC),
    ) -> None:
        self.sessionmaker = sessionmaker
        self.ttl = ttl
        self.now = now

    async def lookup(self, turn: Turn) -> tuple[bytes, list[AgentEvent] | None]:
        async with self.sessionmaker() as session:
            key = answer_key(turn, await cache_store.index_version(session))
            rows = await cache_store.cached_answer(session, key, self.now())
        metrics.recorder.cache_lookup("chat_answer", rows is not None)
        if rows is None:
            return key, None
        return key, [AgentEvent(row["type"], row["data"]) for row in rows]

    async def store(self, key: bytes, turn: Turn, events: list[AgentEvent]) -> None:
        rows = [{"type": e.type, "data": e.data} for e in events]
        try:
            async with self.sessionmaker.begin() as session:
                await cache_store.store_answer(
                    session, key, turn.cv_id, rows, self.now() + self.ttl
                )
        except (SQLAlchemyError, OSError) as exc:
            log.warning("chat answer not cached: %s", exc)


async def replay(events: list[AgentEvent]) -> AsyncIterator[AgentEvent]:
    for event in events:
        yield AgentEvent(event.type, {**event.data, "cached": True})


async def recording(
    cache: AnswerCache, key: bytes, turn: Turn, events: AsyncIterator[AgentEvent]
) -> AsyncIterator[AgentEvent]:
    seen: list[AgentEvent] = []
    async for event in events:
        seen.append(event)
        yield event
    if cacheable_events(seen):
        await cache.store(key, turn, seen)
