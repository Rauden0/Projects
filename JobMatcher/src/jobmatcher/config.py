from functools import lru_cache
from typing import Literal, Self

from pydantic import Field, SecretStr, model_validator
from pydantic_settings import BaseSettings, SettingsConfigDict

from jobmatcher.db.models import EMBEDDING_DIM
from jobmatcher.observability import LogFormat
from jobmatcher.observability import configure_logging as _configure_logging

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
    llm_base_url: str = "http://localhost:11434/v1"
    llm_api_key: SecretStr = SecretStr("")
    llm_model: str = Field(default="qwen2.5:7b", min_length=1)
    llm_timeout_s: float = Field(default=180.0, gt=0)
    # Constrain OpenAI-compatible JSON output to the schema (`response_format`). Ollama and
    # most servers support it; turn off for one that rejects the parameter.
    llm_structured_output: bool = True
    # Optional overrides for the chat agent, which needs stronger tool use than extraction
    # (e.g. extraction on a local model, chat on a hosted one). Empty = same as above.
    chat_llm_provider: Provider | Literal[""] = ""
    chat_llm_base_url: str = ""
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

    log_level: LogLevel = "INFO"
    log_format: LogFormat = "text"

    @model_validator(mode="after")
    def _consistent(self) -> Self:
        if self.embedding_dim != EMBEDDING_DIM:
            raise ValueError(
                f"EMBEDDING_DIM={self.embedding_dim} does not match the vector({EMBEDDING_DIM}) "
                "columns; changing it needs a migration"
            )
        if self.extraction_mode == "batch" and self.llm_provider != "anthropic":
            raise ValueError("EXTRACTION_MODE=batch needs LLM_PROVIDER=anthropic")
        return self


@lru_cache
def get_settings() -> Settings:
    return Settings()


def configure_logging() -> None:
    settings = get_settings()
    _configure_logging(settings.log_level, settings.log_format)
