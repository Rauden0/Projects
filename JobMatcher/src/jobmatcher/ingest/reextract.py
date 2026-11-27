"""Re-run LLM extraction for jobs that only got heuristic facts.

    python -m jobmatcher.ingest.reextract            # jobs with extraction_status = 'fallback'
    python -m jobmatcher.ingest.reextract --empty    # also jobs with no extracted skills
    python -m jobmatcher.ingest.reextract --outdated # also jobs from an older EXTRACTION_VERSION

Only the extracted columns change; chunks and embeddings are left as they are, so this is
cheap to run after a local LLM was overloaded or offline during a crawl.
"""

import argparse
import asyncio
import logging

from sqlalchemy import ColumnElement, or_, select, update

from jobmatcher.config import configure_logging, configure_tracing
from jobmatcher.db.models import Job
from jobmatcher.db.session import get_sessionmaker
from jobmatcher.domain import JobFacts
from jobmatcher.errors import ExtractionError, LLMError
from jobmatcher.ingest.extraction import EXTRACTION_VERSION, LLMExtractor
from jobmatcher.llm import get_llm

log = logging.getLogger(__name__)


def selection(include_empty: bool, include_outdated: bool) -> ColumnElement[bool]:
    condition: ColumnElement[bool] = Job.extraction_status != "ok"
    if include_empty:
        condition = or_(condition, Job.required_skills == [])
    if include_outdated:
        outdated = or_(
            Job.extraction_version.is_(None), Job.extraction_version < EXTRACTION_VERSION
        )
        condition = or_(condition, outdated)
    return condition


async def reextract(include_empty: bool, include_outdated: bool = False) -> tuple[int, int]:
    llm = get_llm()
    if llm is None:
        raise SystemExit("no LLM configured")
    extractor = LLMExtractor(llm)
    sessionmaker = get_sessionmaker()

    condition = selection(include_empty, include_outdated)
    async with sessionmaker() as session:
        jobs = (
            await session.execute(
                select(Job.id, Job.title, Job.location_raw, Job.description, Job.content_hash)
                .where(Job.status == "open", condition)
                .order_by(Job.id)
            )
        ).all()

    fixed = 0
    for job_id, title, location, description, digest in jobs:
        try:
            facts: JobFacts = await extractor.extract(title, location, description)
        except (LLMError, ExtractionError) as exc:
            log.warning("job %s still failing: %s", job_id, exc)
            continue
        values = facts.model_dump(exclude={"languages_required"})
        stmt = (
            update(Job)
            .where(Job.id == job_id, Job.content_hash == digest)
            .values(**values, extraction_status="ok", extraction_version=EXTRACTION_VERSION)
            .returning(Job.id)
        )
        async with sessionmaker() as session, session.begin():
            updated = (await session.execute(stmt)).scalar_one_or_none()
        if updated is None:
            log.info("job %s changed meanwhile, keeping its newer facts", job_id)
            continue
        fixed += 1
        log.info("re-extracted job %s (%d/%d)", job_id, fixed, len(jobs))
    return fixed, len(jobs)


def main() -> None:
    configure_logging()
    configure_tracing()
    parser = argparse.ArgumentParser()
    parser.add_argument("--empty", action="store_true", help="also jobs without skills")
    parser.add_argument(
        "--outdated", action="store_true", help="also jobs from an older extraction version"
    )
    args = parser.parse_args()
    fixed, total = asyncio.run(reextract(args.empty, args.outdated))
    log.info("done: %d of %d jobs re-extracted", fixed, total)


if __name__ == "__main__":
    main()
