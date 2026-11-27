from types import SimpleNamespace

from jobmatcher.ingest.batch_extract import PendingJob, batch_request, job_id_of, outcome_of


def test_batch_request_uses_compact_prompt_and_schema() -> None:
    job = PendingJob(7, "Dev", "Brno", "## About us\nWe are great.\n## Requirements\n- Go")
    request = batch_request(job, "claude-haiku-5-5", "low")
    params = request["params"]
    assert request["custom_id"] == "job-7"
    assert params["thinking"] == {"type": "disabled"}
    assert "We are great" not in params["messages"][0]["content"]
    assert job_id_of(request["custom_id"]) == 7


def test_errored_result_has_no_facts() -> None:
    result = SimpleNamespace(custom_id="job-3", result=SimpleNamespace(type="errored"))
    outcome = outcome_of(result)
    assert (outcome.job_id, outcome.facts) == (3, None)
