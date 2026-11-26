from sqlalchemy.ext.asyncio import async_sessionmaker

from jobmatcher.ingest.listing_sync import ListingSyncConsumer


async def test_malformed_event_is_dropped() -> None:
    consumer = ListingSyncConsumer(async_sessionmaker())
    assert await consumer.sync(b'{"kind": "greenhouse"}') is None
