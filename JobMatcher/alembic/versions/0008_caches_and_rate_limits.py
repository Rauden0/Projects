"""chat answer cache and shared rate limit windows

Revision ID: 0008
Revises: 0007
Create Date: 2026-11-27
"""

from alembic import op

revision = "0008"
down_revision = "0007"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.execute(
        """
        CREATE TABLE chat_answer_cache (
            key         bytea PRIMARY KEY,
            cv_id       uuid REFERENCES cv (id) ON DELETE CASCADE,
            events      jsonb NOT NULL,
            created_at  timestamptz NOT NULL DEFAULT now(),
            expires_at  timestamptz NOT NULL
        )
        """
    )
    op.execute("CREATE INDEX chat_answer_cache_expires ON chat_answer_cache (expires_at)")
    op.execute("CREATE INDEX chat_answer_cache_cv ON chat_answer_cache (cv_id)")
    op.execute(
        """
        CREATE TABLE rate_limit (
            key           bytea NOT NULL,
            window_start  timestamptz NOT NULL,
            hits          integer NOT NULL,
            expires_at    timestamptz NOT NULL,
            PRIMARY KEY (key, window_start)
        )
        """
    )
    op.execute("CREATE INDEX rate_limit_expires ON rate_limit (expires_at)")


def downgrade() -> None:
    op.execute("DROP TABLE rate_limit")
    op.execute("DROP TABLE chat_answer_cache")
