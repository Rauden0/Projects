import json
import uuid
from collections.abc import AsyncIterator
from typing import Any, Literal

from fastapi import APIRouter, Depends
from fastapi.responses import StreamingResponse
from pydantic import BaseModel, Field, model_validator

from jobmatcher.agent.session import chat_events
from jobmatcher.api.deps import (
    ChatGateDep,
    ClientKeyDep,
    EmbedderDep,
    PrincipalDep,
    SessionDep,
    reject_cross_site,
)
from jobmatcher.api.errors import ERROR_RESPONSES
from jobmatcher.config import get_settings
from jobmatcher.cv.extraction import LLMCvExtractor
from jobmatcher.cv.profile import CvExtractor, CvProfile
from jobmatcher.cv.service import create_cv, delete_cv, profile_summary
from jobmatcher.db.session import get_sessionmaker
from jobmatcher.errors import LLMUnavailable
from jobmatcher.llm import get_llm
from jobmatcher.observability import UsageMeter

router = APIRouter(
    tags=["chat"], responses=ERROR_RESPONSES, dependencies=[Depends(reject_cross_site)]
)

# The history is re-sent on every LLM round of a turn, so its size multiplies the token cost.
HISTORY_CHARS = 24_000


class CvIn(BaseModel):
    text: str = Field(min_length=50, max_length=50_000)


class CvOut(BaseModel):
    cv_id: uuid.UUID
    profile: CvProfile


class ChatTurn(BaseModel):
    role: Literal["user", "assistant"]
    content: str = Field(max_length=20_000)


class ChatIn(BaseModel):
    message: str = Field(min_length=1, max_length=4000)
    cv_id: uuid.UUID | None = None
    history: list[ChatTurn] = Field(default_factory=list, max_length=40)

    @model_validator(mode="after")
    def _history_fits(self) -> "ChatIn":
        if sum(len(turn.content) for turn in self.history) > HISTORY_CHARS:
            raise ValueError(f"history exceeds {HISTORY_CHARS} characters in total")
        return self


def cv_extractor(client_id: str) -> CvExtractor | None:
    if not get_settings().cv_llm_extraction:
        return None
    llm = get_llm("extraction", client_id=client_id)
    return None if llm is None else LLMCvExtractor(llm)


@router.post("/cv", response_model=CvOut, status_code=201)
async def upload_cv(
    body: CvIn,
    session: SessionDep,
    embedder: EmbedderDep,
    principal: PrincipalDep,
    client_id: ClientKeyDep,
) -> CvOut:
    extractor = cv_extractor(client_id)
    cv_id, profile = await create_cv(session, embedder, body.text, principal, extractor)
    return CvOut(cv_id=cv_id, profile=profile)


@router.delete("/cv/{cv_id}", status_code=204)
async def remove_cv(cv_id: uuid.UUID, session: SessionDep, principal: PrincipalDep) -> None:
    await delete_cv(session, cv_id, principal)


def _sse(event: str, data: dict[str, Any]) -> str:
    return f"event: {event}\ndata: {json.dumps(data, ensure_ascii=False, default=str)}\n\n"


@router.post("/chat", response_class=StreamingResponse)
async def chat(
    body: ChatIn,
    session: SessionDep,
    embedder: EmbedderDep,
    gate: ChatGateDep,
    principal: PrincipalDep,
    client_id: ClientKeyDep,
) -> StreamingResponse:
    meter = UsageMeter()
    llm = get_llm("chat", client_id=client_id, meter=meter)
    if llm is None:
        raise LLMUnavailable("no API key configured for the chat model")
    summary = await profile_summary(session, body.cv_id, principal)
    await gate.admit(client_id)
    history = [turn.model_dump() for turn in body.history]

    async def stream() -> AsyncIterator[str]:
        events = chat_events(
            llm, meter, get_sessionmaker(), embedder, body.cv_id, body.message, history, summary
        )
        async for event in events:
            yield _sse(event.type, event.data)

    return StreamingResponse(stream(), media_type="text/event-stream")
