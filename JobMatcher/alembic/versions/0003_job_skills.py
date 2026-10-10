"""required and nice-to-have skills per job

Revision ID: 0003
Revises: 0002
Create Date: 2026-10-10
"""
from alembic import op

revision = "0003"
down_revision = "0002"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.execute("ALTER TABLE job ADD COLUMN required_skills text[] NOT NULL DEFAULT '{}'")
    op.execute("ALTER TABLE job ADD COLUMN nice_to_have_skills text[] NOT NULL DEFAULT '{}'")
    # Force re-extraction on the next crawl: the indexer skips jobs whose hash is unchanged.
    op.execute("UPDATE job SET content_hash = ''::bytea")


def downgrade() -> None:
    op.execute("ALTER TABLE job DROP COLUMN required_skills, DROP COLUMN nice_to_have_skills")
