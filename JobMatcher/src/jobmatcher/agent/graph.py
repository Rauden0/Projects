"""The chat agent as a LangGraph state graph. `model` asks the LLM, `tools` runs the tool calls
it asked for, and the turn ends when the model answers or a step fails. Events reach the caller
through the custom stream while the graph runs. Provider-agnostic as long as the response has
Anthropic-style content blocks: {type: text, text} and {type: tool_use, id, name, input}."""

import json
import logging
import re
from collections.abc import AsyncIterator
from dataclasses import dataclass
from typing import Any, NotRequired, TypedDict

from langgraph.errors import GraphRecursionError
from langgraph.graph import END, START, StateGraph
from langgraph.graph.state import CompiledStateGraph
from langgraph.runtime import Runtime
from langgraph.types import StreamWriter
from sqlalchemy.exc import SQLAlchemyError

from jobmatcher import telemetry
from jobmatcher.agent.loop import (
    BUDGET_MESSAGE,
    COMPACT_CHARS,
    FINAL_ROUND_NOTE,
    LLM_ERROR_MESSAGE,
    MAX_CALLS_PER_ROUND,
    MAX_ROUNDS,
    TOOL_FAILURE_MESSAGE,
    UNFINISHED_MESSAGE,
    UNTRUSTED_CLOSE,
    UNTRUSTED_OPEN,
    AgentEvent,
)
from jobmatcher.agent.tools import Toolbox, strip_role_markers, without_links
from jobmatcher.errors import BudgetExceeded, LLMError, ToolError
from jobmatcher.llm import LLMClient

log = logging.getLogger(__name__)


class AgentState(TypedDict):
    """Plain JSON-like data only, so a checkpointer can persist it; collaborators live in
    `AgentContext`. `round` counts finished tool rounds."""

    messages: list[dict[str, Any]]
    round: int
    seen_urls: list[str]
    answer: NotRequired[str]
    error: NotRequired[str]


@dataclass(frozen=True, slots=True)
class AgentContext:
    llm: LLMClient
    toolbox: Toolbox
    system: str


def _block(block: Any, key: str) -> Any:
    return block[key] if isinstance(block, dict) else getattr(block, key)


def recursion_limit() -> int:
    """A model and a tools step per tool round and the final model step, plus one: LangGraph
    raises once the step count reaches the limit, even on the step that ends the run."""
    return 2 * MAX_ROUNDS + 2


async def run_turn(
    llm: LLMClient,
    toolbox: Toolbox,
    system: str,
    user_message: str,
    history: list[dict[str, Any]],
) -> AsyncIterator[AgentEvent]:
    state: AgentState = {
        "messages": [*history, {"role": "user", "content": user_message}],
        "round": 0,
        "seen_urls": [],
    }
    stream = GRAPH.astream(
        state,
        {"recursion_limit": recursion_limit()},
        context=AgentContext(llm, toolbox, system),
        stream_mode="custom",
    )
    try:
        async for event in stream:
            if isinstance(event, AgentEvent):
                yield event
    except GraphRecursionError as exc:
        log.warning("agent stopped: %s", exc)
        yield AgentEvent("error", {"message": UNFINISHED_MESSAGE})


async def _model(state: AgentState, runtime: Runtime[AgentContext]) -> dict[str, Any]:
    ctx, emit, round_no = runtime.context, runtime.stream_writer, state["round"]
    # On the last round tool use is disabled, so the model must answer with what it has.
    last_round = round_no >= MAX_ROUNDS
    try:
        response = await ctx.llm.create_message(
            system=ctx.system,
            messages=state["messages"],
            tools=ctx.toolbox.definitions(),
            tool_choice={"type": "none"} if last_round else {"type": "auto"},
        )
    except BudgetExceeded as exc:
        return _stop(emit, BUDGET_MESSAGE, round_no, exc)
    except LLMError as exc:
        return _stop(emit, LLM_ERROR_MESSAGE, round_no, exc)
    content = list(_block(response, "content"))

    if not any(_block(b, "type") == "tool_use" for b in content):
        answer = "".join(_block(b, "text") for b in content if _block(b, "type") == "text")
        emit(
            AgentEvent(
                "answer",
                {
                    "text": answer,
                    "unverified_urls": _unverified(answer, set(state["seen_urls"])),
                    "rounds": round_no + 1,
                },
            )
        )
        return {
            "messages": [*state["messages"], {"role": "assistant", "content": answer}],
            "answer": answer,
        }
    if last_round:
        return _stop(emit, UNFINISHED_MESSAGE, round_no, "tool use on the last round")

    messages = (
        state["messages"] if ctx.llm.caches_prompts else _compact_old_results(state["messages"])
    )
    assistant = {"role": "assistant", "content": [_as_dict(b) for b in content]}
    return {"messages": [*messages, assistant]}


async def _tools(state: AgentState, runtime: Runtime[AgentContext]) -> dict[str, Any]:
    ctx, emit, round_no = runtime.context, runtime.stream_writer, state["round"]
    tool_uses = _pending_tool_uses(state)
    results: list[dict[str, Any]] = []
    for i, use in enumerate(tool_uses):
        try:
            results.append(await _run_call(ctx.toolbox, emit, i, use))
        except (SQLAlchemyError, OSError) as exc:
            return _stop(emit, TOOL_FAILURE_MESSAGE, round_no, f"tool {use['name']} failed: {exc}")
    if round_no == MAX_ROUNDS - 1:
        results.append({"type": "text", "text": FINAL_ROUND_NOTE})
    return {
        "messages": [*state["messages"], {"role": "user", "content": results}],
        "round": round_no + 1,
        "seen_urls": sorted({*state["seen_urls"], *ctx.toolbox.seen_urls}),
    }


async def _run_call(
    toolbox: Toolbox, emit: StreamWriter, index: int, use: dict[str, Any]
) -> dict[str, Any]:
    name, tool_input, use_id = use["name"], use["input"], use["id"]
    emit(AgentEvent("tool_call", {"name": name, "input": tool_input}))
    output, is_error = await _call_tool(toolbox, index, name, tool_input, use_id)
    emit(
        AgentEvent(
            "tool_result",
            {
                "name": name,
                "is_error": is_error,
                "count": len(output) if isinstance(output, list) else None,
            },
        )
    )
    return {
        "type": "tool_result",
        "tool_use_id": use_id,
        "content": output if is_error else _untrusted(output),
        "is_error": is_error,
    }


def _stop(emit: StreamWriter, message: str, round_no: int, reason: object) -> dict[str, Any]:
    log.warning("agent stopped in round %d: %s", round_no, reason)
    emit(AgentEvent("error", {"message": message}))
    return {"error": message}


def _pending_tool_uses(state: AgentState) -> list[dict[str, Any]]:
    last = state["messages"][-1]
    if last["role"] != "assistant" or isinstance(last["content"], str):
        return []
    return [b for b in last["content"] if b["type"] == "tool_use"]


def route_after_model(state: AgentState) -> str:
    return "tools" if _pending_tool_uses(state) else END


def route_after_tools(state: AgentState) -> str:
    return END if "error" in state else "model"


def build_graph() -> CompiledStateGraph[AgentState, AgentContext, AgentState, AgentState]:
    graph = StateGraph(AgentState, context_schema=AgentContext)
    graph.add_node("model", _model)
    graph.add_node("tools", _tools)
    graph.add_edge(START, "model")
    graph.add_conditional_edges("model", route_after_model, ["tools", END])
    graph.add_conditional_edges("tools", route_after_tools, ["model", END])
    return graph.compile(name="chat_agent")


async def _call_tool(
    toolbox: Toolbox, index: int, name: str, tool_input: Any, use_id: str
) -> tuple[Any, bool]:
    with telemetry.tool_call(name, use_id, tool_input) as span:
        output, error = await _execute(toolbox, index, name, tool_input)
        span.result(output, error=error)
    return output, error is not None


async def _execute(
    toolbox: Toolbox, index: int, name: str, tool_input: Any
) -> tuple[Any, str | None]:
    """The tool's output, or the message the model gets instead, and the error type if any."""
    if index >= MAX_CALLS_PER_ROUND:
        return "too many tool calls in one round; ask again next turn", "too_many_calls"
    try:
        return await toolbox.call(name, tool_input), None
    except (ToolError, ValueError) as exc:
        return str(exc), type(exc).__name__


def _untrusted(output: Any) -> str:
    """Tool output marked as data: posting text is written by whoever published the ad. Role
    markers are removed and links in posting text are replaced, so the only links the model
    sees are the jobs' own `url` fields, the ones it may cite."""
    text = output if isinstance(output, str) else json.dumps(without_links(output), default=str)
    return f"{UNTRUSTED_OPEN}{strip_role_markers(text)}{UNTRUSTED_CLOSE}"


def _compact_old_results(messages: list[dict[str, Any]]) -> list[dict[str, Any]]:
    """Shorten tool results from earlier rounds. The model has already read them; keeping them
    whole makes every request re-send the full history, which quickly exceeds the per-minute
    token limits of free API tiers. Job ids, titles and URLs come first in each result, so a
    prefix keeps what later reasoning refers to."""
    return [_compact_message(message) for message in messages]


def _compact_message(message: dict[str, Any]) -> dict[str, Any]:
    if message["role"] != "user" or isinstance(message["content"], str):
        return message
    return {**message, "content": [_compact_block(block) for block in message["content"]]}


def _compact_block(block: dict[str, Any]) -> dict[str, Any]:
    content = block.get("content")
    if block.get("type") != "tool_result" or not isinstance(content, str):
        return block
    if len(content) <= COMPACT_CHARS:
        return block
    closing = UNTRUSTED_CLOSE if content.endswith(UNTRUSTED_CLOSE) else ""
    return {**block, "content": content[:COMPACT_CHARS] + " ...(truncated)" + closing}


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
# Trailing punctuation and Markdown emphasis (**url**, _url_); inside a URL both are legal.
_URL_TRAILERS = ".,;:!?*_"


def _unverified(answer: str, seen: set[str]) -> list[str]:
    """URLs in the answer that no tool returned — a cheap hallucination check."""
    urls = {u.rstrip(_URL_TRAILERS) for u in _URL_RE.findall(answer)}
    return sorted(u for u in urls if u not in seen)


GRAPH = build_graph()
