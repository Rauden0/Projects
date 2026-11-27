from typing import Any

import pytest
from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher.db.models import EMBEDDING_DIM, Job
from jobmatcher.domain import JobFacts, RawPosting
from jobmatcher.errors import LLMError
from jobmatcher.ingest import reextract
from jobmatcher.ingest.indexer import Indexer
from jobmatcher.search.embeddings import HashingEmbedder

POSTING = RawPosting(
    kind="greenhouse",
    company="acme",
    external_id="1",
    url="https://example.com/1",
    title="Senior Python Developer",
    location="Brno",
    description_html="<p>Hybrid. 5+ years of Python and PostgreSQL.</p>",
)


class DownExtractor:
    async def extract(self, title: str, location: str | None, text: str) -> JobFacts:
        raise LLMError("connection refused")


class CompactJsonLLM:
    caches_prompts = False

    async def complete_json(self, **_: Any) -> str:
        return '{"sen": "senior", "yrs": 5, "rem": "hybrid", "req": ["python", "postgresql"]}'


async def stored(sm: async_sessionmaker[AsyncSession]) -> Job:
    async with sm() as session:
        return (await session.execute(select(Job))).scalar_one()


async def test_llm_outage_falls_back_to_heuristics_then_reextract_repairs(
    sessionmaker: async_sessionmaker[AsyncSession], monkeypatch: pytest.MonkeyPatch
) -> None:
    await Indexer(sessionmaker, DownExtractor(), HashingEmbedder(EMBEDDING_DIM)).index(POSTING)

    job = await stored(sessionmaker)
    assert job.extraction_status == "fallback"
    assert (job.seniority, job.years_required_min) == ("senior", 5)
    assert job.required_skills == []

    monkeypatch.setattr(reextract, "get_llm", lambda: CompactJsonLLM())
    monkeypatch.setattr(reextract, "get_sessionmaker", lambda: sessionmaker)
    assert await reextract.reextract(include_empty=False) == (1, 1)

    job = await stored(sessionmaker)
    assert job.extraction_status == "ok"
    assert job.required_skills == ["python", "postgresql"]
