import pytest
from sqlalchemy import text
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher.db import repository
from jobmatcher.db.models import EMBEDDING_DIM
from jobmatcher.domain import RawPosting
from jobmatcher.ingest.extraction import HeuristicExtractor
from jobmatcher.ingest.indexer import Indexer
from jobmatcher.search import hybrid
from jobmatcher.search.embeddings import HashingEmbedder
from jobmatcher.search.hybrid import SearchFilters, search_jobs

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


async def test_thin_board_copy_is_hidden_when_full_copy_exists(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    indexer = Indexer(sessionmaker, HeuristicExtractor(), EMBEDDER)
    full_text = "<p>" + "Python data collection and MES integration. " * 20 + "</p>"
    for kind, company, external_id, body in [
        ("jobscz", "Thermo Fisher Scientific", "board", "<p>Python Developer</p>"),
        ("workday", "thermofisher", "ats", full_text),
    ]:
        await indexer.index(
            RawPosting(
                kind=kind,
                company=company,
                external_id=external_id,
                url=f"https://example.com/{external_id}",
                title="Python Developer - Manufacturing",
                location="Brno",
                description_html=body,
            )
        )

    async with sessionmaker() as session:
        hits = await search_jobs(session, EMBEDDER, "python developer", k=10)
    assert [h.url.rsplit("/", 1)[1] for h in hits] == ["ats"]


async def test_city_filter_ranks_local_job_above_equal_remote_job(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    indexer = Indexer(sessionmaker, HeuristicExtractor(), EMBEDDER)
    for external_id, location, body in [
        ("remote", "Remote, Europe", "<p>Fully remote. Python, FastAPI, Python, FastAPI.</p>"),
        ("local", "Brno", "<p>Hybrid, office in Brno. Python, FastAPI.</p>"),
    ]:
        await indexer.index(
            RawPosting(
                kind="greenhouse",
                company="acme",
                external_id=external_id,
                url=f"https://example.com/{external_id}",
                title="Python Developer",
                location=location,
                description_html=body,
            )
        )

    async with sessionmaker() as session:
        plain = await search_jobs(session, EMBEDDER, "python fastapi", k=10)
        in_brno = await search_jobs(
            session, EMBEDDER, "python fastapi", SearchFilters(city="Brno"), k=10
        )
    assert [h.url.rsplit("/", 1)[1] for h in plain] == ["remote", "local"]
    assert [h.url.rsplit("/", 1)[1] for h in in_brno] == ["local", "remote"]


FULL_TEXT = "<p>" + "Python data collection and MES integration. " * 20 + "</p>"


async def index_copies(
    sm: async_sessionmaker[AsyncSession], copies: list[tuple[str, str, str, str]]
) -> None:
    indexer = Indexer(sm, HeuristicExtractor(), EMBEDDER)
    for kind, company, external_id, body in copies:
        await indexer.index(
            RawPosting(
                kind=kind,
                company=company,
                external_id=external_id,
                url=f"https://example.com/{external_id}",
                title="Python Developer - Manufacturing",
                location="Brno",
                description_html=body,
            )
        )


async def found(sm: async_sessionmaker[AsyncSession]) -> list[str]:
    async with sm() as session:
        hits = await search_jobs(session, EMBEDDER, "python developer", k=10)
    return [h.url.rsplit("/", 1)[1] for h in hits]


async def test_full_copy_from_another_source_is_hidden(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    longer = FULL_TEXT.replace("</p>", " Apply now.</p>")
    await index_copies(
        sessionmaker,
        [
            ("jobscz", "Thermo Fisher", "board", FULL_TEXT),
            ("workday", "thermofisher", "ats", longer),
        ],
    )
    assert await found(sessionmaker) == ["ats"]


async def test_full_postings_from_one_source_both_stay(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    await index_copies(
        sessionmaker,
        [("workday", "acme", "a", FULL_TEXT), ("workday", "acme", "b", FULL_TEXT)],
    )
    assert sorted(await found(sessionmaker)) == ["a", "b"]


async def test_stub_resurfaces_when_full_copy_closes(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    await index_copies(
        sessionmaker,
        [
            ("jobscz", "acme", "board", "<p>Python Developer</p>"),
            ("workday", "acme", "ats", FULL_TEXT),
        ],
    )
    async with sessionmaker() as session, session.begin():
        source_id = await repository.get_or_create_source(session, "workday", "acme")
        await repository.close_unseen_jobs(session, source_id, {"other"})

    assert await found(sessionmaker) == ["board"]


async def test_title_match_ignores_accents(sessionmaker: async_sessionmaker[AsyncSession]) -> None:
    indexer = Indexer(sessionmaker, HeuristicExtractor(), EMBEDDER)
    await indexer.index(
        RawPosting(
            kind="jobscz",
            company="acme",
            external_id="cz",
            url="https://example.com/cz",
            title="Vývojář Python",
            location="Brno",
            description_html="<p>Backend.</p>",
        )
    )
    async with sessionmaker() as session:
        hits = await search_jobs(session, EMBEDDER, "vyvojar", k=5, mode="hybrid")
        titles = (
            await session.execute(
                text("SELECT id FROM job WHERE title_tsv @@ to_tsquery('simple', 'vyvojar')")
            )
        ).all()
    assert [h.url for h in hits] == ["https://example.com/cz"]
    assert len(titles) == 1


async def test_long_posting_takes_one_vector_slot(
    sessionmaker: async_sessionmaker[AsyncSession], monkeypatch: pytest.MonkeyPatch
) -> None:
    monkeypatch.setattr(hybrid, "CANDIDATES", 2)
    monkeypatch.setattr(hybrid, "VECTOR_CHUNKS", 20)
    indexer = Indexer(sessionmaker, HeuristicExtractor(), EMBEDDER)
    sections = "".join(f"<h3>Part {n}:</h3><p>{'kotlin android ' * 400}</p>" for n in range(3))
    for external_id, body in [("long", sections), ("short", "<p>kotlin</p>")]:
        await indexer.index(
            RawPosting(
                kind="greenhouse",
                company="acme",
                external_id=external_id,
                url=f"https://example.com/{external_id}",
                title=f"Kotlin Developer {external_id}",
                location="Brno",
                description_html=body,
            )
        )
    async with sessionmaker() as session:
        hits = await search_jobs(session, EMBEDDER, "kotlin android", k=5, mode="vector")
    assert sorted(h.url.rsplit("/", 1)[1] for h in hits) == ["long", "short"]
