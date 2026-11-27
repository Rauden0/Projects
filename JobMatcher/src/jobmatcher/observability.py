import json
import logging
import time
import uuid
from contextvars import ContextVar
from dataclasses import dataclass, field
from typing import Literal

request_id_var: ContextVar[str | None] = ContextVar("request_id", default=None)

LogFormat = Literal["text", "json"]

_NOISY_LOGGERS = ("httpx", "httpcore", "huggingface_hub", "sentence_transformers", "aio_pika")
_STANDARD_ATTRS = set(vars(logging.makeLogRecord({}))) | {"message", "asctime", "request_id"}


def new_request_id() -> str:
    return uuid.uuid4().hex[:16]


class _RequestIdFilter(logging.Filter):
    def filter(self, record: logging.LogRecord) -> bool:
        record.request_id = request_id_var.get() or "-"
        return True


class JsonFormatter(logging.Formatter):
    def format(self, record: logging.LogRecord) -> str:
        entry: dict[str, object] = {
            "ts": self.formatTime(record, "%Y-%m-%dT%H:%M:%S%z"),
            "level": record.levelname,
            "logger": record.name,
            "msg": record.getMessage(),
        }
        request_id = getattr(record, "request_id", "-")
        if request_id != "-":
            entry["request_id"] = request_id
        entry.update({k: v for k, v in vars(record).items() if k not in _STANDARD_ATTRS})
        if record.exc_info:
            entry["exc"] = self.formatException(record.exc_info)
        return json.dumps(entry, ensure_ascii=False, default=str)


def configure_logging(level: str = "INFO", fmt: LogFormat = "text") -> None:
    handler = logging.StreamHandler()
    handler.addFilter(_RequestIdFilter())
    if fmt == "json":
        handler.setFormatter(JsonFormatter())
    else:
        handler.setFormatter(
            logging.Formatter("%(levelname)s %(name)s [%(request_id)s]: %(message)s")
        )
    root = logging.getLogger()
    root.handlers[:] = [handler]
    root.setLevel(level)
    for noisy in _NOISY_LOGGERS:
        logging.getLogger(noisy).setLevel(logging.WARNING)


@dataclass(frozen=True, slots=True)
class LlmCall:
    purpose: str
    model: str
    latency_s: float
    input_tokens: int
    output_tokens: int
    ok: bool


@dataclass(slots=True)
class UsageMeter:
    """Collects the LLM calls of one unit of work (a chat request, an eval case)."""

    calls: list[LlmCall] = field(default_factory=list)

    def add(self, call: LlmCall) -> None:
        self.calls.append(call)

    @property
    def input_tokens(self) -> int:
        return sum(c.input_tokens for c in self.calls)

    @property
    def output_tokens(self) -> int:
        return sum(c.output_tokens for c in self.calls)

    @property
    def latency_s(self) -> float:
        return sum(c.latency_s for c in self.calls)


class Stopwatch:
    def __init__(self) -> None:
        self._start = time.perf_counter()

    @property
    def elapsed_s(self) -> float:
        return time.perf_counter() - self._start
