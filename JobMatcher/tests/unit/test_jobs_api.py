from fastapi.testclient import TestClient

from jobmatcher.api import deps
from jobmatcher.api.main import create_app
from jobmatcher.api.routes import jobs as jobs_route
from jobmatcher.search.embeddings import HashingEmbedder
from jobmatcher.search.hybrid import SearchFilters, SearchHit


def test_search_passes_filters(monkeypatch) -> None:  # type: ignore[no-untyped-def]
    captured: dict[str, object] = {}

    async def fake_search(session, embedder, q, filters: SearchFilters, k, mode):  # type: ignore[no-untyped-def]
        captured.update(q=q, filters=filters, k=k, mode=mode)
        return [
            SearchHit(
                job_id=1, title="Python Dev", company="acme", location="Brno", url="u",
                seniority="medior", years_required_min=2, remote_policy="hybrid",
                snippet="...", score=0.03,
            )
        ]

    async def fake_session():  # type: ignore[no-untyped-def]
        yield None

    monkeypatch.setattr(jobs_route, "search_jobs", fake_search)
    app = create_app()
    app.dependency_overrides[deps.get_session] = fake_session
    app.dependency_overrides[deps.get_embedder] = lambda: HashingEmbedder(8)

    response = TestClient(app).get(
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
