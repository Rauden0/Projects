from datetime import UTC, datetime

from sqlalchemy import func, select
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher.budget.pricing import TokenUsage, cost_usd
from jobmatcher.db.models import LlmUsage


def month_start(now: datetime) -> datetime:
    return now.replace(day=1, hour=0, minute=0, second=0, microsecond=0)


def day_start(now: datetime) -> datetime:
    return now.replace(hour=0, minute=0, second=0, microsecond=0)


class UsageLedger:
    def __init__(self, sessionmaker: async_sessionmaker[AsyncSession]) -> None:
        self.sessionmaker = sessionmaker

    async def record(
        self,
        *,
        purpose: str,
        model: str,
        usage: TokenUsage,
        client_id: str | None = None,
        batch: bool = False,
    ) -> float:
        cost = cost_usd(model, usage, batch)
        async with self.sessionmaker() as session, session.begin():
            session.add(
                LlmUsage(
                    purpose=purpose,
                    client_id=client_id,
                    model=model,
                    batch=batch,
                    input_tokens=usage.input_tokens,
                    output_tokens=usage.output_tokens,
                    cache_read_tokens=usage.cache_read_tokens,
                    cache_write_tokens=usage.cache_write_tokens,
                    cost_usd=cost,
                )
            )
        return cost

    async def spent_since(self, since: datetime) -> float:
        query = select(func.coalesce(func.sum(LlmUsage.cost_usd), 0)).where(
            LlmUsage.created_at >= since
        )
        async with self.sessionmaker() as session:
            return float((await session.execute(query)).scalar_one())

    async def client_tokens_since(self, client_id: str, since: datetime) -> int:
        tokens = (
            LlmUsage.input_tokens
            + LlmUsage.output_tokens
            + LlmUsage.cache_read_tokens
            + LlmUsage.cache_write_tokens
        )
        query = select(func.coalesce(func.sum(tokens), 0)).where(
            LlmUsage.client_id == client_id, LlmUsage.created_at >= since
        )
        async with self.sessionmaker() as session:
            return int((await session.execute(query)).scalar_one())


def utc_now() -> datetime:
    return datetime.now(UTC)

