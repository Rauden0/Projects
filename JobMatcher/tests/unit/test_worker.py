from dataclasses import dataclass, field
from typing import Any

import pytest

from jobmatcher import messaging
from jobmatcher.ingest.indexer import IndexResult
from jobmatcher.ingest.worker import Worker
from jobmatcher.sources import RawPosting

BODY = RawPosting(
    kind="greenhouse",
    company="acme",
    external_id="1",
    url="u",
    title="t",
    location=None,
    description_html="",
).model_dump_json().encode()


@dataclass
class FakeExchange:
    published: list[tuple[str, int]] = field(default_factory=list)

    async def publish(self, message: Any, routing_key: str) -> None:
        self.published.append((routing_key, message.headers["x-attempt"]))


@dataclass
class FakeChannel:
    retry: FakeExchange = field(default_factory=FakeExchange)
    default_exchange: FakeExchange = field(default_factory=FakeExchange)

    async def get_exchange(self, name: str) -> FakeExchange:
        assert name == messaging.RETRY_EXCHANGE
        return self.retry


@dataclass
class FakeMessage:
    body: bytes
    headers: dict[str, Any]
    acked: bool = False

    async def ack(self) -> None:
        self.acked = True


class FailingIndexer:
    async def index(self, posting: RawPosting) -> IndexResult:
        raise RuntimeError("boom")


class OkIndexer:
    async def index(self, posting: RawPosting) -> IndexResult:
        return IndexResult(job_id=1, changed=True)


@pytest.mark.asyncio
async def test_success_acks_without_republish() -> None:
    channel = FakeChannel()
    msg = FakeMessage(BODY, {})
    await Worker(OkIndexer(), channel).handle(msg)  # type: ignore[arg-type]
    assert msg.acked and not channel.retry.published


@pytest.mark.asyncio
@pytest.mark.parametrize("attempt", [0, 1, 2])
async def test_failure_schedules_next_retry(attempt: int) -> None:
    channel = FakeChannel()
    msg = FakeMessage(BODY, {"x-attempt": attempt})
    await Worker(FailingIndexer(), channel).handle(msg)  # type: ignore[arg-type]
    assert channel.retry.published == [(str(attempt + 1), attempt + 1)]
    assert msg.acked


@pytest.mark.asyncio
async def test_last_failure_goes_to_dlq() -> None:
    channel = FakeChannel()
    msg = FakeMessage(BODY, {"x-attempt": len(messaging.RETRY_DELAYS_S)})
    await Worker(FailingIndexer(), channel).handle(msg)  # type: ignore[arg-type]
    assert channel.default_exchange.published == [(messaging.DLQ, 3)]
    assert not channel.retry.published


@pytest.mark.asyncio
async def test_malformed_message_goes_to_dlq() -> None:
    channel = FakeChannel()
    msg = FakeMessage(b"{not json", {})
    await Worker(OkIndexer(), channel).handle(msg)  # type: ignore[arg-type]
    assert channel.default_exchange.published == [(messaging.DLQ, 0)]
