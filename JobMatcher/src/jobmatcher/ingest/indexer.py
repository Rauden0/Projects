import logging
from dataclasses import dataclass

from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher.db import repository
from jobmatcher.ingest.chunking import chunk_posting
from jobmatcher.ingest.extraction import Extractor, HeuristicExtractor
from jobmatcher.ingest.normalize import content_hash, html_to_text
from jobmatcher.search.embeddings import Embedder
from jobmatcher.sources import RawPosting

log = logging.getLogger(__name__)


@dataclass(frozen=True)
class IndexResult:
    job_id: int
    changed: bool


class Indexer:
    """Turns a raw posting into a stored, searchable job. Idempotent per (source, external_id)."""

    def __init__(
        self,
        sessionmaker: async_sessionmaker[AsyncSession],
        extractor: Extractor,
        embedder: Embedder,
    ) -> None:
        self.sessionmaker = sessionmaker
        self.extractor = extractor
        self.embedder = embedder

    async def index(self, posting: RawPosting) -> IndexResult:
        text = html_to_text(posting.description_html)
        digest = content_hash(posting.title, posting.location, text)

        async with self.sessionmaker() as session, session.begin():
            source_id = await repository.get_or_create_source(
                session, posting.kind, posting.source or posting.company
            )
            existing = await repository.find_job(session, source_id, posting.external_id)
            if existing is not None and existing.content_hash == digest:
                await repository.touch_job(session, existing.id)
                return IndexResult(existing.id, changed=False)

        # Slow work (LLM, embeddings) happens outside of any open transaction.
        status = "ok"
        try:
            facts = await self.extractor.extract(posting.title, posting.location, text)
        except Exception:
            log.exception("extraction failed for %s/%s", posting.company, posting.external_id)
            facts = await HeuristicExtractor().extract(posting.title, posting.location, text)
            status = "fallback"

        chunks = chunk_posting(posting.title, posting.company, text)
        embeddings = await self.embedder.embed_documents([c.content for c in chunks])

        record = repository.PostingRecord(
            kind=posting.kind,
            company=posting.company,
            external_id=posting.external_id,
            url=posting.url,
            title=posting.title,
            location=posting.location,
            description=text,
            content_hash=digest,
        )
        async with self.sessionmaker() as session, session.begin():
            job_id = await repository.upsert_job(
                session, source_id, record, facts, status, chunks, embeddings
            )
        return IndexResult(job_id, changed=True)
