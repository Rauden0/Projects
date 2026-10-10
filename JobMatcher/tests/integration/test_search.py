import pytest
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher.db.models import EMBEDDING_DIM
from jobmatcher.ingest.extraction import HeuristicExtractor
from jobmatcher.ingest.indexer import Indexer
from jobmatcher.search.embeddings import HashingEmbedder
from jobmatcher.search.hybrid import SearchFilters, search_jobs
from jobmatcher.sources import RawPosting

pytestmark = pytest.mark.integration

EMBEDDER = HashingEmbedder(EMBEDDING_DIM)

POSTINGS = [
    ("py-brno", "Python Backend Developer", "Brno", "Hybrid. 2+ years Python, FastAPI, Postgres."),
    ("py-senior", "Senior Python Engineer", "Brno", "Hybrid. 6+ years Python, Kafka."),
    ("go-remote", "Go Engineer", "Remote, Europe", "Fully remote. Go, Kubernetes."),
    ("sales", "Account Executive", "Prague", "Sales role, CRM experience."),
]


@pytest.fixture
async def indexed(sessionmaker: async_sessionmaker[AsyncSession]) -> None:
    indexer = Indexer(sessionmaker, HeuristicExtractor(), EMBEDDER)
    for external_id, title, location, body in POSTINGS:
        await indexer.index(
            RawPosting(
                kind="greenhouse",
                company="acme",
                external_id=external_id,
                url=f"https://example.com/{external_id}",
                title=title,
                location=location,
                description_html=f"<p>{body}</p>",
            )
        )


@pytest.mark.parametrize("mode", ["hybrid", "vector", "fulltext"])
async def test_search_ranks_relevant_job_first(
    sessionmaker: async_sessionmaker[AsyncSession], indexed: None, mode: str
) -> None:
    async with sessionmaker() as session:
        hits = await search_jobs(session, EMBEDDER, "python fastapi postgres", k=3, mode=mode)  # type: ignore[arg-type]
    assert hits[0].url.endswith("py-brno")


async def test_filters_exclude_by_years_and_remote(
    sessionmaker: async_sessionmaker[AsyncSession], indexed: None
) -> None:
    async with sessionmaker() as session:
        hits = await search_jobs(
            session, EMBEDDER, "python engineer", SearchFilters(max_years_required=3), k=10
        )
        remote = await search_jobs(
            session,
            EMBEDDER,
            "engineer",
            SearchFilters(remote_policy=["remote_region", "remote_global"]),
            k=10,
        )
    assert "py-senior" not in {h.url.rsplit("/", 1)[1] for h in hits}
    remote_ids = [h.url.rsplit("/", 1)[1] for h in remote]
    # Hybrid jobs are excluded; jobs with an unknown policy ("sales") are kept.
    assert "go-remote" in remote_ids
    assert not {"py-brno", "py-senior"} & set(remote_ids)
