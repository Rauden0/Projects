import logging
from dataclasses import dataclass

log = logging.getLogger(__name__)

CACHE_READ_FACTOR = 0.1
CACHE_WRITE_FACTOR = 1.25
BATCH_FACTOR = 0.5
PER_TOKEN = 1 / 1_000_000


@dataclass(frozen=True, slots=True)
class Price:
    input_per_mtok: float
    output_per_mtok: float


PRICES: dict[str, Price] = {
    "claude-haiku-5-5": Price(0.10, 0.50),
    "claude-sonnet-5-5": Price(2.00, 10.00),
    "claude-opus-5-5": Price(4.00, 20.00),
}


@dataclass(frozen=True, slots=True)
class TokenUsage:
    input_tokens: int
    output_tokens: int
    cache_read_tokens: int = 0
    cache_write_tokens: int = 0

    @property
    def total(self) -> int:
        return (
            self.input_tokens
            + self.output_tokens
            + self.cache_read_tokens
            + self.cache_write_tokens
        )


def cost_usd(model: str, usage: TokenUsage, batch: bool = False) -> float:
    price = PRICES.get(model)
    if price is None:
        return 0.0
    input_cost = price.input_per_mtok * (
        usage.input_tokens
        + usage.cache_read_tokens * CACHE_READ_FACTOR
        + usage.cache_write_tokens * CACHE_WRITE_FACTOR
    )
    total = (input_cost + price.output_per_mtok * usage.output_tokens) * PER_TOKEN
    return total * BATCH_FACTOR if batch else total


_warned_unpriced: set[str] = set()


def warn_if_unpriced(model: str) -> None:
    """Calls to a model missing from PRICES cost $0, so the monthly cap never stops them."""
    if model in PRICES or model in _warned_unpriced:
        return
    _warned_unpriced.add(model)
    log.warning(
        "llm_model_unpriced model=%s: its calls are recorded at $0 and the budget cap does "
        "not apply; add it to budget/pricing.py PRICES",
        model,
        extra={"event": "llm_model_unpriced", "model": model},
    )
