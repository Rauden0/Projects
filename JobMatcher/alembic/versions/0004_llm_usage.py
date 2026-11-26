"""llm token usage and cost ledger

Revision ID: 0004
Revises: 0003
Create Date: 2026-11-26
"""
from alembic import op

revision = "0004"
down_revision = "0003"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.execute(
        """
        CREATE TABLE llm_usage (
            id                  bigserial PRIMARY KEY,
            created_at          timestamptz NOT NULL DEFAULT now(),
            purpose             text NOT NULL,
            client_id           text,
            model               text NOT NULL,
            batch               boolean NOT NULL DEFAULT false,
            input_tokens        integer NOT NULL,
            output_tokens       integer NOT NULL,
            cache_read_tokens   integer NOT NULL DEFAULT 0,
            cache_write_tokens  integer NOT NULL DEFAULT 0,
            cost_usd            numeric(12, 6) NOT NULL
        )
        """
    )
    op.execute("CREATE INDEX llm_usage_created ON llm_usage (created_at)")
    op.execute("CREATE INDEX llm_usage_client_created ON llm_usage (client_id, created_at)")


def downgrade() -> None:
    op.execute("DROP TABLE llm_usage")
