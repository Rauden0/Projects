from dataclasses import dataclass, field
from types import SimpleNamespace
from typing import Any

from sqlalchemy import select, text
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher.budget.guard import BudgetGuard, BudgetPolicy
from jobmatcher.budget.ledger import UsageLedger
from jobmatcher.db.models import EMBEDDING_DIM, Job
from jobmatcher.ingest.batch_extract import BatchExtraction
from jobmatcher.ingest.extraction import HeuristicExtractor
from jobmatcher.ingest.indexer import Indexer
from jobmatcher.search.embeddings import HashingEmbedder
from jobmatcher.sources import RawPosting

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

    async def create(self, requests: list[dict[str, Any]]) -> SimpleNamespace:
        self.requests = requests
        return SimpleNamespace(id="batch_1")

    async def retrieve(self, batch_id: str) -> SimpleNamespace:
        self.polls += 1
        return SimpleNamespace(processing_status="ended" if self.polls > 1 else "in_progress")

    async def results(self, batch_id: str) -> Results:
        first, second = (r["custom_id"] for r in self.requests)
        return Results([succeeded(first, GOOD), succeeded(second, "not json")])


async def test_batch_extracts_pending_jobs_and_records_usage(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    async with sessionmaker() as session, session.begin():
        await session.execute(text("TRUNCATE llm_usage"))
    indexer = Indexer(
        sessionmaker, HeuristicExtractor(), HashingEmbedder(EMBEDDING_DIM), defer_extraction=True
    )
    for external_id in ("1", "2"):
        await indexer.index(
            RawPosting(
                kind="greenhouse",
                company="acme",
                external_id=external_id,
                url=f"https://example.com/{external_id}",
                title=f"Go Developer {external_id}",
                location="Brno",
                description_html="<p>Go</p>",
            )
        )
    batches = FakeBatches()
    ledger = UsageLedger(sessionmaker)
    extraction = BatchExtraction(
        SimpleNamespace(messages=SimpleNamespace(batches=batches)),
        sessionmaker,
        ledger,
        BudgetGuard(ledger, BudgetPolicy(40.0)),
        "claude-haiku-5-5",
        poll_interval_s=0,
    )

    assert await extraction.run() == (1, 2)

    async with sessionmaker() as session:
        rows = (
            await session.execute(select(Job.extraction_status, Job.seniority).order_by(Job.id))
        ).all()
        batch_rows = (
            await session.execute(text("SELECT count(*) FROM llm_usage WHERE batch"))
        ).scalar_one()
    assert [tuple(r) for r in rows] == [("ok", "senior"), ("fallback", None)]
    assert batch_rows == 2
    assert batches.requests[0]["params"]["output_config"]["format"]["type"] == "json_schema"
