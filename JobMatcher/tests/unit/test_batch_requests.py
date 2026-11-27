from types import SimpleNamespace
from typing import Any

import pytest
from sqlalchemy.ext.asyncio import async_sessionmaker

from jobmatcher.errors import LLMError
from jobmatcher.ingest.batch_extract import (
    BatchExtraction,
    PendingJob,
    batch_request,
    job_id_of,
    loop,
    outcome_of,
)
from jobmatcher.ingest.extraction import EXTRACTION_MAX_TOKENS


def test_batch_request_uses_compact_prompt_and_schema() -> None:
    job = PendingJob(7, "Dev", "Brno", "## About us\nWe are great.\n## Requirements\n- Go", b"h")
    request = batch_request(job, "claude-haiku-5-5", "low")
    params = request["params"]
    assert request["custom_id"] == "job-7"
    assert params["thinking"] == {"type": "disabled"}
    assert params["max_tokens"] == EXTRACTION_MAX_TOKENS
    assert "We are great" not in params["messages"][0]["content"]
    assert job_id_of(request["custom_id"]) == 7


def test_errored_result_has_no_facts() -> None:
    result = SimpleNamespace(custom_id="job-3", result=SimpleNamespace(type="errored"))
    outcome = outcome_of(result)
    assert (outcome.job_id, outcome.facts) == (3, None)


class SdkDown(Exception):
    pass


class DownBatches:
    async def create(self, requests: list[dict[str, Any]]) -> Any:
        raise SdkDown("overloaded")


class OnePending(BatchExtraction):
    async def pending_jobs(self, limit: int) -> list[PendingJob]:
        return [PendingJob(1, "Dev", None, "Go", b"h")]


class AllowAll:
    async def check(self, purpose: str) -> None:
        return None


def down_extraction() -> BatchExtraction:
    sdk = SimpleNamespace(messages=SimpleNamespace(batches=DownBatches()))
    return OnePending(
        sdk,
        async_sessionmaker(),
        None,
        AllowAll(),
        "m",
        sdk_errors=(SdkDown,),  # type: ignore[arg-type]
    )


async def test_sdk_error_is_raised_as_llm_error() -> None:
    with pytest.raises(LLMError):
        await down_extraction().run()


async def test_loop_survives_llm_error() -> None:
    await loop(down_extraction(), limit=1, every_s=None)
