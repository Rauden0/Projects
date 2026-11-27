import json
import logging

from jobmatcher.observability import JsonFormatter, LlmCall, UsageMeter, request_id_var


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
