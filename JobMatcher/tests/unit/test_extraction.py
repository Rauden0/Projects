import pytest

from jobmatcher.ingest.extraction import HeuristicExtractor, JobFacts, LLMExtractor

TEXT = """We are a hybrid team in Brno.
## Requirements
- 3+ years of Python (FastAPI or Django)
- PostgreSQL, RabbitMQ, Docker
"""


class ScriptedLLM:
    def __init__(self, *outputs: str) -> None:
        self.outputs = list(outputs)
        self.prompts: list[str] = []

    async def complete(self, *, system: str, user: str, max_tokens: int = 1024) -> str:
        self.prompts.append(user)
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
        '{"seniority": "lead"}',
        '```json\n{"seniority": "senior", "years_required_min": 5}\n```',
    )
    facts = await LLMExtractor(llm).extract("Senior Dev", None, "text")
    assert facts == JobFacts(seniority="senior", years_required_min=5)
    assert "Previous output was invalid" in llm.prompts[1]


@pytest.mark.asyncio
async def test_llm_extractor_falls_back_to_heuristics() -> None:
    llm = ScriptedLLM("not json", "still not json")
    facts = await LLMExtractor(llm).extract("Junior Dev", "Prague", "1 year of Go")
    assert facts.seniority == "junior"
    assert facts.years_required_min == 1
