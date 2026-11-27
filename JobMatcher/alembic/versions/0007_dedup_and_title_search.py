"""cross-source duplicates resolved at ingest, accent-insensitive title search

Revision ID: 0007
Revises: 0006
Create Date: 2026-11-27
"""

from alembic import op

revision = "0007"
down_revision = "0006"
branch_labels = None
depends_on = None

DEDUP_KEY = (
    "left(regexp_replace(lower(company), '[^a-z0-9]', '', 'g'), 6) || '|' || lower(title)"
    " || '|' || left(lower(coalesce(location_raw, '')), 4)"
)

# Same rule as repository.assign_canonical, over every open group.
BACKFILL_CANONICAL = """
    WITH grp AS (
        SELECT j.id, j.source_id, length(j.description) AS len,
               first_value(j.id) OVER (
                   PARTITION BY j.dedup_key ORDER BY length(j.description) DESC, j.id
               ) AS canon
        FROM job j
        WHERE j.status = 'open'
    ),
    decided AS (
        SELECT g.id,
               CASE WHEN g.id = g.canon THEN NULL
                    WHEN g.len < 300 AND c.len >= 300 THEN g.canon
                    WHEN g.len >= 300 AND g.source_id <> c.source_id AND (
                        SELECT 1 - (a.embedding <=> b.embedding)
                        FROM job_chunk a, job_chunk b
                        WHERE a.job_id = g.id AND a.ord = 0 AND b.job_id = g.canon AND b.ord = 0
                    ) >= 0.9 THEN g.canon
               END AS canonical
        FROM grp g JOIN grp c ON c.id = g.canon
    )
    UPDATE job SET canonical_job_id = d.canonical
    FROM decided d
    WHERE job.id = d.id AND d.canonical IS NOT NULL
"""


def upgrade() -> None:
    op.execute(
        f"ALTER TABLE job ADD COLUMN dedup_key text GENERATED ALWAYS AS ({DEDUP_KEY}) STORED"
    )
    op.execute(
        "ALTER TABLE job ADD COLUMN canonical_job_id bigint REFERENCES job(id) ON DELETE SET NULL"
    )
    op.execute("CREATE INDEX job_dedup_key ON job (dedup_key) WHERE status = 'open'")
    op.execute("CREATE INDEX job_canonical ON job (canonical_job_id)")
    op.execute(BACKFILL_CANONICAL)

    op.execute("CREATE EXTENSION IF NOT EXISTS unaccent WITH SCHEMA public")
    # unaccent() is only STABLE (its dictionary could change), which generated columns and
    # index expressions reject; the wrapper pins the dictionary and declares it immutable.
    op.execute(
        """
        CREATE FUNCTION immutable_unaccent(text) RETURNS text
        LANGUAGE sql IMMUTABLE PARALLEL SAFE STRICT
        AS $$ SELECT public.unaccent('public.unaccent'::regdictionary, $1) $$
        """
    )
    op.execute(
        "ALTER TABLE job ADD COLUMN title_tsv tsvector GENERATED ALWAYS AS "
        "(to_tsvector('simple'::regconfig, public.immutable_unaccent(title))) STORED"
    )
    op.execute("CREATE INDEX job_title_tsv ON job USING gin (title_tsv)")


def downgrade() -> None:
    op.execute("ALTER TABLE job DROP COLUMN title_tsv")
    op.execute("DROP FUNCTION immutable_unaccent(text)")
    op.execute("DROP EXTENSION IF EXISTS unaccent")
    op.execute("ALTER TABLE job DROP COLUMN canonical_job_id, DROP COLUMN dedup_key")
