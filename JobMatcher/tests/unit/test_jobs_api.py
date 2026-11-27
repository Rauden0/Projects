from fastapi import FastAPI
from fastapi.testclient import TestClient

from jobmatcher.api import deps
from jobmatcher.api.main import create_app
from jobmatcher.api.routes import jobs as jobs_route
from jobmatcher.search.cache import SearchCache
from jobmatcher.search.embeddings import HashingEmbedder
from jobmatcher.search.hybrid import SearchFilters, SearchHit


class Index:
    def __init__(self) -> None:
        self.version = 1

    async def __call__(self, session: object) -> int:
        return self.version


def make_app(index: Index | None = None, ttl_s: int = 60) -> FastAPI:
    async def fake_session():  # type: ignore[no-untyped-def]
        yield None

    app = create_app()
    cache = SearchCache(16, ttl_s, version=index or Index())
    app.dependency_overrides[deps.get_session] = fake_session
    app.dependency_overrides[deps.get_embedder] = lambda: HashingEmbedder(8)
    app.dependency_overrides[deps.get_search_cache] = lambda: cache
    return app


def test_search_passes_filters(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    captured: dict[str, object] = {}

    async def fake_search(session, embedder, q, filters: SearchFilters, k, mode):  # type: ignore[no-untyped-def]
        captured.update(q=q, filters=filters, k=k, mode=mode)
        return [
            SearchHit(
                job_id=1,
                title="Python Dev",
                company="acme",
                location="Brno",
                url="u",
                seniority="medior",
                years_required_min=2,
                remote_policy="hybrid",
                snippet="...",
                score=0.03,
            )
        ]

    monkeypatch.setattr(jobs_route, "search_jobs", fake_search)
    response = TestClient(make_app()).get(
        "/jobs/search",
        params={
            "q": "python",
            "remote_policy": ["hybrid", "remote_region"],
            "max_years_required": 3,
        },
    )

    assert response.status_code == 200
    assert response.json()[0]["title"] == "Python Dev"
    filters = captured["filters"]
    assert isinstance(filters, SearchFilters)
    assert filters.remote_policy == ["hybrid", "remote_region"]
    assert filters.max_years_required == 3


def test_search_validates_query() -> None:
    app = create_app()
    assert TestClient(app).get("/jobs/search", params={"q": "x"}).status_code == 422


def test_job_id_outside_bigint_is_422() -> None:
    app = create_app()
    assert TestClient(app).get("/jobs/99999999999999999999").status_code == 422
    assert TestClient(app).get("/jobs/0").status_code == 422


def test_search_is_rate_limited_per_address(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    async def fake_search(*_: object, **__: object) -> list[SearchHit]:
        return []

    monkeypatch.setenv("SEARCH_REQUESTS_PER_MINUTE", "3")
    deps.get_settings.cache_clear()
    monkeypatch.setattr(jobs_route, "search_jobs", fake_search)
    try:
        client = TestClient(make_app())
        statuses = [client.get("/jobs/search", params={"q": f"q{i}"}).status_code for i in range(4)]
    finally:
        monkeypatch.delenv("SEARCH_REQUESTS_PER_MINUTE")
        deps.get_settings.cache_clear()
    assert statuses == [200, 200, 200, 429]


def _counting_search(monkeypatch) -> list[str]:  # type: ignore[no-untyped-def]
    queries: list[str] = []

    async def fake_search(session, embedder, q, filters, k, mode):  # type: ignore[no-untyped-def]
        queries.append(q)
        return []

    monkeypatch.setattr(jobs_route, "search_jobs", fake_search)
    return queries


def test_repeated_search_is_served_from_cache_until_the_index_changes(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    queries = _counting_search(monkeypatch)
    index = Index()
    client = TestClient(make_app(index))
    first = client.get("/jobs/search", params={"q": "python  brno"})
    client.get("/jobs/search", params={"q": " python brno"})
    client.get("/jobs/search", params={"q": "python brno", "k": 5})
    index.version = 2
    client.get("/jobs/search", params={"q": "python brno"})
    assert queries == ["python brno"] * 3
    assert first.headers["cache-control"] == "public, max-age=60"


def test_search_cache_off_sends_no_cache_header(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    queries = _counting_search(monkeypatch)
    client = TestClient(make_app(ttl_s=0))
    response = client.get("/jobs/search", params={"q": "python"})
    client.get("/jobs/search", params={"q": "python"})
    assert len(queries) == 2
    assert "cache-control" not in response.headers


def test_protected_search_is_cached_privately(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    class Protected:
        protects_search = True
        limits_by_principal = True

        async def authenticate(self, request: object) -> str:
            return "oidc:alice"

    _counting_search(monkeypatch)
    app = make_app()
    app.dependency_overrides[deps.get_authenticator] = Protected
    response = TestClient(app).get("/jobs/search", params={"q": "python"})
    assert response.headers["cache-control"] == "private, max-age=60"
