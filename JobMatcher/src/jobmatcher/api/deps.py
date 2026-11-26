from collections.abc import AsyncIterator
from functools import lru_cache
from typing import Annotated

from fastapi import Depends
from sqlalchemy.ext.asyncio import AsyncSession

from jobmatcher.budget.guard import BudgetGuard, BudgetPolicy
from jobmatcher.budget.ledger import UsageLedger
from jobmatcher.budget.limits import ChatGate, ClientLimiter
from jobmatcher.config import get_settings
from jobmatcher.db.session import get_sessionmaker
from jobmatcher.search.embeddings import Embedder, get_embedder


async def get_session() -> AsyncIterator[AsyncSession]:
    async with get_sessionmaker()() as session:
        yield session


SessionDep = Annotated[AsyncSession, Depends(get_session)]
EmbedderDep = Annotated[Embedder, Depends(get_embedder)]


@lru_cache
def get_chat_gate() -> ChatGate:
    settings = get_settings()
    ledger = UsageLedger(get_sessionmaker())
    policy = BudgetPolicy(settings.llm_monthly_budget_usd, settings.llm_extraction_budget_share)
    limiter = ClientLimiter(
        ledger, settings.chat_requests_per_minute, settings.chat_daily_tokens_per_client
    )
    return ChatGate(limiter, BudgetGuard(ledger, policy))


ChatGateDep = Annotated[ChatGate, Depends(get_chat_gate)]
