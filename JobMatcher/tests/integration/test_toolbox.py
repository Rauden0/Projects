from datetime import date

import pytest
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher.agent.tools import Toolbox, ToolError
from jobmatcher.cv.profile import build_profile
from jobmatcher.db.models import EMBEDDING_DIM, Cv
from jobmatcher.ingest.extraction import HeuristicExtractor
from jobmatcher.ingest.indexer import Indexer
from jobmatcher.search.embeddings import HashingEmbedder
from jobmatcher.sources import RawPosting

pytestmark = pytest.mark.integration

EMBEDDER = HashingEmbedder(EMBEDDING_DIM)
CV_TEXT = "Python developer, Brno. 01/2024 - 01/2026. Python, FastAPI, PostgreSQL, Docker."


async def test_search_then_compare(sessionmaker: async_sessionmaker[AsyncSession]) -> None:
    indexer = Indexer(sessionmaker, HeuristicExtractor(), EMBEDDER)
    result = await indexer.index(
        RawPosting(
            kind="greenhouse",
            company="acme",
            external_id="1",
            url="https://example.com/1",
            title="Python Developer",
            location="Brno",
            description_html=(
                "<p>Hybrid in Brno.</p><h3>Requirements</h3>"
                "<ul><li>3+ years of Python and PostgreSQL</li><li>Kubernetes</li></ul>"
                "<h3>Nice to have</h3><ul><li>Terraform</li></ul>"
            ),
        )
    )

    async with sessionmaker() as session:
        profile = await build_profile(CV_TEXT, today=date(2026, 10, 1))
        cv = Cv(raw_text=CV_TEXT, profile=profile.model_dump(),
                embedding=await EMBEDDER.embed_query(CV_TEXT))
        session.add(cv)
        await session.commit()

        toolbox = Toolbox(session, EMBEDDER, cv.id)
        hits = await toolbox.call("search_jobs", {"query": "python postgresql", "city": "Brno"})
        comparison = await toolbox.call("compare_with_cv", {"job_id": result.job_id})

        with pytest.raises(ToolError):
            await toolbox.call("get_job", {"job_id": 999})

    assert hits[0]["job_id"] == result.job_id
    assert "https://example.com/1" in toolbox.seen_urls
    assert comparison["missing_required"] == ["kubernetes"]
    assert comparison["missing_nice_to_have"] == ["terraform"]
    assert comparison["years_gap"] == 1.0
    assert comparison["location_fit"] == "same_city"
