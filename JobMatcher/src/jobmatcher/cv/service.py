import json
import uuid

from sqlalchemy.ext.asyncio import AsyncSession

from jobmatcher.cv.profile import CvProfile, build_profile
from jobmatcher.db.models import Cv
from jobmatcher.errors import NotFound
from jobmatcher.search.embeddings import Embedder

EMBEDDED_CV_CHARS = 4000


async def create_cv(
    session: AsyncSession, embedder: Embedder, text: str, owner_id: str
) -> tuple[uuid.UUID, CvProfile]:
    profile = await build_profile(text)
    cv = Cv(
        owner_id=owner_id,
        raw_text=text,
        profile=profile.model_dump(),
        embedding=await embedder.embed_query(text[:EMBEDDED_CV_CHARS]),
    )
    session.add(cv)
    await session.commit()
    return cv.id, profile


async def owned_cv(session: AsyncSession, cv_id: uuid.UUID, owner_id: str) -> Cv:
    """Someone else's CV is reported exactly like a missing one, so ids reveal nothing. Rows
    from before owners were recorded have none and belong to nobody."""
    cv = await session.get(Cv, cv_id)
    if cv is None or cv.owner_id is None or cv.owner_id != owner_id:
        raise NotFound("cv", cv_id)
    return cv


async def profile_summary(session: AsyncSession, cv_id: uuid.UUID | None, owner_id: str) -> str:
    if cv_id is None:
        return ""
    cv = await owned_cv(session, cv_id, owner_id)
    return json.dumps(cv.profile, ensure_ascii=False)


async def delete_cv(session: AsyncSession, cv_id: uuid.UUID, owner_id: str) -> None:
    cv = await owned_cv(session, cv_id, owner_id)
    await session.delete(cv)
    await session.commit()
