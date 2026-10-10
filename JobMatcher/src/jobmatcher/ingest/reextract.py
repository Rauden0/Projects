"""Re-run LLM extraction for jobs that only got heuristic facts.

    python -m jobmatcher.ingest.reextract            # jobs with extraction_status = 'fallback'
    python -m jobmatcher.ingest.reextract --empty    # also jobs with no extracted skills

Only the extracted columns change; chunks and embeddings are left as they are, so this is
cheap to run after a local LLM was overloaded or offline during a crawl.
"""

import argparse
import asyncio
import logging

from sqlalchemy import or_, select, update

from jobmatcher.config import configure_logging
from jobmatcher.db.models import Job
from jobmatcher.db.session import get_sessionmaker
from jobmatcher.ingest.extraction import JobFacts, LLMExtractor
from jobmatcher.llm import get_llm

log = logging.getLogger(__name__)


async def reextract(include_empty: bool) -> tuple[int, int]:
    llm = get_llm()
    if llm is None:
        raise SystemExit("no LLM configured")
    extractor = LLMExtractor(llm)
    sessionmaker = get_sessionmaker()

    condition = Job.extraction_status != "ok"
    if include_empty:
        condition = or_(condition, Job.required_skills == [])
    async with sessionmaker() as session:
        jobs = (
            await session.execute(
                select(Job.id, Job.title, Job.location_raw, Job.description)
                .where(Job.status == "open", condition)
                .order_by(Job.id)
            )
        ).all()

    fixed = 0
    for job_id, title, location, description in jobs:
        try:
            facts: JobFacts = await extractor.extract(title, location, description)
        except Exception as exc:
            log.warning("job %s still failing: %s", job_id, exc)
            continue
        values = facts.model_dump(exclude={"languages_required"})
        async with sessionmaker() as session, session.begin():
            await session.execute(
                update(Job).where(Job.id == job_id).values(**values, extraction_status="ok")
            )
        fixed += 1
        log.info("re-extracted job %s (%d/%d)", job_id, fixed, len(jobs))
    return fixed, len(jobs)


def main() -> None:
    configure_logging()
    parser = argparse.ArgumentParser()
    parser.add_argument("--empty", action="store_true", help="also jobs without skills")
    args = parser.parse_args()
    fixed, total = asyncio.run(reextract(args.empty))
    log.info("done: %d of %d jobs re-extracted", fixed, total)


if __name__ == "__main__":
    main()
