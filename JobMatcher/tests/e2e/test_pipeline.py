"""job.discovered on the broker -> worker indexes it -> /jobs/search finds it -> /chat cites it.

Everything runs in process against the test Postgres and the test RabbitMQ vhost. The chat
model is a scripted stub by default; E2E_LLM=local uses the configured local model (Ollama at
E2E_LLM_BASE_URL, default http://localhost:11434/v1, model E2E_LLM_MODEL, default qwen2.5:7b).
"""

import asyncio
import gzip
import json
import os
import re
from collections.abc import AsyncIterator
from typing import Any

import aio_pika
import httpx
import pytest
from aio_pika.abc import AbstractChannel
from sqlalchemy import func, select
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher import messaging
from jobmatcher.api import deps
from jobmatcher.api.main import create_app
from jobmatcher.api.routes import chat as chat_route
from jobmatcher.db.models import EMBEDDING_DIM, Job
from jobmatcher.domain import RawPosting
from jobmatcher.ingest.extraction import HeuristicExtractor
from jobmatcher.ingest.indexer import Indexer
from jobmatcher.ingest.worker import start_consumers
from jobmatcher.llm import LLMClient, OpenAICompatClient
from jobmatcher.observability import UsageMeter
from jobmatcher.search.embeddings import HashingEmbedder

EMBEDDER = HashingEmbedder(EMBEDDING_DIM)
POSTINGS = [
    RawPosting(
        kind="greenhouse",
        company="Moravia Data",
        external_id="py-1",
        url="https://moravia.example/jobs/py-1",
        title="Python Backend Developer",
        location="Brno, Czechia",
        description_html=(
            "<p>Hybrid, 2 days a week in Brno.</p><h3>Requirements</h3>"
            "<ul><li>2+ years of Python</li><li>FastAPI, PostgreSQL, RabbitMQ</li></ul>"
        ),
    ),
    RawPosting(
        kind="jobscz",
        company="Dopravní společnost Morava",
        source="jobscz-brno",
        external_id="drv-7",
        url="https://jobs.example/rpd/drv-7",
        title="Řidič nákladního vozidla sk. C",
        location="Brno",
        description_html=(
            "<p>Rozvoz zboží po Jihomoravském kraji.</p><h3>Požadujeme</h3>"
            "<ul><li>řidičské oprávnění sk. C</li><li>profesní průkaz</li></ul>"
        ),
    ),
]


class ScriptedChatModel:
    """Searches once, then answers citing the first URL the search returned."""

    caches_prompts = False

    def __init__(self) -> None:
        self.rounds = 0

    async def complete(self, *, system: str, user: str, max_tokens: int = 1024) -> str:
        raise NotImplementedError

    async def complete_json(
        self, *, system: str, user: str, schema: dict[str, Any], max_tokens: int = 512
    ) -> str:
        raise NotImplementedError

    async def create_message(self, **kwargs: Any) -> dict[str, Any]:
        self.rounds += 1
        if self.rounds == 1:
            call = {"type": "tool_use", "id": "t1", "name": "search_jobs"}
            return {"content": [{**call, "input": {"query": "python backend", "city": "Brno"}}]}
        result = kwargs["messages"][-1]["content"][0]["content"]
        url = json.loads(result)[0]["url"]
        return {"content": [{"type": "text", "text": f"Python Backend Developer: {url}"}]}


def chat_model(meter: UsageMeter) -> LLMClient:
    if os.environ.get("E2E_LLM") != "local":
        return ScriptedChatModel()
    return OpenAICompatClient(
        os.environ.get("E2E_LLM_BASE_URL", "http://localhost:11434/v1"),
        os.environ.get("E2E_LLM_MODEL", "qwen2.5:7b"),
        timeout=300,
        purpose="chat",
        meter=meter,
    )


class AdmitAll:
    async def admit(self, client_id: str) -> None:
        return None


@pytest.fixture
async def api(monkeypatch: pytest.MonkeyPatch) -> AsyncIterator[httpx.AsyncClient]:
    monkeypatch.setattr(chat_route, "get_llm", lambda *_, meter, **__: chat_model(meter))
    app = create_app()
    app.dependency_overrides[deps.get_embedder] = lambda: EMBEDDER
    app.dependency_overrides[deps.get_chat_gate] = lambda: AdmitAll()
    transport = httpx.ASGITransport(app=app)
    async with httpx.AsyncClient(transport=transport, base_url="http://api", timeout=600) as client:
        yield client


async def publish_discovered(channel: AbstractChannel, posting: RawPosting) -> None:
    exchange = await channel.get_exchange(messaging.EXCHANGE)
    body = gzip.compress(posting.model_dump_json().encode())
    message = aio_pika.Message(body, content_encoding="gzip", headers={"x-attempt": 0})
    await exchange.publish(message, routing_key=messaging.DISCOVERED)


async def wait_for_jobs(sessionmaker: async_sessionmaker[AsyncSession], count: int) -> None:
    for _ in range(200):
        async with sessionmaker() as session:
            indexed = (await session.execute(select(func.count()).select_from(Job))).scalar_one()
        if indexed >= count:
            return
        await asyncio.sleep(0.1)
    raise AssertionError(f"only {indexed} of {count} postings were indexed")


def sse_events(body: str) -> list[tuple[str, dict[str, Any]]]:
    frames = re.findall(r"event: (\w+)\ndata: (.*)\n\n", body)
    return [(name, json.loads(data)) for name, data in frames]


async def test_discovered_posting_is_searchable_and_cited_in_chat(
    sessionmaker: async_sessionmaker[AsyncSession],
    amqp_channel: AbstractChannel,
    api: httpx.AsyncClient,
) -> None:
    indexer = Indexer(sessionmaker, HeuristicExtractor(), EMBEDDER)
    await start_consumers(amqp_channel, indexer, sessionmaker, prefetch=2)
    for posting in POSTINGS:
        await publish_discovered(amqp_channel, posting)
    await wait_for_jobs(sessionmaker, len(POSTINGS))

    search = await api.get("/jobs/search", params={"q": "python backend fastapi"})
    assert search.status_code == 200
    assert search.json()[0]["url"] == POSTINGS[0].url

    driver = await api.get("/jobs/search", params={"q": "řidič sk. C", "city": "Brno"})
    assert driver.json()[0]["url"] == POSTINGS[1].url

    chat = await api.post(
        "/chat", json={"message": "Python backend job in Brno?"}, headers={"X-Request-ID": "e2e"}
    )
    assert chat.status_code == 200
    assert chat.headers["x-request-id"] == "e2e"
    events = sse_events(chat.text)
    names = [name for name, _ in events]
    assert "tool_call" in names and names[-1] == "answer", names
    answer = events[-1][1]
    assert POSTINGS[0].url in answer["text"]
    assert answer["unverified_urls"] == []
