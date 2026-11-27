"""Index the frozen fixtures and report retrieval quality per search mode.

    python eval/run_retrieval.py            # uses the configured embedder (e5 if installed)
    python eval/run_retrieval.py --hashing  # dependency-free baseline embedder
    python eval/run_retrieval.py --min-hit5 0.80 --min-recall10 0.85  # CI gate on hybrid, all

The job tables are truncated first, so it runs against EVAL_DATABASE_URL (default:
jobmatcher_eval on localhost, migrated with alembic) and refuses databases not named *_eval or
*_test.
"""

import argparse
import asyncio
import os
from collections.abc import Sequence
from dataclasses import dataclass
from pathlib import Path
from statistics import mean
from typing import Any

import yaml
from sqlalchemy import text
from sqlalchemy.engine import make_url

from jobmatcher.db.models import EMBEDDING_DIM
from jobmatcher.db.session import get_sessionmaker
from jobmatcher.domain import JobFacts, RawPosting
from jobmatcher.ingest.extraction import HeuristicExtractor
from jobmatcher.ingest.indexer import Indexer
from jobmatcher.search.embeddings import Embedder, HashingEmbedder, get_embedder
from jobmatcher.search.hybrid import SearchFilters, search_jobs
from jobmatcher.search.metrics import hit_at_k, recall_at_k, reciprocal_rank

HERE = Path(__file__).parent
EVAL_DATABASE_URL = os.environ.get(
    "EVAL_DATABASE_URL", "postgresql+asyncpg://jobmatcher:jobmatcher@localhost:5432/jobmatcher_eval"
)


def use_eval_database() -> None:
    if not (make_url(EVAL_DATABASE_URL).database or "").endswith(("_eval", "_test")):
        raise SystemExit(
            f"EVAL_DATABASE_URL must name a *_eval or *_test database: {EVAL_DATABASE_URL}"
        )
    os.environ["DATABASE_URL"] = EVAL_DATABASE_URL


@dataclass(frozen=True)
class FixtureExtractor:
    """Heuristic facts plus the required skills a fixture states, so a test can plant text
    in the skills field."""

    skills: tuple[str, ...]

    async def extract(self, title: str, location: str | None, text: str) -> JobFacts:
        facts = await HeuristicExtractor().extract(title, location, text)
        return facts.model_copy(update={"required_skills": list(self.skills)})


async def load_fixtures(
    embedder: Embedder, url_prefix: str = "fixture://", extra: Sequence[str] = ()
) -> None:
    """Seeds fixtures/postings.yaml, then the `extra` files from fixtures/ (job ids follow)."""
    sessionmaker = get_sessionmaker()
    async with sessionmaker() as session, session.begin():
        await session.execute(
            text("TRUNCATE outbox, cv, job_chunk, job, source RESTART IDENTITY CASCADE")
        )

    postings = []
    for name in ("postings.yaml", *extra):
        postings += yaml.safe_load((HERE / "fixtures" / name).read_text(encoding="utf-8"))
    for p in postings:
        indexer = Indexer(sessionmaker, FixtureExtractor(tuple(p.get("skills", []))), embedder)
        await indexer.index(
            RawPosting(
                kind="fixture",
                company=p["company"],
                external_id=p["id"],
                url=f"{url_prefix}{p['id']}",
                title=p["title"],
                location=p["location"],
                description_html=p["description"].replace("\n", "<br>"),
            )
        )


@dataclass(frozen=True, slots=True)
class Scores:
    hit5: float
    recall10: float
    mrr: float


async def evaluate(embedder: Embedder) -> dict[tuple[str, str], Scores]:
    golden = yaml.safe_load((HERE / "golden.yaml").read_text(encoding="utf-8"))
    subsets = {
        "all": golden,
        "original": [c for c in golden if "tag" not in c],
        "added": [c for c in golden if c.get("tag") == "added"],
    }
    print("| questions | mode | hit@5 | recall@10 | MRR |")
    print("|---|---|---|---|---|")
    scores = {}
    for name, cases in subsets.items():
        for mode in ("vector", "fulltext", "hybrid"):
            s = scores[name, mode] = await _evaluate_mode(embedder, mode, cases)
            label = f"{name} ({len(cases)})"
            print(f"| {label} | {mode} | {s.hit5:.2f} | {s.recall10:.2f} | {s.mrr:.2f} |")
    return scores


async def _evaluate_mode(embedder: Embedder, mode: str, golden: list[dict[str, Any]]) -> Scores:
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
    return Scores(mean(hits), mean(recalls), mean(rrs))


def gate_failures(scores: Scores, min_hit5: float, min_recall10: float) -> list[str]:
    failures = []
    if scores.hit5 < min_hit5:
        failures.append(f"hybrid hit@5 {scores.hit5:.3f} < {min_hit5}")
    if scores.recall10 < min_recall10:
        failures.append(f"hybrid recall@10 {scores.recall10:.3f} < {min_recall10}")
    return failures


async def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--hashing", action="store_true", help="use the hashing baseline")
    parser.add_argument("--min-hit5", type=float, default=0.0, help="fail below (hybrid, all)")
    parser.add_argument("--min-recall10", type=float, default=0.0, help="fail below (hybrid, all)")
    args = parser.parse_args()
    use_eval_database()
    embedder = HashingEmbedder(EMBEDDING_DIM) if args.hashing else get_embedder()
    if (
        not args.hashing
        and isinstance(embedder, HashingEmbedder)
        and (args.min_hit5 or args.min_recall10)
    ):
        raise SystemExit("sentence-transformers is not installed; the gate needs the e5 model")
    await load_fixtures(embedder)
    scores = await evaluate(embedder)
    failures = gate_failures(scores["all", "hybrid"], args.min_hit5, args.min_recall10)
    if failures:
        raise SystemExit("retrieval below threshold: " + "; ".join(failures))


if __name__ == "__main__":
    asyncio.run(main())
