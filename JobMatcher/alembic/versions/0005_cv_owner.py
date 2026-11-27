"""owner of each cv

Revision ID: 0005
Revises: 0004
Create Date: 2026-11-27
"""

from alembic import op

revision = "0005"
down_revision = "0004"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.execute("ALTER TABLE cv ADD COLUMN owner_id text")
    op.execute("CREATE INDEX cv_owner ON cv (owner_id)")


def downgrade() -> None:
    op.execute("DROP INDEX cv_owner")
    op.execute("ALTER TABLE cv DROP COLUMN owner_id")
