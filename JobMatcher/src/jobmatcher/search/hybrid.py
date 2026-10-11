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
# Keyword lists only contribute their strongest matches: their long tails (any posting that
# shares one common word) mostly add noise to the fusion.
KEYWORD_CANDIDATES = 10
LOCAL_BOOST = 1.0 / (RRF_K + 1)
# Per-list RRF weights, kept as a tuning knob for eval/run_retrieval.py. On the current golden
# set equal weights score best (see README), so they are all 1.
WEIGHTS = {"vector": 1.0, "fulltext": 1.0, "title": 1.0}

Mode = Literal["hybrid", "vector", "fulltext"]


class SearchFilters(BaseModel):
    # Enumerated values end up in the tool's JSON schema, so the model sees exactly what is
    # allowed instead of guessing (small local models otherwise invent values).
    remote_policy: list[RemotePolicy] | None = Field(
        default=None, description="Accept jobs with any of these remote policies"
    )
    city: str | None = Field(
        default=None,
        description="Office city, e.g. Brno. Remote jobs always pass this filter, so for "
        "'in Brno or remote' set only city and leave remote_policy empty.",
    )
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


# Job boards often carry only a stub of an ad that links to the employer's own career site
# (jobs.cz -> a JavaScript page). When the same job was also crawled from the employer's ATS
# with its full text, show that copy and hide the stub. Same job = same title, and company
# names agreeing on their first letters ("Thermo Fisher Scientific" vs. "thermofisher"),
# same location prefix ("Brno – Černovice" vs. "Brno, Czechia").
THIN_DESCRIPTION_CHARS = 300
_NOT_A_THIN_DUPLICATE = f"""NOT (
    length(j.description) < {THIN_DESCRIPTION_CHARS}
    AND EXISTS (
        SELECT 1 FROM job d
        WHERE d.id <> j.id AND d.status = 'open'
          AND lower(d.title) = lower(j.title)
          AND length(d.description) >= {THIN_DESCRIPTION_CHARS}
          AND left(regexp_replace(lower(d.company), '[^a-z0-9]', '', 'g'), 6)
            = left(regexp_replace(lower(j.company), '[^a-z0-9]', '', 'g'), 6)
          -- and the same place: generic titles repeat across a company's offices
          AND left(lower(coalesce(d.location_raw, '')), 4)
            = left(lower(coalesce(j.location_raw, '')), 4)
    )
)"""


def _filter_sql(
    filters: SearchFilters, params: dict[str, object], strict_city: bool = False
) -> str:
    clauses = ["j.status = 'open'", _NOT_A_THIN_DUPLICATE]
    if filters.remote_policy:
        # Unknown policy is kept, as for the other filters: many postings never state it.
        clauses.append("(j.remote_policy IS NULL OR j.remote_policy = ANY(:remote_policy))")
        params["remote_policy"] = filters.remote_policy
    if filters.city:
        # A remote job is acceptable whatever its office city.
        # Extracted cities vary ("Brno", "Brno, Czech Republic"), so match by substring.
        remote_ok = "" if strict_city else " OR j.remote_policy LIKE 'remote%'"
        clauses.append(
            "(j.office_city ILIKE '%' || :city || '%' OR j.location_raw ILIKE '%' || :city || '%'"
            f"{remote_ok})"
        )
        params["city"] = filters.city
    if filters.max_years_required is not None:
        clauses.append("(j.years_required_min IS NULL OR j.years_required_min <= :max_years)")
        params["max_years"] = filters.max_years_required
    if filters.seniority:
        clauses.append("(j.seniority IS NULL OR j.seniority = ANY(:seniority))")
        # Internships rarely state a seniority, so they would slip through the NULL case.
        clauses.append(r"j.title !~* '\m(intern|internship|trainee|stáž|stážista|graduate)\M'")
        params["seniority"] = filters.seniority
    if filters.contract_type:
        clauses.append("(j.contract_type IS NULL OR j.contract_type = ANY(:contract_type))")
        params["contract_type"] = filters.contract_type
    if filters.tech:
        clauses.append("j.tech_stack && :tech")
        params["tech"] = [t.lower() for t in filters.tech]
    return " AND ".join(clauses)


# The 'simple' text-search config keeps every word, so stop words would match almost every
# posting and turn the keyword lists into noise (Czech "a", "z", "na", English "and", "with").
_QUERY_STOPWORDS = {
    "and", "the", "with", "for", "from", "that", "this", "jobs", "job", "work", "role",
    "pro", "nebo", "jako", "které", "který", "která", "práce", "práci", "pozice", "chci",
    "hledám", "najdi", "mám", "jsem", "být", "jen", "jak", "aby", "při", "přes", "pod",
}


def _or_tsquery(query: str) -> str:
    """Any-term match: 'python remote backend' -> 'python | remote | backend'."""
    tokens = [
        t for t in re.findall(r"\w+", query.lower()) if len(t) >= 3 and t not in _QUERY_STOPWORDS
    ]
    return " | ".join(dict.fromkeys(tokens)) or "''"


async def search_jobs(
    session: AsyncSession,
    embedder: Embedder,
    query: str,
    filters: SearchFilters | None = None,
    k: int = 10,
    mode: Mode = "hybrid",
    strict_city: bool = False,
) -> list[SearchHit]:
    """``strict_city`` drops the "remote jobs pass any city" rule, for local-only listings."""
    params: dict[str, object] = {
        "k": k,
        "candidates": CANDIDATES,
        "keyword_candidates": KEYWORD_CANDIDATES,
        "rrf_k": RRF_K,
    }
    params.update({f"w_{name}": weight for name, weight in WEIGHTS.items()})
    where = _filter_sql(filters or SearchFilters(), params, strict_city)

    ranked: list[str] = []
    if mode in ("hybrid", "vector"):
        params["qvec"] = str(await embedder.embed_query(query))
        ranked.append(
            f"""
            (SELECT 'vector' AS list, c.id AS chunk_id, c.job_id,
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
            (SELECT 'fulltext' AS list, c.id AS chunk_id, c.job_id,
                    row_number() OVER (ORDER BY ts_rank_cd(c.tsv, q) DESC) AS rank
             FROM job_chunk c JOIN job j ON j.id = c.job_id,
                  to_tsquery('simple', :tsq) q
             WHERE {where} AND c.tsv @@ q
             ORDER BY ts_rank_cd(c.tsv, q) DESC
             LIMIT :keyword_candidates)
            """
        )

    # With a city filter, remote jobs from anywhere also pass; postings actually in that city
    # get a boost worth about one first-place rank so they are not buried by remote ones.
    local_boost = "0"
    if filters and filters.city:
        local_boost = (
            "CASE WHEN j.office_city ILIKE '%' || :city || '%'"
            " OR j.location_raw ILIKE '%' || :city || '%' THEN :local_boost ELSE 0 END"
        )
        params["local_boost"] = LOCAL_BOOST

    if mode == "hybrid":
        # Titles are the most precise signal ("Python Developer" vs. a TypeScript job that
        # mentions Python once), so title matches form a third ranked list in the fusion.
        ranked.append(
            f"""
            (SELECT 'title' AS list, c.id AS chunk_id, j.id AS job_id,
                    row_number() OVER (
                        ORDER BY ts_rank_cd(to_tsvector('simple', j.title), q) DESC
                    ) AS rank
             FROM job j JOIN job_chunk c ON c.job_id = j.id AND c.ord = 0,
                  to_tsquery('simple', :tsq) q
             WHERE {where} AND to_tsvector('simple', j.title) @@ q
             ORDER BY ts_rank_cd(to_tsvector('simple', j.title), q) DESC
             LIMIT :keyword_candidates)
            """
        )

    sql = f"""
        WITH candidates AS ({" UNION ALL ".join(ranked)}),
        -- RRF per job: each list contributes its best rank for that job.
        per_list AS (
            SELECT list, job_id, min(rank) AS rank FROM candidates GROUP BY list, job_id
        ),
        fused AS (
            SELECT job_id,
                   sum(CASE list WHEN 'vector' THEN CAST(:w_vector AS float8)
                                 WHEN 'fulltext' THEN CAST(:w_fulltext AS float8)
                                 ELSE CAST(:w_title AS float8) END / (:rrf_k + rank)) AS score
            FROM per_list GROUP BY job_id
        ),
        -- The snippet is the job's best-ranked chunk.
        snippet AS (
            SELECT DISTINCT ON (job_id) job_id, chunk_id
            FROM candidates ORDER BY job_id, rank, list
        ),
        best AS (
            SELECT f.job_id, s.chunk_id, f.score
            FROM fused f JOIN snippet s USING (job_id)
        )
        SELECT j.id AS job_id, j.title, j.company, j.location_raw AS location, j.url,
               j.seniority, j.years_required_min, j.remote_policy,
               c.content AS snippet, b.score + {local_boost} AS score
        FROM best b
        JOIN job j ON j.id = b.job_id
        JOIN job_chunk c ON c.id = b.chunk_id
        ORDER BY score DESC, j.id
        LIMIT :k
    """
    rows = (await session.execute(text(sql), params)).mappings().all()
    return [SearchHit(**{**row, "score": float(row["score"])}) for row in rows]
