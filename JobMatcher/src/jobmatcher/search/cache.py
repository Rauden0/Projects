from collections.abc import Awaitable, Callable, Hashable
from typing import Any

from sqlalchemy.ext.asyncio import AsyncSession

from jobmatcher import metrics
from jobmatcher.caching import TtlCache
from jobmatcher.db.cache_store import index_version
from jobmatcher.search.hybrid import SearchHit

Version = Callable[[AsyncSession], Awaitable[Hashable]]


class SearchCache:
    """Search responses for `ttl_s`, keyed by the request and the index version: a new or changed
    job is visible at once, other changes after at most `ttl_s`."""

    def __init__(self, max_entries: int, ttl_s: float, version: Version = index_version) -> None:
        self.ttl_s = ttl_s
        self.version = version
        self._hits: TtlCache[tuple[Any, ...], tuple[SearchHit, ...]] = TtlCache(max_entries, ttl_s)

    async def fetch(
        self,
        session: AsyncSession,
        request: tuple[Hashable, ...],
        run: Callable[[], Awaitable[list[SearchHit]]],
    ) -> list[SearchHit]:
        if self.ttl_s <= 0:
            return await run()
        key = (*request, await self.version(session))
        hits = self._hits.get(key)
        metrics.recorder.cache_lookup("search", hits is not None)
        if hits is None:
            hits = tuple(await run())
            self._hits.put(key, hits)
        return list(hits)
