"""Queries behind the response caches and the shared rate limit windows (docs/adr/0004)."""

import uuid
from datetime import datetime
from typing import Any

from sqlalchemy import delete, func, select
from sqlalchemy.dialects.postgresql import insert
from sqlalchemy.ext.asyncio import AsyncSession

from jobmatcher.db.models import ChatAnswerCache, Outbox, RateLimit


async def index_version(session: AsyncSession) -> int | None:
    """Changes whenever the job index does: every job.created / job.updated / job.closed writes
    an outbox row in the same transaction. Fact re-extraction (batch, reextract) writes none,
    so its changes reach cached responses only when they expire."""
    return (await session.execute(select(func.max(Outbox.id)))).scalar_one_or_none()


async def cached_answer(
    session: AsyncSession, key: bytes, now: datetime
) -> list[dict[str, Any]] | None:
    stmt = select(ChatAnswerCache.events).where(
        ChatAnswerCache.key == key, ChatAnswerCache.expires_at > now
    )
    return (await session.execute(stmt)).scalar_one_or_none()


async def store_answer(
    session: AsyncSession,
    key: bytes,
    cv_id: uuid.UUID | None,
    events: list[dict[str, Any]],
    expires_at: datetime,
) -> None:
    values = {"key": key, "cv_id": cv_id, "events": events, "expires_at": expires_at}
    stmt = insert(ChatAnswerCache).values(**values)
    stmt = stmt.on_conflict_do_update(
        index_elements=["key"],
        set_={"events": stmt.excluded.events, "expires_at": stmt.excluded.expires_at},
    )
    await session.execute(stmt)


async def delete_expired(session: AsyncSession, now: datetime) -> int:
    answers = await session.execute(
        delete(ChatAnswerCache)
        .where(ChatAnswerCache.expires_at <= now)
        .returning(ChatAnswerCache.key)
    )
    windows = await session.execute(
        delete(RateLimit).where(RateLimit.expires_at <= now).returning(RateLimit.key)
    )
    return len(answers.all()) + len(windows.all())
