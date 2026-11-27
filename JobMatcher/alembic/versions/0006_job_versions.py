"""crawl time and derived-data versions per job

Revision ID: 0006
Revises: 0005
Create Date: 2026-11-27
"""

from alembic import op

revision = "0006"
down_revision = "0005"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.execute("ALTER TABLE job ADD COLUMN crawled_at timestamptz")
    op.execute("ALTER TABLE job ADD COLUMN extraction_version smallint")
    op.execute("ALTER TABLE job_chunk ADD COLUMN embedding_model text")


def downgrade() -> None:
    op.execute("ALTER TABLE job_chunk DROP COLUMN embedding_model")
    op.execute("ALTER TABLE job DROP COLUMN crawled_at, DROP COLUMN extraction_version")
