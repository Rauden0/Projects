import json
import uuid
from collections.abc import AsyncIterator
from typing import Any

from fastapi import APIRouter, HTTPException
from fastapi.responses import StreamingResponse
from pydantic import BaseModel, Field

from jobmatcher.agent.loop import run_agent
from jobmatcher.agent.tools import Toolbox
from jobmatcher.api.deps import EmbedderDep, SessionDep
from jobmatcher.cv.profile import CvProfile, build_profile
from jobmatcher.db.models import Cv
from jobmatcher.db.session import get_sessionmaker
from jobmatcher.llm import get_llm

router = APIRouter(tags=["chat"])


class CvIn(BaseModel):
    text: str = Field(min_length=50, max_length=50_000)


class CvOut(BaseModel):
    cv_id: uuid.UUID
    profile: CvProfile


class ChatIn(BaseModel):
    message: str = Field(min_length=1, max_length=4000)
    cv_id: uuid.UUID | None = None
    history: list[dict[str, Any]] = Field(default_factory=list, max_length=40)


@router.post("/cv", response_model=CvOut, status_code=201)
async def upload_cv(body: CvIn, session: SessionDep, embedder: EmbedderDep) -> CvOut:
    profile = await build_profile(body.text)
    cv = Cv(
        raw_text=body.text,
        profile=profile.model_dump(),
        embedding=await embedder.embed_query(body.text[:4000]),
    )
    session.add(cv)
    await session.commit()
    return CvOut(cv_id=cv.id, profile=profile)


def _sse(event: str, data: dict[str, Any]) -> str:
    return f"event: {event}\ndata: {json.dumps(data, ensure_ascii=False, default=str)}\n\n"


@router.post("/chat")
async def chat(body: ChatIn, session: SessionDep, embedder: EmbedderDep) -> StreamingResponse:
    llm = get_llm("chat")
    if llm is None:
        raise HTTPException(status_code=503, detail="LLM_API_KEY is not configured")

    profile_summary = ""
    if body.cv_id is not None:
        cv = await session.get(Cv, body.cv_id)
        if cv is None:
            raise HTTPException(status_code=404, detail="cv not found")
        profile_summary = json.dumps(cv.profile, ensure_ascii=False)

    async def stream() -> AsyncIterator[str]:
        # The stream outlives the request handler, so it owns its own session rather than
        # relying on the dependency's lifetime.
        async with get_sessionmaker()() as stream_session:
            toolbox = Toolbox(stream_session, embedder, body.cv_id)
            async for event in run_agent(
                llm, toolbox, body.message, body.history, profile_summary
            ):
                yield _sse(event.type, event.data)

    return StreamingResponse(stream(), media_type="text/event-stream")
