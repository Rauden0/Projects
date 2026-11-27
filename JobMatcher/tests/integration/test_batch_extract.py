from collections.abc import Awaitable, Callable
from dataclasses import dataclass, field
from types import SimpleNamespace
from typing import Any

from sqlalchemy import select, text
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher.budget.guard import BudgetGuard, BudgetPolicy
from jobmatcher.budget.ledger import UsageLedger
from jobmatcher.db.models import EMBEDDING_DIM, Job
from jobmatcher.domain import RawPosting
from jobmatcher.ingest.batch_extract import BatchExtraction
from jobmatcher.ingest.extraction import HeuristicExtractor
from jobmatcher.ingest.indexer import Indexer
from jobmatcher.search.embeddings import HashingEmbedder

GOOD = (
    '{"sen": "senior", "yrs": 5, "rem": null, "city": null, "smin": null, "smax": null,'
    ' "cur": null, "ct": null, "tech": ["go"], "req": ["go"], "nice": [], "lang": []}'
)


def succeeded(custom_id: str, body: str) -> SimpleNamespace:
    message = SimpleNamespace(
        content=[SimpleNamespace(type="text", text=body)],
        usage=SimpleNamespace(input_tokens=800, output_tokens=60),
    )
    return SimpleNamespace(
        custom_id=custom_id, result=SimpleNamespace(type="succeeded", message=message)
    )


class Results:
    def __init__(self, items: list[SimpleNamespace]) -> None:
        self.items = items

    def __aiter__(self) -> "Results":
        return self

    async def __anext__(self) -> SimpleNamespace:
        if not self.items:
            raise StopAsyncIteration
        return self.items.pop(0)


@dataclass
class FakeBatches:
    requests: list[dict[str, Any]] = field(default_factory=list)
    polls: int = 0
    created: int = 0
    before_results: Callable[[], Awaitable[None]] | None = None

    async def create(self, requests: list[dict[str, Any]]) -> SimpleNamespace:
        self.requests = requests
        self.created += 1
        return SimpleNamespace(id="batch_1")

    async def retrieve(self, batch_id: str) -> SimpleNamespace:
        self.polls += 1
        return SimpleNamespace(processing_status="ended" if self.polls > 1 else "in_progress")

    async def results(self, batch_id: str) -> Results:
        if self.before_results is not None:
            await self.before_results()
        first, second = (r["custom_id"] for r in self.requests)
        return Results([succeeded(first, GOOD), succeeded(second, "not json")])


def posting(external_id: str, description: str = "<p>Go</p>") -> RawPosting:
    return RawPosting(
        kind="greenhouse",
        company="acme",
        external_id=external_id,
        url=f"https://example.com/{external_id}",
        title=f"Go Developer {external_id}",
        location="Brno",
        description_html=description,
    )


def deferred_indexer(sessionmaker: async_sessionmaker[AsyncSession]) -> Indexer:
    return Indexer(
        sessionmaker, HeuristicExtractor(), HashingEmbedder(EMBEDDING_DIM), defer_extraction=True
    )


def extraction_with(
    sessionmaker: async_sessionmaker[AsyncSession], batches: FakeBatches
) -> BatchExtraction:
    ledger = UsageLedger(sessionmaker)
    return BatchExtraction(
        SimpleNamespace(messages=SimpleNamespace(batches=batches)),
        sessionmaker,
        ledger,
        BudgetGuard(ledger, BudgetPolicy(40.0)),
        "claude-haiku-5-5",
        poll_interval_s=0,
    )


async def statuses(sessionmaker: async_sessionmaker[AsyncSession]) -> list[tuple[Any, ...]]:
    async with sessionmaker() as session:
        rows = await session.execute(
            select(Job.extraction_status, Job.seniority, Job.description).order_by(Job.id)
        )
        return [tuple(r) for r in rows]


async def test_batch_extracts_pending_jobs_and_records_usage(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    async with sessionmaker() as session, session.begin():
        await session.execute(text("TRUNCATE llm_usage"))
    for external_id in ("1", "2"):
        await deferred_indexer(sessionmaker).index(posting(external_id))
    batches = FakeBatches()
    extraction = extraction_with(sessionmaker, batches)

    assert await extraction.run() == (1, 2)

    async with sessionmaker() as session:
        batch_rows = (
            await session.execute(text("SELECT count(*) FROM llm_usage WHERE batch"))
        ).scalar_one()
    assert [r[:2] for r in await statuses(sessionmaker)] == [
        ("ok", "senior"),
        ("batch_failed", None),
    ]
    assert batch_rows == 2
    assert batches.requests[0]["params"]["output_config"]["format"]["type"] == "json_schema"


async def test_failed_batch_jobs_are_not_resubmitted(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    for external_id in ("1", "2"):
        await deferred_indexer(sessionmaker).index(posting(external_id))
    batches = FakeBatches()
    extraction = extraction_with(sessionmaker, batches)
    await extraction.run()

    assert await extraction.run() == (0, 0)
    assert batches.created == 1


async def test_job_changed_during_batch_keeps_newer_content(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    indexer = deferred_indexer(sessionmaker)
    for external_id in ("1", "2"):
        await indexer.index(posting(external_id))

    async def reindex_first() -> None:
        await indexer.index(posting("1", "<p>Rust now</p>"))

    batches = FakeBatches(before_results=reindex_first)

    assert await extraction_with(sessionmaker, batches).run() == (0, 2)
    rows = await statuses(sessionmaker)
    assert rows[0] == ("pending", None, "Rust now")
    assert rows[1][0] == "batch_failed"
