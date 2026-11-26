from dataclasses import dataclass, field
from types import SimpleNamespace
from typing import Any

import pytest

from jobmatcher.budget.guard import BudgetExceeded, BudgetGuard, BudgetPolicy
from jobmatcher.budget.pricing import TokenUsage
from jobmatcher.llm import AnthropicClient, usage_of


def response(text: str = '{"sen": null}') -> SimpleNamespace:
    return SimpleNamespace(
        content=[SimpleNamespace(type="text", text=text)],
        usage=SimpleNamespace(
            input_tokens=900,
            output_tokens=40,
            cache_read_input_tokens=300,
            cache_creation_input_tokens=None,
        ),
    )


@dataclass
class FakeMessages:
    calls: list[dict[str, Any]] = field(default_factory=list)

    async def create(self, **params: Any) -> SimpleNamespace:
        self.calls.append(params)
        return response()


@dataclass
class FakeSdk:
    messages: FakeMessages = field(default_factory=FakeMessages)


@dataclass
class RecordingLedger:
    spent: float = 0.0
    records: list[dict[str, Any]] = field(default_factory=list)

    async def record(self, **entry: Any) -> float:
        self.records.append(entry)
        return 0.0

    async def spent_since(self, since: object) -> float:
        return self.spent


def client(ledger: RecordingLedger, **kwargs: Any) -> tuple[AnthropicClient, FakeSdk]:
    sdk = FakeSdk()
    llm = AnthropicClient(
        "key",
        "claude-haiku-5-5",
        ledger=ledger,  # type: ignore[arg-type]
        guard=BudgetGuard(ledger, BudgetPolicy(40.0)),
        sdk=sdk,
        **kwargs,
    )
    return llm, sdk


def test_usage_of_reads_cache_tokens() -> None:
    assert usage_of(response()) == TokenUsage(900, 40, 300, 0)


async def test_complete_json_uses_structured_output_without_thinking() -> None:
    ledger = RecordingLedger()
    llm, sdk = client(ledger)
    text = await llm.complete_json(system="s", user="u", schema={"type": "object"})

    (call,) = sdk.messages.calls
    assert text == '{"sen": null}'
    assert call["model"] == "claude-haiku-5-5"
    assert call["thinking"] == {"type": "disabled"}
    assert call["output_config"]["effort"] == "low"
    assert call["output_config"]["format"]["type"] == "json_schema"
    assert ledger.records[0]["purpose"] == "extraction"


async def test_chat_caches_tools_and_system_and_records_client() -> None:
    ledger = RecordingLedger()
    llm, sdk = client(ledger, purpose="chat", client_id="abc")
    tools = [{"name": "a"}, {"name": "b"}]
    await llm.create_message(system="s", messages=[], tools=tools)

    (call,) = sdk.messages.calls
    assert "cache_control" not in call["tools"][0]
    assert call["tools"][1]["cache_control"] == {"type": "ephemeral"}
    assert call["system"][0]["cache_control"] == {"type": "ephemeral"}
    assert call["cache_control"] == {"type": "ephemeral"}
    assert "cache_control" not in tools[1]
    assert ledger.records[0]["client_id"] == "abc"


async def test_exhausted_budget_blocks_the_call() -> None:
    ledger = RecordingLedger(spent=41.0)
    llm, sdk = client(ledger)
    with pytest.raises(BudgetExceeded):
        await llm.complete(system="s", user="u")
    assert not sdk.messages.calls
