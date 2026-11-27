"""Bounded in-process caches. Single event loop, no locks: every operation is synchronous."""

import re
import time
import unicodedata
from collections import OrderedDict
from collections.abc import Callable, Hashable

_SPACES = re.compile(r"\s+")


def normalize_text(text: str) -> str:
    return _SPACES.sub(" ", unicodedata.normalize("NFC", text)).strip()


class LruCache[K: Hashable, V]:
    def __init__(self, max_entries: int) -> None:
        if max_entries < 1:
            raise ValueError("max_entries must be at least 1")
        self.max_entries = max_entries
        self._entries: OrderedDict[K, V] = OrderedDict()

    def __len__(self) -> int:
        return len(self._entries)

    def get(self, key: K) -> V | None:
        value = self._entries.get(key)
        if value is not None:
            self._entries.move_to_end(key)
        return value

    def put(self, key: K, value: V) -> None:
        self._entries[key] = value
        self._entries.move_to_end(key)
        while len(self._entries) > self.max_entries:
            self._entries.popitem(last=False)


class TtlCache[K: Hashable, V]:
    """LRU-bounded, and an entry is served for at most `ttl_s` after it was stored."""

    def __init__(
        self, max_entries: int, ttl_s: float, clock: Callable[[], float] = time.monotonic
    ) -> None:
        self.ttl_s = ttl_s
        self.clock = clock
        self._entries: LruCache[K, tuple[float, V]] = LruCache(max_entries)

    def __len__(self) -> int:
        return len(self._entries)

    def get(self, key: K) -> V | None:
        entry = self._entries.get(key)
        if entry is None or entry[0] <= self.clock():
            return None
        return entry[1]

    def put(self, key: K, value: V) -> None:
        self._entries.put(key, (self.clock() + self.ttl_s, value))
