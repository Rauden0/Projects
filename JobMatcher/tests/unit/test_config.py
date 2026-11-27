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
        {"otel_exporter_otlp_endpoint": "phoenix:6006"},
        {"otel_service_name": ""},
        {"otel_capture_content": "sometimes"},
        {"llm_base_url": "localhost:11434/v1"},
        {"llm_base_url": ""},
        {"chat_llm_base_url": "ftp://groq"},
        {"llm_provider": "anthropic", "chat_llm_provider": "openai_compat"},
        {
            "llm_provider": "anthropic",
            "chat_llm_provider": "openai_compat",
            "chat_llm_model": "openai/gpt-oss-120b",
        },
        {"chat_llm_provider": "anthropic"},
    ],
)
def test_invalid_settings_are_rejected(overrides: dict[str, object]) -> None:
    with pytest.raises(ValidationError):
        make(**overrides)


def test_batch_extraction_with_anthropic_is_valid() -> None:
    assert make(extraction_mode="batch", llm_provider="anthropic").extraction_mode == "batch"


def test_tracing_is_off_and_without_content_by_default() -> None:
    settings = make()
    assert settings.otel_exporter_otlp_endpoint == ""
    assert settings.otel_capture_content is False


def test_otlp_endpoint_accepts_http_urls() -> None:
    endpoint = make(otel_exporter_otlp_endpoint="http://phoenix:6006").otel_exporter_otlp_endpoint
    assert endpoint == "http://phoenix:6006"


def test_chat_on_another_provider_with_explicit_values_is_valid() -> None:
    settings = make(
        llm_provider="anthropic",
        chat_llm_provider="openai_compat",
        chat_llm_base_url="https://api.groq.com/openai/v1",
        chat_llm_model="openai/gpt-oss-120b",
    )
    assert settings.chat_llm_provider == "openai_compat"
