import pytest
from pydantic import ValidationError

from jobmatcher.config import Settings


def make(**overrides: object) -> Settings:
    return Settings(_env_file=None, **overrides)  # type: ignore[call-arg]


def test_defaults_are_valid_and_hide_secrets() -> None:
    settings = make(llm_api_key="sk-secret")
    assert settings.llm_provider == "openai_compat"
    assert "sk-secret" not in repr(settings)
    assert settings.llm_api_key.get_secret_value() == "sk-secret"


@pytest.mark.parametrize(
    "overrides",
    [
        {"llm_provider": "openai"},
        {"database_url": "postgresql://x@y/z"},
        {"amqp_url": "http://rabbit"},
        {"llm_extraction_budget_share": 1.5},
        {"chat_requests_per_minute": 0},
        {"embedding_dim": 1024},
        {"extraction_mode": "batch"},
        {"log_format": "xml"},
    ],
)
def test_invalid_settings_are_rejected(overrides: dict[str, object]) -> None:
    with pytest.raises(ValidationError):
        make(**overrides)


def test_batch_extraction_with_anthropic_is_valid() -> None:
    assert make(extraction_mode="batch", llm_provider="anthropic").extraction_mode == "batch"
