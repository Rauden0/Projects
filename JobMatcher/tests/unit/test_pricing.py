import pytest

from jobmatcher.budget.pricing import TokenUsage, cost_usd, warn_if_unpriced


def test_haiku_cost_counts_cache_and_output() -> None:
    usage = TokenUsage(1_000_000, 1_000_000, cache_read_tokens=1_000_000, cache_write_tokens=0)
    assert cost_usd("claude-haiku-5-5", usage) == pytest.approx(0.10 + 0.50 + 0.01)


def test_batch_halves_cost() -> None:
    usage = TokenUsage(2000, 200)
    assert cost_usd("claude-haiku-5-5", usage, batch=True) == pytest.approx(
        cost_usd("claude-haiku-5-5", usage) / 2
    )


def test_local_models_are_free() -> None:
    assert cost_usd("qwen2.5:7b", TokenUsage(10_000, 10_000)) == 0.0


def test_total_tokens() -> None:
    assert TokenUsage(1, 2, 3, 4).total == 10


def test_unpriced_model_is_warned_about_once(caplog: pytest.LogCaptureFixture) -> None:
    warn_if_unpriced("claude-haiku-5-5")
    warn_if_unpriced("claude-haiku-5-5-20261101")
    warn_if_unpriced("claude-haiku-5-5-20261101")
    events = [(getattr(r, "event", None), getattr(r, "model", None)) for r in caplog.records]
    assert events == [("llm_model_unpriced", "claude-haiku-5-5-20261101")]
