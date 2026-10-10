"""Tool-calling loop. Provider-agnostic as long as the response has Anthropic-style content
blocks: {type: text, text} and {type: tool_use, id, name, input}."""

import json
import logging
import re
from collections.abc import AsyncIterator
from dataclasses import dataclass
from typing import Any

from jobmatcher.agent.tools import Toolbox, ToolError
from jobmatcher.llm import LLMClient

log = logging.getLogger(__name__)

MAX_ROUNDS = 6
MAX_PARALLEL_CALLS = 3
COMPACT_CHARS = 600

SYSTEM = """You help a software developer find jobs that fit their CV.
CV profile: {profile}

Rules:
- Only recommend jobs that appeared in tool results in this conversation, and link each one
  by its exact URL.
- Use search_jobs with filters only for the user's hard constraints (location, remote,
  years of experience); put skills into the query text. If a search returns nothing,
  search again with fewer filters before concluding there are no jobs.
- Use compare_with_cv, with a job_id taken from search results, before claiming a job is a
  good fit. Never invent job ids or URLs.
- Be explicit about gaps: missing required skills and years of experience.
- Answer in the user's language, concisely."""


@dataclass(frozen=True)
class AgentEvent:
    type: str  # tool_call | tool_result | answer | error
    data: dict[str, Any]


def _block(block: Any, key: str) -> Any:
    return block[key] if isinstance(block, dict) else getattr(block, key)


async def run_agent(
    llm: LLMClient,
    toolbox: Toolbox,
    user_message: str,
    history: list[dict[str, Any]],
    profile_summary: str,
) -> AsyncIterator[AgentEvent]:
    messages = [*history, {"role": "user", "content": user_message}]
    system = SYSTEM.format(profile=profile_summary or "not provided")

    for round_no in range(MAX_ROUNDS + 1):
        # On the last round tool use is disabled, so the model must answer with what it has.
        tool_choice = {"type": "auto"} if round_no < MAX_ROUNDS else {"type": "none"}
        try:
            response = await llm.create_message(
                system=system,
                messages=messages,
                tools=toolbox.definitions(),
                tool_choice=tool_choice,
            )
        except Exception as exc:
            # Surface provider failures to the client instead of cutting the stream.
            log.exception("LLM call failed")
            yield AgentEvent("error", {"message": f"LLM request failed: {exc}"[:500]})
            return
        content = list(_block(response, "content"))
        tool_uses = [b for b in content if _block(b, "type") == "tool_use"]

        if not tool_uses:
            answer = "".join(_block(b, "text") for b in content if _block(b, "type") == "text")
            yield AgentEvent("answer", {"text": answer, "unverified_urls": _unverified(
                answer, toolbox.seen_urls)})
            return

        _compact_old_results(messages)
        messages.append({"role": "assistant", "content": [_as_dict(b) for b in content]})
        results = []
        for i, use in enumerate(tool_uses):
            name, tool_input, use_id = _block(use, "name"), _block(use, "input"), _block(use, "id")
            yield AgentEvent("tool_call", {"name": name, "input": tool_input})
            if i >= MAX_PARALLEL_CALLS:
                output, is_error = "too many parallel tool calls; ask again next turn", True
            else:
                try:
                    output, is_error = await toolbox.call(name, tool_input), False
                except (ToolError, ValueError) as exc:
                    output, is_error = str(exc), True
            yield AgentEvent("tool_result", {"name": name, "is_error": is_error,
                                             "count": len(output) if isinstance(output, list)
                                             else None})
            results.append({
                "type": "tool_result",
                "tool_use_id": use_id,
                "content": output if isinstance(output, str) else json.dumps(output, default=str),
                "is_error": is_error,
            })
        messages.append({"role": "user", "content": results})

    yield AgentEvent("error", {"message": "agent did not finish"})  # pragma: no cover


def _compact_old_results(messages: list[dict[str, Any]]) -> None:
    """Shorten tool results from earlier rounds. The model has already read them; keeping them
    whole makes every request re-send the full history, which quickly exceeds the per-minute
    token limits of free API tiers. Job ids, titles and URLs come first in each result, so a
    prefix keeps what later reasoning refers to."""
    for message in messages:
        if message["role"] != "user" or isinstance(message["content"], str):
            continue
        for block in message["content"]:
            if block.get("type") == "tool_result" and len(block["content"]) > COMPACT_CHARS:
                block["content"] = block["content"][:COMPACT_CHARS] + " ...(truncated)"


def _as_dict(block: Any) -> dict[str, Any]:
    if isinstance(block, dict):
        return block
    if _block(block, "type") == "tool_use":
        return {"type": "tool_use", "id": block.id, "name": block.name, "input": block.input}
    return {"type": "text", "text": block.text}


def _unverified(answer: str, seen: set[str]) -> list[str]:
    """URLs in the answer that no tool returned — a cheap hallucination check."""
    urls = set(re.findall(r"https?://[^\s)\]>]+", answer))
    return sorted(u.rstrip(".,") for u in urls if u.rstrip(".,") not in seen)
