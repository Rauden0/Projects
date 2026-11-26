"""Extract facts for pending jobs through the Message Batches API at half the token price.

python -m jobmatcher.ingest.batch_extract              # one batch, wait for results
python -m jobmatcher.ingest.batch_extract --every 3600 # keep running, one batch an hour
"""

import argparse
import asyncio
import logging
from collections.abc import Sequence
from dataclasses import dataclass
from typing import Any

from pydantic import ValidationError
from sqlalchemy import select, update
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher.budget.guard import BudgetExceeded, BudgetGuard, BudgetPolicy
from jobmatcher.budget.ledger import UsageLedger
from jobmatcher.config import Settings, configure_logging, get_settings
from jobmatcher.db.models import Job
from jobmatcher.db.session import get_sessionmaker
from jobmatcher.ingest.extraction import (
    COMPACT_SCHEMA,
    EXTRACTION_SYSTEM,
    JobFacts,
    extraction_prompt,
    parse_compact,
)
from jobmatcher.llm import usage_of

log = logging.getLogger(__name__)

CUSTOM_ID_PREFIX = "job-"
DEFAULT_LIMIT = 5000
POLL_INTERVAL_S = 60.0
MAX_OUTPUT_TOKENS = 400


@dataclass(frozen=True, slots=True)
class PendingJob:
    id: int
    title: str
    location: str | None
    description: str


@dataclass(frozen=True, slots=True)
class BatchOutcome:
    job_id: int
    facts: JobFacts | None


def batch_request(job: PendingJob, model: str, effort: str) -> dict[str, Any]:
    return {
        "custom_id": f"{CUSTOM_ID_PREFIX}{job.id}",
        "params": {
            "model": model,
            "max_tokens": MAX_OUTPUT_TOKENS,
            "system": EXTRACTION_SYSTEM,
            "thinking": {"type": "disabled"},
            "output_config": {
                "effort": effort,
                "format": {"type": "json_schema", "schema": COMPACT_SCHEMA},
            },
            "messages": [
                {
                    "role": "user",
                    "content": extraction_prompt(job.title, job.location, job.description),
                }
            ],
        },
    }


def job_id_of(custom_id: str) -> int:
    return int(custom_id.removeprefix(CUSTOM_ID_PREFIX))


def outcome_of(result: Any) -> BatchOutcome:
    job_id = job_id_of(result.custom_id)
    if result.result.type != "succeeded":
        return BatchOutcome(job_id, None)
    text = "".join(b.text for b in result.result.message.content if b.type == "text")
    try:
        return BatchOutcome(job_id, parse_compact(text))
    except ValidationError:
        return BatchOutcome(job_id, None)


class BatchExtraction:
    def __init__(
        self,
        sdk: Any,
        sessionmaker: async_sessionmaker[AsyncSession],
        ledger: UsageLedger,
        guard: BudgetGuard,
        model: str,
        effort: str = "low",
        poll_interval_s: float = POLL_INTERVAL_S,
    ) -> None:
        self.sdk = sdk
        self.sessionmaker = sessionmaker
        self.ledger = ledger
        self.guard = guard
        self.model = model
        self.effort = effort
        self.poll_interval_s = poll_interval_s

    async def run(self, limit: int = DEFAULT_LIMIT) -> tuple[int, int]:
        jobs = await self.pending_jobs(limit)
        if not jobs:
            return 0, 0
        await self.guard.check("extraction")
        requests = [batch_request(job, self.model, self.effort) for job in jobs]
        batch = await self.sdk.messages.batches.create(requests=requests)
        log.info("submitted batch %s with %d jobs", batch.id, len(requests))
        await self.wait(batch.id)
        applied = await self.apply(batch.id)
        return applied, len(jobs)

    async def pending_jobs(self, limit: int) -> list[PendingJob]:
        query = (
            select(Job.id, Job.title, Job.location_raw, Job.description)
            .where(Job.status == "open", Job.extraction_status.in_(("pending", "fallback")))
            .order_by(Job.id)
            .limit(limit)
        )
        async with self.sessionmaker() as session:
            rows = (await session.execute(query)).all()
        return [PendingJob(*row) for row in rows]

    async def wait(self, batch_id: str) -> None:
        while True:
            batch = await self.sdk.messages.batches.retrieve(batch_id)
            if batch.processing_status == "ended":
                return
            await asyncio.sleep(self.poll_interval_s)

    async def apply(self, batch_id: str) -> int:
        applied = 0
        async for result in await self.sdk.messages.batches.results(batch_id):
            applied += await self.apply_one(result)
        return applied

    async def apply_one(self, result: Any) -> int:
        outcome = outcome_of(result)
        if result.result.type == "succeeded":
            await self.ledger.record(
                purpose="extraction",
                model=self.model,
                usage=usage_of(result.result.message),
                batch=True,
            )
        if outcome.facts is None:
            await self.mark(outcome.job_id, {"extraction_status": "fallback"})
            return 0
        values = outcome.facts.model_dump(exclude={"languages_required"})
        await self.mark(outcome.job_id, {**values, "extraction_status": "ok"})
        return 1

    async def mark(self, job_id: int, values: dict[str, Any]) -> None:
        async with self.sessionmaker() as session, session.begin():
            await session.execute(update(Job).where(Job.id == job_id).values(**values))


def build(settings: Settings) -> BatchExtraction:
    from anthropic import AsyncAnthropic

    if settings.llm_provider != "anthropic" or not settings.llm_api_key:
        raise SystemExit("batch extraction needs LLM_PROVIDER=anthropic and LLM_API_KEY")
    sessionmaker = get_sessionmaker()
    ledger = UsageLedger(sessionmaker)
    policy = BudgetPolicy(settings.llm_monthly_budget_usd, settings.llm_extraction_budget_share)
    return BatchExtraction(
        AsyncAnthropic(api_key=settings.llm_api_key),
        sessionmaker,
        ledger,
        BudgetGuard(ledger, policy),
        settings.llm_model,
        settings.llm_effort,
    )


async def loop(extraction: BatchExtraction, limit: int, every_s: float | None) -> None:
    while True:
        try:
            applied, total = await extraction.run(limit)
            log.info("batch done: %d of %d jobs extracted", applied, total)
        except BudgetExceeded as exc:
            log.warning("skipping batch: %s", exc)
        if every_s is None:
            return
        await asyncio.sleep(every_s)


def main(argv: Sequence[str] | None = None) -> None:
    configure_logging()
    parser = argparse.ArgumentParser()
    parser.add_argument("--limit", type=int, default=DEFAULT_LIMIT)
    parser.add_argument("--every", type=float, default=None, help="seconds between batches")
    args = parser.parse_args(argv)
    asyncio.run(loop(build(get_settings()), args.limit, args.every))


if __name__ == "__main__":
    main()
