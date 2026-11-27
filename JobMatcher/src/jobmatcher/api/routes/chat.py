import json
import uuid
from collections.abc import AsyncIterator
from typing import Any, Literal

from fastapi import APIRouter, Request
from fastapi.responses import StreamingResponse
from pydantic import BaseModel, Field, model_validator

from jobmatcher.agent.session import chat_events
from jobmatcher.api.deps import ChatGateDep, EmbedderDep, SessionDep
from jobmatcher.api.errors import ERROR_RESPONSES
from jobmatcher.cv.profile import CvProfile
from jobmatcher.cv.service import create_cv, profile_summary
from jobmatcher.db.session import get_sessionmaker
from jobmatcher.errors import LLMUnavailable
from jobmatcher.llm import get_llm
from jobmatcher.observability import UsageMeter

router = APIRouter(tags=["chat"], responses=ERROR_RESPONSES)

CLIENT_ID_CHARS = 64
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


@router.post("/cv", response_model=CvOut, status_code=201)
async def upload_cv(body: CvIn, session: SessionDep, embedder: EmbedderDep) -> CvOut:
    cv_id, profile = await create_cv(session, embedder, body.text)
    return CvOut(cv_id=cv_id, profile=profile)


def _sse(event: str, data: dict[str, Any]) -> str:
    return f"event: {event}\ndata: {json.dumps(data, ensure_ascii=False, default=str)}\n\n"


def client_id_of(request: Request) -> str:
    header = request.headers.get("x-client-id")
    if header:
        return header[:CLIENT_ID_CHARS]
    return request.client.host if request.client else "anonymous"


@router.post("/chat", response_class=StreamingResponse)
async def chat(
    body: ChatIn,
    request: Request,
    session: SessionDep,
    embedder: EmbedderDep,
    gate: ChatGateDep,
) -> StreamingResponse:
    client_id = client_id_of(request)
    await gate.admit(client_id)
    meter = UsageMeter()
    llm = get_llm("chat", client_id=client_id, meter=meter)
    if llm is None:
        raise LLMUnavailable("no API key configured for the chat model")
    summary = await profile_summary(session, body.cv_id)
    history = [turn.model_dump() for turn in body.history]

    async def stream() -> AsyncIterator[str]:
        events = chat_events(
            llm, meter, get_sessionmaker(), embedder, body.cv_id, body.message, history, summary
        )
        async for event in events:
            yield _sse(event.type, event.data)

    return StreamingResponse(stream(), media_type="text/event-stream")
