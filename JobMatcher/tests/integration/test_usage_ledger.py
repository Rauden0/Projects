from datetime import UTC, datetime, timedelta

from sqlalchemy import text
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from jobmatcher.budget.ledger import UsageLedger
from jobmatcher.budget.pricing import TokenUsage


async def test_ledger_sums_cost_and_client_tokens(
    sessionmaker: async_sessionmaker[AsyncSession],
) -> None:
    async with sessionmaker() as session, session.begin():
        await session.execute(text("TRUNCATE llm_usage"))
    ledger = UsageLedger(sessionmaker)
    usage = TokenUsage(10_000, 1_000)
    await ledger.record(purpose="chat", model="claude-haiku-5-5", usage=usage, client_id="a")
    await ledger.record(purpose="extraction", model="claude-haiku-5-5", usage=usage, batch=True)

    since = datetime.now(UTC) - timedelta(minutes=1)
    assert round(await ledger.spent_since(since), 6) == round(0.0015 + 0.00075, 6)
    assert await ledger.client_tokens_since("a", since) == 11_000
    assert await ledger.client_tokens_since("b", since) == 0
