from jobmatcher.caching import TtlCache, normalize_text


class Clock:
    def __init__(self) -> None:
        self.now = 0.0

    def __call__(self) -> float:
        return self.now


def test_ttl_cache_expires_entries() -> None:
    clock = Clock()
    cache: TtlCache[str, int] = TtlCache(max_entries=4, ttl_s=60, clock=clock)
    cache.put("a", 1)
    clock.now = 59.9
    assert cache.get("a") == 1
    clock.now = 60
    assert cache.get("a") is None


def test_ttl_cache_is_bounded() -> None:
    cache: TtlCache[int, int] = TtlCache(max_entries=2, ttl_s=60, clock=Clock())
    for i in range(3):
        cache.put(i, i)
    assert len(cache) == 2
    assert cache.get(0) is None


def test_normalize_text_collapses_whitespace_and_composes() -> None:
    assert normalize_text("  Brně \n python ") == "Brně python"
