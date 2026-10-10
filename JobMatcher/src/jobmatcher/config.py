from functools import lru_cache

from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", extra="ignore")

    database_url: str = "postgresql+asyncpg://jobmatcher:jobmatcher@localhost:5432/jobmatcher"
    amqp_url: str = "amqp://guest:guest@localhost:5672/"

    # "anthropic" or "openai_compat" (Ollama, Groq, Gemini and other OpenAI-style APIs).
    llm_provider: str = "openai_compat"
    llm_base_url: str = "http://localhost:11434/v1"
    llm_api_key: str = ""
    llm_model: str = "qwen2.5:7b"
    llm_timeout_s: float = 180.0
    # Optional overrides for the chat agent, which needs stronger tool use than extraction
    # (e.g. extraction on a local model, chat on a hosted one). Empty = same as above.
    chat_llm_provider: str = ""
    chat_llm_base_url: str = ""
    chat_llm_api_key: str = ""
    chat_llm_model: str = ""
    embedding_model: str = "intfloat/multilingual-e5-small"
    embedding_dim: int = 384

    sources_file: str = "sources.yaml"
    crawl_interval_s: int = 6 * 3600
    http_concurrency_per_host: int = 4
    worker_prefetch: int = 8
    max_delivery_attempts: int = 3


@lru_cache
def get_settings() -> Settings:
    return Settings()


def configure_logging() -> None:
    import logging

    logging.basicConfig(level=logging.INFO, format="%(levelname)s %(name)s: %(message)s")
    # Per-request logs from HTTP clients (job boards, Hugging Face, Ollama) drown the app's own.
    for noisy in ("httpx", "huggingface_hub", "sentence_transformers"):
        logging.getLogger(noisy).setLevel(logging.WARNING)
