from dataclasses import dataclass

from sqlalchemy.ext.asyncio import async_sessionmaker

from jobmatcher.db.repository import ListingSync
from jobmatcher.ingest.listing_sync import ListingSyncConsumer


async def test_malformed_event_is_dropped() -> None:
    consumer = ListingSyncConsumer(async_sessionmaker())
    assert await consumer.sync(b'{"kind": "greenhouse"}') is None


async def test_unknown_encoding_is_dropped() -> None:
    consumer = ListingSyncConsumer(async_sessionmaker())
    assert await consumer.sync(b"{}", "br") is None


@dataclass
class FakeMessage:
    redelivered: bool
    body: bytes = (
        b'{"kind": "greenhouse", "source": "acme", "external_ids": ["1"], "complete": true}'
    )
    content_encoding: str | None = None
    settled: str = ""

    async def ack(self) -> None:
        self.settled = "ack"

    async def nack(self, requeue: bool = True) -> None:
        self.settled = f"nack requeue={requeue}"

    async def reject(self, requeue: bool = False) -> None:
        self.settled = f"reject requeue={requeue}"


class FailingConsumer(ListingSyncConsumer):
    async def sync(self, body: bytes, encoding: str | None = None) -> ListingSync | None:
        raise RuntimeError("database down")


async def test_failed_sync_is_requeued_once() -> None:
    msg = FakeMessage(redelivered=False)
    await FailingConsumer(async_sessionmaker()).handle(msg)  # type: ignore[arg-type]
    assert msg.settled == "nack requeue=True"


async def test_failed_redelivered_sync_is_dropped() -> None:
    msg = FakeMessage(redelivered=True)
    await FailingConsumer(async_sessionmaker()).handle(msg)  # type: ignore[arg-type]
    assert msg.settled == "reject requeue=False"


async def test_malformed_event_is_acked() -> None:
    msg = FakeMessage(redelivered=False, body=b"{}")
    await ListingSyncConsumer(async_sessionmaker()).handle(msg)  # type: ignore[arg-type]
    assert msg.settled == "ack"
