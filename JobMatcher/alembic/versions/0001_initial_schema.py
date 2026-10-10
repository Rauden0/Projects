"""initial schema

Revision ID: 0001
Revises:
Create Date: 2026-10-10
"""
from alembic import op

revision = "0001"
down_revision = None
branch_labels = None
depends_on = None


_SCHEMA = [
    """
    CREATE TABLE source (
        id       smallserial PRIMARY KEY,
        kind     text NOT NULL,
        company  text NOT NULL,
        config   jsonb NOT NULL DEFAULT '{}',
        UNIQUE (kind, company)
    )
    """,
    """
    CREATE TABLE job (
        id                  bigserial PRIMARY KEY,
        source_id           smallint NOT NULL REFERENCES source(id),
        external_id         text NOT NULL,
        url                 text NOT NULL,
        title               text NOT NULL,
        company             text NOT NULL,
        location_raw        text,
        description         text NOT NULL,
        content_hash        bytea NOT NULL,
        seniority           text,
        years_required_min  smallint,
        remote_policy       text,
        office_city         text,
        salary_min          integer,
        salary_max          integer,
        currency            char(3),
        contract_type       text,
        tech_stack          text[] NOT NULL DEFAULT '{}',
        extraction_status   text NOT NULL DEFAULT 'pending',
        status              text NOT NULL DEFAULT 'open',
        first_seen_at       timestamptz NOT NULL DEFAULT now(),
        last_seen_at        timestamptz NOT NULL DEFAULT now(),
        UNIQUE (source_id, external_id)
    )
    """,
    """
    CREATE INDEX job_open_filters ON job (remote_policy, seniority, years_required_min)
        WHERE status = 'open'
    """,
    """
    CREATE INDEX job_tech_stack ON job USING gin (tech_stack)
    """,
    """
    CREATE TABLE job_chunk (
        id         bigserial PRIMARY KEY,
        job_id     bigint NOT NULL REFERENCES job(id) ON DELETE CASCADE,
        ord        smallint NOT NULL,
        section    text,
        content    text NOT NULL,
        embedding  vector(1024) NOT NULL,
        tsv        tsvector GENERATED ALWAYS AS (to_tsvector('simple', content)) STORED
    )
    """,
    """
    CREATE INDEX job_chunk_job ON job_chunk (job_id)
    """,
    """
    CREATE INDEX job_chunk_embedding ON job_chunk USING hnsw (embedding vector_cosine_ops)
    """,
    """
    CREATE INDEX job_chunk_tsv ON job_chunk USING gin (tsv)
    """,
    """
    CREATE TABLE cv (
        id          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
        raw_text    text NOT NULL,
        profile     jsonb NOT NULL,
        embedding   vector(1024) NOT NULL,
        created_at  timestamptz NOT NULL DEFAULT now()
    )
    """,
    """
    CREATE TABLE outbox (
        id            bigserial PRIMARY KEY,
        topic         text NOT NULL,
        payload       jsonb NOT NULL,
        created_at    timestamptz NOT NULL DEFAULT now(),
        published_at  timestamptz
    )
    """,
    """
    CREATE INDEX outbox_unpublished ON outbox (id) WHERE published_at IS NULL
    """,
]


def upgrade() -> None:
    op.execute("CREATE EXTENSION IF NOT EXISTS vector")
    for statement in _SCHEMA:
        op.execute(statement)


def downgrade() -> None:
    op.execute("DROP TABLE outbox, cv, job_chunk, job, source")
