import logging
from dataclasses import dataclass

from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher.db import repository
from jobmatcher.domain import RawPosting
from jobmatcher.errors import BudgetExceeded, ExtractionError, LLMError
from jobmatcher.ingest.chunking import chunk_posting
from jobmatcher.ingest.extraction import EXTRACTION_VERSION, Extractor, HeuristicExtractor
from jobmatcher.ingest.normalize import content_hash, html_to_text
from jobmatcher.search.embeddings import Embedder

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
        defer_extraction: bool = False,
    ) -> None:
        self.sessionmaker = sessionmaker
        self.extractor = extractor
        self.embedder = embedder
        self.defer_extraction = defer_extraction

    async def index(self, posting: RawPosting) -> IndexResult:
        text = html_to_text(posting.description_html)
        digest = content_hash(posting.title, posting.location, text)

        async with self.sessionmaker() as session, session.begin():
            source_id = await repository.get_or_create_source(
                session, posting.kind, posting.source or posting.company
            )
            existing = await repository.find_job(session, source_id, posting.external_id)
            if existing is not None and repository.is_stale(existing, posting.crawled_at):
                log.info("ignoring stale %s/%s", posting.company, posting.external_id)
                return IndexResult(existing.id, changed=False)
            if existing is not None and existing.content_hash == digest:
                await repository.touch_job(session, existing.id, posting.crawled_at)
                return IndexResult(existing.id, changed=False)

        # Slow work (LLM, embeddings) happens outside of any open transaction.
        status = "ok"
        try:
            if self.defer_extraction:
                status = "pending"
                facts = await HeuristicExtractor().extract(posting.title, posting.location, text)
            else:
                facts = await self.extractor.extract(posting.title, posting.location, text)
        except (LLMError, ExtractionError, BudgetExceeded) as exc:
            log.warning(
                "extraction failed for %s/%s, using heuristics: %s",
                posting.company,
                posting.external_id,
                exc,
            )
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
            crawled_at=posting.crawled_at,
        )
        derivation = repository.Derivation(
            EXTRACTION_VERSION if status == "ok" else None, self.embedder.model_name
        )
        async with self.sessionmaker() as session, session.begin():
            job_id = await repository.upsert_job(
                session, source_id, record, facts, status, chunks, embeddings, derivation
            )
            if job_id is None:
                log.info("newer %s/%s indexed meanwhile", posting.company, posting.external_id)
                newer = await repository.find_job(session, source_id, posting.external_id)
                return IndexResult(newer.id if newer else 0, changed=False)
        return IndexResult(job_id, changed=True)
