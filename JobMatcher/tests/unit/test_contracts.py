import json
import os
from datetime import UTC, datetime
from pathlib import Path

import pytest
from pydantic import BaseModel

from jobmatcher.domain import RawPosting, SourceCrawled

CONTRACTS = Path(__file__).parents[2] / "docs" / "contracts"
SCHEMAS: dict[str, type[BaseModel]] = {
    "job.discovered.json": RawPosting,
    "source.crawled.json": SourceCrawled,
}


def rendered(model: type[BaseModel]) -> str:
    return json.dumps(model.model_json_schema(), indent=2, ensure_ascii=False) + "\n"


@pytest.mark.parametrize("name", sorted(SCHEMAS))
def test_committed_schema_matches_model(name: str) -> None:
    path = CONTRACTS / name
    if os.environ.get("UPDATE_CONTRACTS"):
        path.write_text(rendered(SCHEMAS[name]), encoding="utf-8")
    assert path.read_text(encoding="utf-8") == rendered(SCHEMAS[name]), (
        f"{name} drifted from the model; run UPDATE_CONTRACTS=1 pytest {__file__}"
    )


def test_v0_posting_without_envelope_fields_still_parses() -> None:
    posting = RawPosting.model_validate_json(
        '{"kind": "greenhouse", "company": "acme", "external_id": "1", "url": "https://x",'
        ' "title": "Dev", "location": null, "description_html": "<p>x</p>"}'
    )
    assert (posting.schema_version, posting.event_id, posting.crawled_at) == (1, None, None)


def test_unknown_fields_are_ignored_and_naive_crawled_at_is_utc() -> None:
    event = SourceCrawled.model_validate(
        {
            "kind": "greenhouse",
            "source": "acme",
            "external_ids": [],
            "complete": True,
            "schema_version": 2,
            "crawled_at": "2026-11-27T10:00:00",
            "added_later": "x",
        }
    )
    assert event.schema_version == 2
    assert event.crawled_at == datetime(2026, 11, 27, 10, tzinfo=UTC)
