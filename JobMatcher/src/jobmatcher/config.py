from functools import lru_cache
from typing import Literal, Self

from pydantic import Field, SecretStr, model_validator
from pydantic_settings import BaseSettings, SettingsConfigDict

from jobmatcher.db.models import EMBEDDING_DIM
from jobmatcher.observability import LogFormat
from jobmatcher.observability import configure_logging as _configure_logging
from jobmatcher.telemetry import configure_tracing as _configure_tracing

Provider = Literal["anthropic", "openai_compat"]
Effort = Literal["low", "medium", "high", "max"]
LogLevel = Literal["DEBUG", "INFO", "WARNING", "ERROR"]


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", extra="ignore")

    database_url: str = Field(
        default="postgresql+asyncpg://jobmatcher:jobmatcher@localhost:5432/jobmatcher",
        pattern=r"^postgresql\+asyncpg://",
    )
    amqp_url: str = Field(default="amqp://guest:guest@localhost:5672/", pattern=r"^amqps?://")

    llm_provider: Provider = "openai_compat"
    llm_base_url: str = Field(default="http://localhost:11434/v1", pattern=r"^https?://\S+$")
    llm_api_key: SecretStr = SecretStr("")
    llm_model: str = Field(default="qwen2.5:7b", min_length=1)
    llm_timeout_s: float = Field(default=180.0, gt=0)
    # Constrain OpenAI-compatible JSON output to the schema (`response_format`). Ollama and
    # most servers support it; turn off for one that rejects the parameter.
    llm_structured_output: bool = True
    # Sent as `reasoning_effort` to OpenAI-compatible reasoning models (gpt-oss on Groq).
    llm_reasoning_effort: Literal["", "low", "medium", "high"] = ""
    # Optional overrides for the chat agent, which needs stronger tool use than extraction
    # (e.g. extraction on a local model, chat on a hosted one). Empty = same as above; a
    # different CHAT_LLM_PROVIDER inherits nothing, so the other provider's key never leaks.
    chat_llm_provider: Provider | Literal[""] = ""
    chat_llm_base_url: str = Field(default="", pattern=r"^(https?://\S+)?$")
    chat_llm_api_key: SecretStr = SecretStr("")
    chat_llm_model: str = ""
    llm_effort: Effort = "low"
    llm_monthly_budget_usd: float = Field(default=40.0, ge=0)
    llm_extraction_budget_share: float = Field(default=0.6, ge=0, le=1)
    extraction_mode: Literal["sync", "batch"] = "sync"
    chat_requests_per_minute: int = Field(default=6, ge=1)
    chat_daily_tokens_per_client: int = Field(default=300_000, ge=1)
    embedding_model: str = "intfloat/multilingual-e5-small"
    embedding_dim: int = EMBEDDING_DIM

    worker_prefetch: int = Field(default=8, ge=1)

    # Personal data: uploaded CVs are deleted this many days after upload, and the client
    # address on llm_usage rows (only needed for the daily per-client limit) after two days.
    cv_retention_days: int = Field(default=30, ge=1)
    usage_client_retention_days: int = Field(default=2, ge=2)
    # Published outbox rows are kept this long for debugging, then deleted.
    outbox_retention_days: int = Field(default=7, ge=1)

    log_level: LogLevel = "INFO"
    log_format: LogFormat = "text"

    # OpenTelemetry traces over OTLP/HTTP, e.g. http://phoenix:6006. Empty = no tracing.
    otel_exporter_otlp_endpoint: str = Field(default="", pattern=r"^(https?://\S+)?$")
    otel_service_name: str = Field(default="jobmatcher", min_length=1)
    # Prompts, completions and tool arguments and results on spans: CV and posting text.
    otel_capture_content: bool = False

    @model_validator(mode="after")
    def _consistent(self) -> Self:
        if self.embedding_dim != EMBEDDING_DIM:
            raise ValueError(
                f"EMBEDDING_DIM={self.embedding_dim} does not match the vector({EMBEDDING_DIM}) "
                "columns; changing it needs a migration"
            )
        if self.extraction_mode == "batch" and self.llm_provider != "anthropic":
            raise ValueError("EXTRACTION_MODE=batch needs LLM_PROVIDER=anthropic")
        self._check_chat_provider()
        return self

    def _check_chat_provider(self) -> None:
        if self.chat_llm_provider in ("", self.llm_provider):
            return
        if not self.chat_llm_model:
            raise ValueError("a CHAT_LLM_PROVIDER other than LLM_PROVIDER needs CHAT_LLM_MODEL")
        if self.chat_llm_provider == "openai_compat" and not self.chat_llm_base_url:
            raise ValueError("CHAT_LLM_PROVIDER=openai_compat needs CHAT_LLM_BASE_URL")


@lru_cache
def get_settings() -> Settings:
    return Settings()


def configure_logging() -> None:
    settings = get_settings()
    _configure_logging(settings.log_level, settings.log_format)


def configure_tracing() -> None:
    settings = get_settings()
    _configure_tracing(
        settings.otel_exporter_otlp_endpoint,
        service_name=settings.otel_service_name,
        capture_content=settings.otel_capture_content,
    )
