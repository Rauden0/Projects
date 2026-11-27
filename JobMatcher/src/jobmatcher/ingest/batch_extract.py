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

from jobmatcher.budget.guard import BudgetGuard, BudgetPolicy
from jobmatcher.budget.ledger import UsageLedger
from jobmatcher.config import Settings, configure_logging, get_settings
from jobmatcher.db.models import Job
from jobmatcher.db.session import get_sessionmaker
from jobmatcher.domain import JobFacts
from jobmatcher.errors import BudgetExceeded, LLMError
from jobmatcher.ingest.extraction import (
    COMPACT_SCHEMA,
    EXTRACTION_MAX_TOKENS,
    EXTRACTION_SYSTEM,
    extraction_prompt,
    parse_compact,
)
from jobmatcher.llm import usage_of

log = logging.getLogger(__name__)

CUSTOM_ID_PREFIX = "job-"
DEFAULT_LIMIT = 5000
POLL_INTERVAL_S = 60.0


@dataclass(frozen=True, slots=True)
class PendingJob:
    id: int
    title: str
    location: str | None
    description: str
    content_hash: bytes


@dataclass(frozen=True, slots=True)
class BatchOutcome:
    job_id: int
    facts: JobFacts | None


def batch_request(job: PendingJob, model: str, effort: str) -> dict[str, Any]:
    return {
        "custom_id": f"{CUSTOM_ID_PREFIX}{job.id}",
        "params": {
            "model": model,
            "max_tokens": EXTRACTION_MAX_TOKENS,
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
        sdk_errors: tuple[type[Exception], ...] = (),
    ) -> None:
        self.sdk = sdk
        self.sdk_errors = sdk_errors
        self.sessionmaker = sessionmaker
        self.ledger = ledger
        self.guard = guard
        self.model = model
        self.effort = effort
        self.poll_interval_s = poll_interval_s

    async def run(self, limit: int = DEFAULT_LIMIT) -> tuple[int, int]:
        try:
            return await self._run(limit)
        except self.sdk_errors as exc:
            status = getattr(exc, "status_code", None)
            raise LLMError(f"anthropic batch: {exc}"[:500], status) from exc

    async def _run(self, limit: int) -> tuple[int, int]:
        jobs = await self.pending_jobs(limit)
        if not jobs:
            return 0, 0
        await self.guard.check("extraction")
        requests = [batch_request(job, self.model, self.effort) for job in jobs]
        batch = await self.sdk.messages.batches.create(requests=requests)
        log.info("submitted batch %s with %d jobs", batch.id, len(requests))
        await self.wait(batch.id)
        applied = await self.apply(batch.id, {job.id: job.content_hash for job in jobs})
        return applied, len(jobs)

    async def pending_jobs(self, limit: int) -> list[PendingJob]:
        query = (
            select(Job.id, Job.title, Job.location_raw, Job.description, Job.content_hash)
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

    async def apply(self, batch_id: str, hashes: dict[int, bytes]) -> int:
        applied = 0
        async for result in await self.sdk.messages.batches.results(batch_id):
            applied += await self.apply_one(result, hashes)
        return applied

    async def apply_one(self, result: Any, hashes: dict[int, bytes]) -> int:
        outcome = outcome_of(result)
        if result.result.type == "succeeded":
            await self.ledger.record(
                purpose="extraction",
                model=self.model,
                usage=usage_of(result.result.message),
                batch=True,
            )
        content_hash = hashes.get(outcome.job_id)
        if content_hash is None:
            log.warning("batch result for unknown job %s", outcome.job_id)
            return 0
        if outcome.facts is None:
            await self.mark(outcome.job_id, content_hash, {"extraction_status": "batch_failed"})
            return 0
        values = outcome.facts.model_dump(exclude={"languages_required"})
        return await self.mark(outcome.job_id, content_hash, {**values, "extraction_status": "ok"})

    async def mark(self, job_id: int, content_hash: bytes, values: dict[str, Any]) -> int:
        stmt = (
            update(Job)
            .where(Job.id == job_id, Job.content_hash == content_hash)
            .values(**values)
            .returning(Job.id)
        )
        async with self.sessionmaker() as session, session.begin():
            updated = (await session.execute(stmt)).scalar_one_or_none()
        if updated is None:
            log.info("job %s changed during the batch, keeping its newer facts", job_id)
            return 0
        return 1


def build(settings: Settings) -> BatchExtraction:
    from anthropic import APIError, AsyncAnthropic

    api_key = settings.llm_api_key.get_secret_value()
    if settings.llm_provider != "anthropic" or not api_key:
        raise SystemExit("batch extraction needs LLM_PROVIDER=anthropic and LLM_API_KEY")
    sessionmaker = get_sessionmaker()
    ledger = UsageLedger(sessionmaker)
    policy = BudgetPolicy(settings.llm_monthly_budget_usd, settings.llm_extraction_budget_share)
    return BatchExtraction(
        AsyncAnthropic(api_key=api_key),
        sessionmaker,
        ledger,
        BudgetGuard(ledger, policy),
        settings.llm_model,
        settings.llm_effort,
        sdk_errors=(APIError,),
    )


async def loop(extraction: BatchExtraction, limit: int, every_s: float | None) -> None:
    while True:
        try:
            applied, total = await extraction.run(limit)
            log.info("batch done: %d of %d jobs extracted", applied, total)
        except BudgetExceeded as exc:
            log.warning("skipping batch: %s", exc)
        except LLMError as exc:
            log.warning("batch failed: %s", exc)
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
