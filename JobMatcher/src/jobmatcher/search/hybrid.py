"""Hybrid retrieval: hard filters, then vector + full-text candidates fused with RRF.

Everything runs in one SQL statement so filtering, both rankings and the fusion share a single
snapshot and a single round trip.
"""

import re
from typing import Literal

from pydantic import BaseModel, Field
from sqlalchemy import text
from sqlalchemy.ext.asyncio import AsyncSession

from jobmatcher.ingest.extraction import ContractType, RemotePolicy, Seniority
from jobmatcher.search.embeddings import Embedder

RRF_K = 60
CANDIDATES = 50

Mode = Literal["hybrid", "vector", "fulltext"]


class SearchFilters(BaseModel):
    # Enumerated values end up in the tool's JSON schema, so the model sees exactly what is
    # allowed instead of guessing (small local models otherwise invent values).
    remote_policy: list[RemotePolicy] | None = Field(
        default=None, description="Accept jobs with any of these remote policies"
    )
    city: str | None = Field(default=None, description="Office city, e.g. Brno")
    max_years_required: int | None = Field(default=None, ge=0)
    seniority: list[Seniority] | None = None
    contract_type: list[ContractType] | None = None
    tech: list[str] | None = Field(
        default=None, description="At least one of these technologies (lowercase)"
    )


class SearchHit(BaseModel):
    job_id: int
    title: str
    company: str
    location: str | None
    url: str
    seniority: str | None
    years_required_min: int | None
    remote_policy: str | None
    snippet: str
    score: float


def _filter_sql(filters: SearchFilters, params: dict[str, object]) -> str:
    clauses = ["j.status = 'open'"]
    if filters.remote_policy:
        clauses.append("j.remote_policy = ANY(:remote_policy)")
        params["remote_policy"] = filters.remote_policy
    if filters.city:
        # A remote job is acceptable whatever its office city.
        # Extracted cities vary ("Brno", "Brno, Czech Republic"), so match by substring.
        clauses.append(
            "(j.office_city ILIKE '%' || :city || '%' OR j.remote_policy LIKE 'remote%')"
        )
        params["city"] = filters.city
    if filters.max_years_required is not None:
        clauses.append("(j.years_required_min IS NULL OR j.years_required_min <= :max_years)")
        params["max_years"] = filters.max_years_required
    if filters.seniority:
        clauses.append("(j.seniority IS NULL OR j.seniority = ANY(:seniority))")
        params["seniority"] = filters.seniority
    if filters.contract_type:
        clauses.append("(j.contract_type IS NULL OR j.contract_type = ANY(:contract_type))")
        params["contract_type"] = filters.contract_type
    if filters.tech:
        clauses.append("j.tech_stack && :tech")
        params["tech"] = [t.lower() for t in filters.tech]
    return " AND ".join(clauses)


def _or_tsquery(query: str) -> str:
    """Any-term match: 'python remote backend' -> 'python | remote | backend'."""
    tokens = re.findall(r"\w+", query.lower())
    return " | ".join(dict.fromkeys(tokens)) or "''"


async def search_jobs(
    session: AsyncSession,
    embedder: Embedder,
    query: str,
    filters: SearchFilters | None = None,
    k: int = 10,
    mode: Mode = "hybrid",
) -> list[SearchHit]:
    params: dict[str, object] = {"k": k, "candidates": CANDIDATES, "rrf_k": RRF_K}
    where = _filter_sql(filters or SearchFilters(), params)

    ranked: list[str] = []
    if mode in ("hybrid", "vector"):
        params["qvec"] = str(await embedder.embed_query(query))
        ranked.append(
            f"""
            (SELECT c.id AS chunk_id, c.job_id,
                    row_number() OVER (ORDER BY c.embedding <=> CAST(:qvec AS vector)) AS rank
             FROM job_chunk c JOIN job j ON j.id = c.job_id
             WHERE {where}
             ORDER BY c.embedding <=> CAST(:qvec AS vector)
             LIMIT :candidates)
            """
        )
    if mode in ("hybrid", "fulltext"):
        params["tsq"] = _or_tsquery(query)
        ranked.append(
            f"""
            (SELECT c.id AS chunk_id, c.job_id,
                    row_number() OVER (ORDER BY ts_rank_cd(c.tsv, q) DESC) AS rank
             FROM job_chunk c JOIN job j ON j.id = c.job_id,
                  to_tsquery('simple', :tsq) q
             WHERE {where} AND c.tsv @@ q
             ORDER BY ts_rank_cd(c.tsv, q) DESC
             LIMIT :candidates)
            """
        )

    sql = f"""
        WITH candidates AS ({" UNION ALL ".join(ranked)}),
        fused AS (
            SELECT chunk_id, job_id, sum(1.0 / (:rrf_k + rank)) AS score
            FROM candidates GROUP BY chunk_id, job_id
        ),
        best AS (
            SELECT DISTINCT ON (job_id) job_id, chunk_id, score
            FROM fused ORDER BY job_id, score DESC
        )
        SELECT j.id AS job_id, j.title, j.company, j.location_raw AS location, j.url,
               j.seniority, j.years_required_min, j.remote_policy,
               c.content AS snippet, b.score
        FROM best b
        JOIN job j ON j.id = b.job_id
        JOIN job_chunk c ON c.id = b.chunk_id
        ORDER BY b.score DESC, j.id
        LIMIT :k
    """
    rows = (await session.execute(text(sql), params)).mappings().all()
    return [SearchHit(**{**row, "score": float(row["score"])}) for row in rows]
