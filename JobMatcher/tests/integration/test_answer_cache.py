from datetime import UTC, datetime, timedelta

from sqlalchemy import func, select, text
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker, create_async_engine

from jobmatcher.agent.answers import PostgresAnswerCache, Turn
from jobmatcher.agent.loop import AgentEvent
from jobmatcher.db.models import ChatAnswerCache, Cv, Outbox
from jobmatcher.retention import Retention, purge

NOW = datetime(2026, 11, 27, 12, tzinfo=UTC)
EVENTS = [
    AgentEvent("tool_call", {"name": "search_jobs", "input": {"query": "python"}}),
    AgentEvent("answer", {"text": "Zkus Heureku.", "unverified_urls": [], "rounds": 2}),
]


class Clock:
    def __init__(self) -> None:
        self.now = NOW

    def __call__(self) -> datetime:
        return self.now


def cache(sessionmaker: async_sessionmaker[AsyncSession], clock: Clock) -> PostgresAnswerCache:
    return PostgresAnswerCache(sessionmaker, timedelta(minutes=10), now=clock)


async def test_stored_answer_is_found_until_it_expires(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    clock = Clock()
    answers = cache(sessionmaker, clock)
    turn = Turn("python v Brně", None, "session:a", "", model="m")
    key, found = await answers.lookup(turn)
    assert found is None
    await answers.store(key, turn, EVENTS)
    assert (await answers.lookup(Turn("Python v brně", None, "session:b", "", model="m"))) == (
        key,
        EVENTS,
    )
    clock.now = NOW + timedelta(minutes=10)
    assert (await answers.lookup(turn))[1] is None


async def test_a_new_index_version_misses(sessionmaker: async_sessionmaker[AsyncSession]) -> None:
    answers = cache(sessionmaker, Clock())
    turn = Turn("python", None, "session:a", "", model="m")
    key, _ = await answers.lookup(turn)
    await answers.store(key, turn, EVENTS)
    async with sessionmaker.begin() as session:
        session.add(Outbox(topic="job.created", payload={}))
    new_key, found = await answers.lookup(turn)
    assert (new_key != key, found) == (True, None)


async def test_deleting_a_cv_deletes_its_answers_and_purge_drops_expired(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    clock = Clock()
    answers = cache(sessionmaker, clock)
    async with sessionmaker.begin() as session:
        await session.execute(text("TRUNCATE rate_limit"))
        cv = Cv(raw_text="cv", profile={}, embedding=[0.0] * 384, owner_id="session:a")
        session.add(cv)
    with_cv = Turn("python", cv.id, "session:a", "profile", model="m")
    shared = Turn("python", None, "session:a", "", model="m")
    for turn in (with_cv, shared):
        key, _ = await answers.lookup(turn)
        await answers.store(key, turn, EVENTS)
    async with sessionmaker.begin() as session:
        await session.delete(await session.get(Cv, cv.id))
    async with sessionmaker() as session:
        assert await session.scalar(select(func.count()).select_from(ChatAnswerCache)) == 1

    retention = Retention(timedelta(days=30), timedelta(days=2), timedelta(days=7))
    purged = await purge(sessionmaker, retention, NOW + timedelta(minutes=11))
    assert purged.cache_rows == 1
    async with sessionmaker() as session:
        assert await session.scalar(select(func.count()).select_from(ChatAnswerCache)) == 0


async def test_unreadable_cache_runs_the_turn_uncached() -> None:
    engine = create_async_engine("postgresql+asyncpg://nobody:x@127.0.0.1:1/none_test")
    answers = PostgresAnswerCache(async_sessionmaker(engine), timedelta(minutes=10))
    assert await answers.lookup(Turn("python", None, "session:a", "", model="m")) == (None, None)
    await engine.dispose()
