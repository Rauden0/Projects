"""Tool-calling loop. Provider-agnostic as long as the response has Anthropic-style content
blocks: {type: text, text} and {type: tool_use, id, name, input}."""

import json
import logging
import re
from collections.abc import AsyncIterator
from dataclasses import dataclass
from typing import Any, Literal

from jobmatcher.agent.tools import Toolbox
from jobmatcher.errors import BudgetExceeded, LLMError, ToolError
from jobmatcher.llm import LLMClient

log = logging.getLogger(__name__)

MAX_ROUNDS = 4
# Calls of one round run one after another: the tools share the request's DB session.
MAX_CALLS_PER_ROUND = 3
COMPACT_CHARS = 600
FINAL_ROUND_NOTE = "No more tool calls are available. Answer now using only the results above."

SYSTEM = """You help a job seeker find jobs that fit their CV, in any occupation.
CV profile: {profile}

Rules:
- Always start with search_jobs. Call get_job and compare_with_cv only with a job_id that
  appeared in a search result; never guess ids.
- Put the occupation and skills into the query text. Use filters only for constraints the
  user stated: city for a place; max_years_required for experience; remote_policy only when
  the user asks for remote work, then pass all three remote values. Do not use the tech
  filter. For "in <city> or remote" set only city: results then come in two groups ("local"
  and "remote"); recommend the best fitting job from each group.
- Most postings are written in Czech. If a search finds nothing that fits, search again with
  the occupation in Czech (e.g. "zdravotní sestra", "řidič", "účetní") and fewer filters
  before concluding there are no jobs.
- Search results include a short "fit" (missing required skills, years gap); use it to
  choose. Call compare_with_cv only when a CV is provided and only for jobs you recommend.
- Only recommend jobs from tool results, linked by their exact URL, and describe where a job
  is exactly as its "where" field says. If nothing fits, say so and recommend nothing.
- Only skills listed in missing_required / missing_nice_to_have are missing.
- Answer in the user's language and keep it short: at most 5 jobs, one line each with the URL,
  why it fits and the main gap. No introduction, no closing summary."""


EventType = Literal["tool_call", "tool_result", "answer", "error"]


@dataclass(frozen=True, slots=True)
class AgentEvent:
    type: EventType
    data: dict[str, Any]


def _block(block: Any, key: str) -> Any:
    return block[key] if isinstance(block, dict) else getattr(block, key)


async def run_agent(
    llm: LLMClient,
    toolbox: Toolbox,
    user_message: str,
    history: list[dict[str, Any]],
    profile_summary: str,
    system_template: str = SYSTEM,
) -> AsyncIterator[AgentEvent]:
    messages = [*history, {"role": "user", "content": user_message}]
    system = system_template.format(profile=profile_summary or "not provided")

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
        except (LLMError, BudgetExceeded) as exc:
            log.warning("agent stopped in round %d: %s", round_no, exc)
            yield AgentEvent("error", {"message": f"LLM request failed: {exc}"[:500]})
            return
        content = list(_block(response, "content"))
        tool_uses = [b for b in content if _block(b, "type") == "tool_use"]

        if not tool_uses:
            answer = "".join(_block(b, "text") for b in content if _block(b, "type") == "text")
            yield AgentEvent(
                "answer",
                {
                    "text": answer,
                    "unverified_urls": _unverified(answer, toolbox.seen_urls),
                    "rounds": round_no + 1,
                },
            )
            return

        if not llm.caches_prompts:
            _compact_old_results(messages)
        messages.append({"role": "assistant", "content": [_as_dict(b) for b in content]})
        results = []
        for i, use in enumerate(tool_uses):
            name, tool_input, use_id = _block(use, "name"), _block(use, "input"), _block(use, "id")
            yield AgentEvent("tool_call", {"name": name, "input": tool_input})
            if i >= MAX_CALLS_PER_ROUND:
                output, is_error = "too many tool calls in one round; ask again next turn", True
            else:
                try:
                    output, is_error = await toolbox.call(name, tool_input), False
                except (ToolError, ValueError) as exc:
                    output, is_error = str(exc), True
            yield AgentEvent(
                "tool_result",
                {
                    "name": name,
                    "is_error": is_error,
                    "count": len(output) if isinstance(output, list) else None,
                },
            )
            results.append(
                {
                    "type": "tool_result",
                    "tool_use_id": use_id,
                    "content": output
                    if isinstance(output, str)
                    else json.dumps(output, default=str),
                    "is_error": is_error,
                }
            )
        if round_no == MAX_ROUNDS - 1:
            results.append({"type": "text", "text": FINAL_ROUND_NOTE})
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
    match block.type:
        case "tool_use":
            return {"type": "tool_use", "id": block.id, "name": block.name, "input": block.input}
        case "thinking":
            return {"type": "thinking", "thinking": block.thinking, "signature": block.signature}
        case "redacted_thinking":
            return {"type": "redacted_thinking", "data": block.data}
    return {"type": "text", "text": block.text}


# Stops at whitespace, brackets and quotes: models also answer in Markdown links or JSON.
_URL_RE = re.compile(r"https?://[^\s()\[\]<>\"'`]+")


def _unverified(answer: str, seen: set[str]) -> list[str]:
    """URLs in the answer that no tool returned — a cheap hallucination check."""
    urls = {u.rstrip(".,;:!?") for u in _URL_RE.findall(answer)}
    return sorted(u for u in urls if u not in seen)
