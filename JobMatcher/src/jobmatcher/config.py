from functools import lru_cache

from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", extra="ignore")

    database_url: str = "postgresql+asyncpg://jobmatcher:jobmatcher@localhost:5432/jobmatcher"
    amqp_url: str = "amqp://guest:guest@localhost:5672/"

    llm_api_key: str = ""
    llm_model: str = "claude-sonnet-5-5"
    embedding_model: str = "intfloat/multilingual-e5-large"
    embedding_dim: int = 1024

    sources_file: str = "sources.yaml"
    crawl_interval_s: int = 6 * 3600
    http_concurrency_per_host: int = 4
    worker_prefetch: int = 8
    max_delivery_attempts: int = 3


@lru_cache
def get_settings() -> Settings:
    return Settings()
