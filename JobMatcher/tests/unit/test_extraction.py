from typing import Any

import pytest

from jobmatcher.domain import JobFacts
from jobmatcher.errors import ExtractionError
from jobmatcher.ingest.extraction import (
    COMPACT_SCHEMA,
    HeuristicExtractor,
    LLMExtractor,
)

TEXT = """We are a hybrid team in Brno.
## Requirements
- 3+ years of Python (FastAPI or Django)
- PostgreSQL, RabbitMQ, Docker
"""


class ScriptedLLM:
    caches_prompts = False

    def __init__(self, *outputs: str) -> None:
        self.outputs = list(outputs)
        self.prompts: list[str] = []
        self.schemas: list[dict[str, Any]] = []

    async def complete(self, *, system: str, user: str, max_tokens: int = 1024) -> str:
        raise NotImplementedError

    async def complete_json(
        self, *, system: str, user: str, schema: dict[str, Any], max_tokens: int = 512
    ) -> str:
        self.prompts.append(user)
        self.schemas.append(schema)
        return self.outputs.pop(0)

    async def create_message(self, **_: object) -> None:
        raise NotImplementedError


@pytest.mark.asyncio
async def test_heuristic_extractor() -> None:
    facts = await HeuristicExtractor().extract("Medior Python Developer", "Brno", TEXT)
    assert facts.seniority == "medior"
    assert facts.years_required_min == 3
    assert facts.remote_policy == "hybrid"
    assert facts.office_city == "Brno"
    assert {"python", "fastapi", "postgresql", "rabbitmq", "docker"} <= set(facts.tech_stack)
    assert "go" not in facts.tech_stack


@pytest.mark.asyncio
async def test_llm_extractor_retries_with_validation_error() -> None:
    llm = ScriptedLLM(
        '{"sen": "lead"}',
        '```json\n{"sen": "senior", "yrs": 5}\n```',
    )
    facts = await LLMExtractor(llm).extract("Senior Dev", None, "text")
    assert facts == JobFacts(seniority="senior", years_required_min=5)
    assert "Previous output was invalid" in llm.prompts[1]


@pytest.mark.asyncio
async def test_llm_extractor_raises_after_two_invalid_outputs() -> None:
    llm = ScriptedLLM("not json", "still not json")
    with pytest.raises(ExtractionError):
        await LLMExtractor(llm).extract("Junior Dev", "Prague", "1 year of Go")


@pytest.mark.asyncio
async def test_heuristic_reads_singular_year() -> None:
    facts = await HeuristicExtractor().extract("Junior Dev", "Prague", "1 year of Go")
    assert facts.seniority == "junior"
    assert facts.years_required_min == 1


@pytest.mark.asyncio
async def test_compact_keys_map_to_job_facts() -> None:
    llm = ScriptedLLM(
        '{"sen": "medior", "yrs": 3, "rem": "hybrid", "city": "Brno", "smin": 80000,'
        ' "smax": 100000, "cur": "CZK", "ct": "employment", "tech": ["python"],'
        ' "req": ["python"], "nice": ["go"], "lang": ["english"]}'
    )
    facts = await LLMExtractor(llm).extract("Dev", "Brno", "text")
    assert facts.salary_max == 100000 and facts.currency == "CZK"
    assert facts.nice_to_have_skills == ["go"]
    assert llm.schemas[0] is COMPACT_SCHEMA


def test_compact_schema_requires_every_key() -> None:
    assert set(COMPACT_SCHEMA["required"]) == set(COMPACT_SCHEMA["properties"])
    assert COMPACT_SCHEMA["additionalProperties"] is False


@pytest.mark.parametrize(
    ("written", "iso"),
    [("Kč", "CZK"), ("czk", "CZK"), ("€", "EUR"), ("£", "GBP"), ("usd", "USD"), ("k", None)],
)
def test_currency_symbols_become_iso_codes_instead_of_failing(
    written: str, iso: str | None
) -> None:
    from jobmatcher.ingest.extraction import parse_compact

    facts = parse_compact(f'{{"smin": 38000, "cur": "{written}", "yrs": 2}}')
    assert facts.currency == iso
    assert facts.years_required_min == 2


@pytest.mark.parametrize(
    "bad",
    ['"yrs": 99', '"yrs": -1', '"smin": -5', '"smax": 3000000000', '"yrs": "many"'],
)
def test_out_of_range_number_drops_only_that_field(bad: str) -> None:
    from jobmatcher.ingest.extraction import parse_compact

    facts = parse_compact(f'{{"sen": "senior", "tech": ["go"], "cur": "EUR", {bad}}}')
    assert facts.seniority == "senior"
    assert facts.tech_stack == ["go"]
    assert facts.currency == "EUR"
    assert facts.years_required_min is None
    assert facts.salary_min is None and facts.salary_max is None


def test_salary_above_int4_is_rejected_by_job_facts() -> None:
    from pydantic import ValidationError

    with pytest.raises(ValidationError):
        JobFacts(salary_max=2_147_483_648)


@pytest.mark.asyncio
async def test_heuristic_ignores_implausible_years() -> None:
    facts = await HeuristicExtractor().extract("Dev", None, "Jsme 50 let na trhu.")
    assert facts.years_required_min is None
