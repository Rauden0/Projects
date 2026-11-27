import json
import uuid

from sqlalchemy.ext.asyncio import AsyncSession

from jobmatcher.cv.profile import CvProfile, build_profile
from jobmatcher.db.models import Cv
from jobmatcher.errors import NotFound
from jobmatcher.search.embeddings import Embedder

EMBEDDED_CV_CHARS = 4000


async def create_cv(
    session: AsyncSession, embedder: Embedder, text: str
) -> tuple[uuid.UUID, CvProfile]:
    profile = await build_profile(text)
    cv = Cv(
        raw_text=text,
        profile=profile.model_dump(),
        embedding=await embedder.embed_query(text[:EMBEDDED_CV_CHARS]),
    )
    session.add(cv)
    await session.commit()
    return cv.id, profile


async def profile_summary(session: AsyncSession, cv_id: uuid.UUID | None) -> str:
    if cv_id is None:
        return ""
    cv = await session.get(Cv, cv_id)
    if cv is None:
        raise NotFound("cv", cv_id)
    return json.dumps(cv.profile, ensure_ascii=False)


async def delete_cv(session: AsyncSession, cv_id: uuid.UUID) -> None:
    cv = await session.get(Cv, cv_id)
    if cv is None:
        raise NotFound("cv", cv_id)
    await session.delete(cv)
    await session.commit()
