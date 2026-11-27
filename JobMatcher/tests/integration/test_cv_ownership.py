import uuid
from collections.abc import AsyncIterator
from typing import Any

import httpx
import pytest
from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher.api import deps
from jobmatcher.api.main import create_app
from jobmatcher.api.routes import chat as chat_route
from jobmatcher.db.models import Cv
from jobmatcher.search.embeddings import HashingEmbedder

CV_TEXT = "Python developer v Brně, 5 let praxe s FastAPI, PostgreSQL a Dockerem. " * 2


class AdmitAll:
    async def admit(self, client_id: str) -> None:
        return None


@pytest.fixture
def app(sessionmaker: async_sessionmaker[AsyncSession], monkeypatch: pytest.MonkeyPatch) -> Any:
    async def session() -> AsyncIterator[AsyncSession]:
        async with sessionmaker() as s:
            yield s

    async def no_events(*_: Any) -> AsyncIterator[Any]:
        return
        yield

    monkeypatch.setattr(chat_route, "get_llm", lambda *_, **__: object())
    monkeypatch.setattr(chat_route, "cv_extractor", lambda _: None)
    monkeypatch.setattr(chat_route, "chat_events", no_events)
    app = create_app()
    app.dependency_overrides[deps.get_session] = session
    app.dependency_overrides[deps.get_embedder] = lambda: HashingEmbedder(384)
    app.dependency_overrides[deps.get_chat_gate] = AdmitAll
    return app


def client(app: Any) -> httpx.AsyncClient:
    return httpx.AsyncClient(transport=httpx.ASGITransport(app=app), base_url="http://api")


async def test_only_the_uploader_can_chat_with_or_delete_a_cv(
    app: Any, sessionmaker: async_sessionmaker[AsyncSession]
) -> None:
    async with client(app) as owner, client(app) as stranger:
        cv_id = (await owner.post("/cv", json={"text": CV_TEXT})).json()["cv_id"]

        chat = {"message": "hi", "cv_id": cv_id}
        assert (await stranger.post("/chat", json=chat)).status_code == 404
        assert (await stranger.delete(f"/cv/{cv_id}")).status_code == 404
        assert (await owner.post("/chat", json=chat)).status_code == 200
        assert (await owner.delete(f"/cv/{cv_id}")).status_code == 204

    async with sessionmaker() as session:
        assert (await session.execute(select(Cv))).scalars().all() == []


async def test_uploaded_cv_records_its_owner(
    app: Any, sessionmaker: async_sessionmaker[AsyncSession]
) -> None:
    async with client(app) as owner:
        cv_id = (await owner.post("/cv", json={"text": CV_TEXT})).json()["cv_id"]
    async with sessionmaker() as session:
        cv = await session.get(Cv, uuid.UUID(cv_id))
    assert cv is not None and cv.owner_id is not None and cv.owner_id.startswith("session:")


async def test_cv_from_before_owners_belongs_to_nobody(
    app: Any, sessionmaker: async_sessionmaker[AsyncSession]
) -> None:
    async with sessionmaker() as session:
        legacy = Cv(raw_text="cv", profile={}, embedding=[0.0] * 384)
        session.add(legacy)
        await session.commit()
    async with client(app) as anyone:
        assert (await anyone.delete(f"/cv/{legacy.id}")).status_code == 404
        response = await anyone.post("/chat", json={"message": "hi", "cv_id": str(legacy.id)})
        assert response.status_code == 404
