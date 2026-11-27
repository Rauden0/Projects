from datetime import UTC, datetime, timedelta

from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher.db.models import Cv, LlmUsage, Outbox
from jobmatcher.retention import Purged, Retention, purge

NOW = datetime(2026, 11, 27, 12, tzinfo=UTC)
RETENTION = Retention(
    cv=timedelta(days=30), usage_client=timedelta(days=2), outbox=timedelta(days=7)
)


def _cv(age: timedelta) -> Cv:
    return Cv(raw_text="cv", profile={}, embedding=[0.0] * 384, created_at=NOW - age)


def _usage(age: timedelta) -> LlmUsage:
    return LlmUsage(
        purpose="chat",
        client_id="10.0.0.1",
        model="m",
        input_tokens=1,
        output_tokens=1,
        cost_usd=0.0,
        created_at=NOW - age,
    )


async def test_purge_deletes_old_cvs_and_forgets_old_clients(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    async with sessionmaker.begin() as session:
        old_cv, new_cv = _cv(timedelta(days=31)), _cv(timedelta(days=1))
        old_use, new_use = _usage(timedelta(days=3)), _usage(timedelta(hours=1))
        session.add_all([old_cv, new_cv, old_use, new_use])

    purged = await purge(sessionmaker, RETENTION, NOW)

    assert (purged.cvs, purged.usage_clients, purged.outbox) == (1, 1, 0)
    async with sessionmaker() as session:
        assert list((await session.execute(select(Cv.id))).scalars()) == [new_cv.id]
        clients = dict((await session.execute(select(LlmUsage.id, LlmUsage.client_id))).all())
    assert clients == {old_use.id: None, new_use.id: "10.0.0.1"}


async def test_purge_twice_is_a_no_op(sessionmaker: async_sessionmaker[AsyncSession]) -> None:
    async with sessionmaker.begin() as session:
        session.add(_cv(timedelta(days=40)))
    await purge(sessionmaker, RETENTION, NOW)
    assert await purge(sessionmaker, RETENTION, NOW) == Purged(0, 0, 0)


def _outbox(created: timedelta, published: timedelta | None) -> Outbox:
    return Outbox(
        topic="job.indexed",
        payload={},
        created_at=NOW - created,
        published_at=None if published is None else NOW - published,
    )


async def test_purge_deletes_only_outbox_rows_published_before_retention(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    async with sessionmaker.begin() as session:
        old = _outbox(timedelta(days=9), timedelta(days=8))
        recent = _outbox(timedelta(days=9), timedelta(days=1))
        unpublished = _outbox(timedelta(days=30), None)
        session.add_all([old, recent, unpublished])

    purged = await purge(sessionmaker, RETENTION, NOW)

    assert purged.outbox == 1
    async with sessionmaker() as session:
        left = set((await session.execute(select(Outbox.id))).scalars())
    assert left == {recent.id, unpublished.id}
