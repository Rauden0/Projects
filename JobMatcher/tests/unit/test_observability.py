import json
import logging

from jobmatcher.observability import (
    JsonFormatter,
    LlmCall,
    UsageMeter,
    configure_logging,
    request_id_var,
)


def test_json_lines_carry_request_id_and_extra_fields() -> None:
    token = request_id_var.set("abc123")
    try:
        record = logging.makeLogRecord(
            {"name": "jobmatcher.llm", "levelname": "INFO", "msg": "llm_call", "latency_ms": 812}
        )
        record.request_id = request_id_var.get()
        line = json.loads(JsonFormatter().format(record))
    finally:
        request_id_var.reset(token)

    assert line["msg"] == "llm_call"
    assert line["request_id"] == "abc123"
    assert line["latency_ms"] == 812


def test_usage_meter_sums_calls() -> None:
    meter = UsageMeter()
    meter.add(LlmCall("chat", "m", 0.5, 100, 10, True))
    meter.add(LlmCall("chat", "m", 1.5, 200, 20, True))
    assert (meter.input_tokens, meter.output_tokens, meter.latency_s) == (300, 30, 2.0)


def test_uvicorn_loggers_go_through_the_root_handler() -> None:
    access = logging.getLogger("uvicorn.access")
    access.addHandler(logging.StreamHandler())
    access.propagate = False
    root = logging.getLogger()
    saved = root.handlers[:], root.level
    try:
        configure_logging("INFO", "json")
        assert not access.handlers
        assert access.propagate
        assert isinstance(root.handlers[0].formatter, JsonFormatter)
    finally:
        root.handlers[:], level = saved
        root.setLevel(level)
