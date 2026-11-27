import logging

from aio_pika.abc import AbstractIncomingMessage
from pydantic import BaseModel, ValidationError
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher import messaging
from jobmatcher.db import repository

log = logging.getLogger(__name__)


class SourceCrawled(BaseModel):
    kind: str
    source: str
    external_ids: list[str]
    complete: bool


class ListingSyncConsumer:
    def __init__(self, sessionmaker: async_sessionmaker[AsyncSession]) -> None:
        self.sessionmaker = sessionmaker

    async def handle(self, msg: AbstractIncomingMessage) -> None:
        async with msg.process(requeue=True):
            await self.sync(msg.body, msg.content_encoding)

    async def sync(self, body: bytes, encoding: str | None = None) -> repository.ListingSync | None:
        try:
            event = SourceCrawled.model_validate_json(messaging.decode_body(body, encoding))
        except (ValidationError, messaging.UndecodableBody):
            log.error("dropping malformed source.crawled event")
            return None
        async with self.sessionmaker() as session, session.begin():
            source_id = await repository.get_or_create_source(session, event.kind, event.source)
            result = await repository.sync_listing(
                session, source_id, set(event.external_ids), event.complete
            )
        log.info("%s/%s: %d reopened, %d closed", event.kind, event.source, *_counts(result))
        return result


def _counts(result: repository.ListingSync) -> tuple[int, int]:
    return result.reopened, result.closed
