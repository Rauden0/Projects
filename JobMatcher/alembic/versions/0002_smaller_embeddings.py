"""384-dim embeddings (multilingual-e5-small)

e5-large needed ~2.5 GB RAM per process (api + worker), which together with a local LLM did
not fit a 16 GB machine. Stored vectors are incompatible, so chunks are dropped and re-created
by the next crawl; CVs must be uploaded again.

Revision ID: 0002
Revises: 0001
Create Date: 2026-10-10
"""
from alembic import op

revision = "0002"
down_revision = "0001"
branch_labels = None
depends_on = None


def _resize(dim: int) -> None:
    op.execute("DROP INDEX job_chunk_embedding")
    # Without chunks a job would never be re-embedded (its content hash is unchanged).
    op.execute("TRUNCATE outbox, job_chunk, job, cv")
    op.execute(f"ALTER TABLE job_chunk ALTER COLUMN embedding TYPE vector({dim})")
    op.execute(f"ALTER TABLE cv ALTER COLUMN embedding TYPE vector({dim})")
    op.execute(
        "CREATE INDEX job_chunk_embedding ON job_chunk USING hnsw (embedding vector_cosine_ops)"
    )


def upgrade() -> None:
    _resize(384)


def downgrade() -> None:
    _resize(1024)
