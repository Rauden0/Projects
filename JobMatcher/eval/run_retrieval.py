"""Index the frozen fixtures and report retrieval quality per search mode.

    python eval/run_retrieval.py            # uses the configured embedder (e5 if installed)
    python eval/run_retrieval.py --hashing  # dependency-free baseline embedder

Needs a migrated, *empty-able* database at DATABASE_URL: the job tables are truncated first.
"""

import argparse
import asyncio
from pathlib import Path
from statistics import mean

import yaml
from sqlalchemy import text

from jobmatcher.db.models import EMBEDDING_DIM
from jobmatcher.db.session import get_sessionmaker
from jobmatcher.ingest.extraction import HeuristicExtractor
from jobmatcher.ingest.indexer import Indexer
from jobmatcher.search.embeddings import Embedder, HashingEmbedder, get_embedder
from jobmatcher.search.hybrid import SearchFilters, search_jobs
from jobmatcher.search.metrics import hit_at_k, recall_at_k, reciprocal_rank
from jobmatcher.sources import RawPosting

HERE = Path(__file__).parent


async def load_fixtures(embedder: Embedder) -> None:
    sessionmaker = get_sessionmaker()
    async with sessionmaker() as session, session.begin():
        await session.execute(text("TRUNCATE outbox, job_chunk, job, source RESTART IDENTITY"))

    indexer = Indexer(sessionmaker, HeuristicExtractor(), embedder)
    for p in yaml.safe_load((HERE / "fixtures" / "postings.yaml").read_text(encoding="utf-8")):
        await indexer.index(
            RawPosting(
                kind="fixture",
                company=p["company"],
                external_id=p["id"],
                url=f"fixture://{p['id']}",
                title=p["title"],
                location=p["location"],
                description_html=p["description"].replace("\n", "<br>"),
            )
        )


async def evaluate(embedder: Embedder) -> None:
    golden = yaml.safe_load((HERE / "golden.yaml").read_text(encoding="utf-8"))
    print("| mode | hit@5 | recall@10 | MRR |")
    print("|---|---|---|---|")
    for mode in ("vector", "fulltext", "hybrid"):
        hits, recalls, rrs = [], [], []
        async with get_sessionmaker()() as session:
            for case in golden:
                results = await search_jobs(
                    session,
                    embedder,
                    case["query"],
                    SearchFilters(**case.get("filters", {})),
                    k=10,
                    mode=mode,  # type: ignore[arg-type]
                )
                ranked = [r.url.removeprefix("fixture://") for r in results]
                relevant = set(case["relevant"])
                hits.append(hit_at_k(ranked, relevant, 5))
                recalls.append(recall_at_k(ranked, relevant, 10))
                rrs.append(reciprocal_rank(ranked, relevant))
        print(f"| {mode} | {mean(hits):.2f} | {mean(recalls):.2f} | {mean(rrs):.2f} |")


async def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--hashing", action="store_true", help="use the hashing baseline")
    args = parser.parse_args()
    embedder = HashingEmbedder(EMBEDDING_DIM) if args.hashing else get_embedder()
    await load_fixtures(embedder)
    await evaluate(embedder)


if __name__ == "__main__":
    asyncio.run(main())
