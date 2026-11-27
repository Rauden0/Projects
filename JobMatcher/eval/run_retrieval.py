"""Index the frozen fixtures and report retrieval quality per search mode.

    python eval/run_retrieval.py            # uses the configured embedder (e5 if installed)
    python eval/run_retrieval.py --hashing  # dependency-free baseline embedder

The job tables are truncated first, so it runs against EVAL_DATABASE_URL (default:
jobmatcher_eval on localhost, migrated with alembic) and refuses databases not named *_eval or
*_test.
"""

import argparse
import asyncio
import os
from pathlib import Path
from statistics import mean
from typing import Any

import yaml
from sqlalchemy import text
from sqlalchemy.engine import make_url

from jobmatcher.db.models import EMBEDDING_DIM
from jobmatcher.db.session import get_sessionmaker
from jobmatcher.domain import RawPosting
from jobmatcher.ingest.extraction import HeuristicExtractor
from jobmatcher.ingest.indexer import Indexer
from jobmatcher.search.embeddings import Embedder, HashingEmbedder, get_embedder
from jobmatcher.search.hybrid import SearchFilters, search_jobs
from jobmatcher.search.metrics import hit_at_k, recall_at_k, reciprocal_rank

HERE = Path(__file__).parent
EVAL_DATABASE_URL = os.environ.get(
    "EVAL_DATABASE_URL", "postgresql+asyncpg://jobmatcher:jobmatcher@localhost:5432/jobmatcher_eval"
)


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
    subsets = {
        "all": golden,
        "original": [c for c in golden if "tag" not in c],
        "added": [c for c in golden if c.get("tag") == "added"],
    }
    print("| questions | mode | hit@5 | recall@10 | MRR |")
    print("|---|---|---|---|---|")
    for name, cases in subsets.items():
        for mode in ("vector", "fulltext", "hybrid"):
            await _evaluate_mode(embedder, f"{name} ({len(cases)})", mode, cases)


async def _evaluate_mode(
    embedder: Embedder, name: str, mode: str, golden: list[dict[str, Any]]
) -> None:
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
    print(f"| {name} | {mode} | {mean(hits):.2f} | {mean(recalls):.2f} | {mean(rrs):.2f} |")


async def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--hashing", action="store_true", help="use the hashing baseline")
    args = parser.parse_args()
    if not (make_url(EVAL_DATABASE_URL).database or "").endswith(("_eval", "_test")):
        raise SystemExit(
            f"EVAL_DATABASE_URL must name a *_eval or *_test database: {EVAL_DATABASE_URL}"
        )
    os.environ["DATABASE_URL"] = EVAL_DATABASE_URL
    embedder = HashingEmbedder(EMBEDDING_DIM) if args.hashing else get_embedder()
    await load_fixtures(embedder)
    await evaluate(embedder)


if __name__ == "__main__":
    asyncio.run(main())
