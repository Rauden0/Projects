# ADR 0002: LangGraph for the chat agent's orchestration

- **Status:** Accepted
- **Date:** 2026-11-27
- **Supersedes:** [ADR 0001](0001-llm-framework.md), for the agent loop only
- **Scope:** `src/jobmatcher/agent/`

## Context

ADR 0001 recommended keeping the hand-written tool-calling loop and adopting no agent
framework. The project owner decided otherwise: the chat agent runs on LangGraph, so its
control flow is an explicit graph that can grow persistence, human-in-the-loop steps and
more nodes without a rewrite. This ADR records how that is done without giving up what
ADR 0001 found valuable.

## Decision

**The chat agent is a LangGraph `StateGraph` (`langgraph>=1.2,<2`, core only).**
Everything below the orchestration stays custom.

- **State** (`agent/graph.py`, `AgentState`): `messages` (Anthropic-style dicts), `round`
  (finished tool rounds), `seen_urls` (URLs tool results returned), and `answer` or `error`
  once the turn ends. Only plain data; collaborators (`LLMClient`, `Toolbox`, the system
  prompt) come in as the run's `context` (`AgentContext`).
- **Nodes:** `model` calls `LLMClient.create_message` exactly as the loop did (`tool_choice`
  none plus `FINAL_ROUND_NOTE` on the last round, compaction of old results on providers
  without prompt caching). `tools` runs at most `MAX_CALLS_PER_ROUND` calls one after another
  under `execute_tool {name}` spans and wraps outputs in the untrusted-data envelope.
- **Edges:** `START → model`; `model → tools` when the last assistant message has tool uses,
  else `END`; `tools → model`, or `END` after a database or OS error.
- **Events:** nodes write `AgentEvent`s to LangGraph's custom stream (`runtime.stream_writer`),
  and `run_agent` yields them as they are written, so the SSE contract (`tool_call`,
  `tool_result`, `answer`, `error`) and its timing are unchanged.
- **Limits:** `recursion_limit` is derived from `MAX_ROUNDS` (two steps per tool round, the
  final model step, plus one because LangGraph counts the step that ends the run). Hitting it
  ends the turn with an `error` event, like a tool use on the last round.

What stays custom, and why:

- **`llm.py`.** No LangChain chat-model wrappers. `LLMClient` keeps the budget check, the
  `llm_usage` ledger, `llm_call` logging, `chat {model}` spans, Anthropic cache breakpoints
  and the OpenAI-compatible translation with its Groq workarounds.
- **Message Batches and extraction** (`ingest/`) do not touch LangGraph.
- **Telemetry.** The `chat`, `chat {model}` and `execute_tool {name}` spans come from
  `telemetry.py` as before. LangSmith tracing stays off (it needs `LANGSMITH_TRACING`).
- **Tools.** `Toolbox` and its Pydantic schemas, shared with the MCP server.

## Consequences

**Positive**

- The control flow is declared (nodes, routing functions) and each piece is testable on its
  own; adding a node (a reranker, a guard, an approval step) is a local change.
- Stateless turns today, but the state is already serialisable: compiling the graph with a
  checkpointer (e.g. `langgraph-checkpoint-postgres`) and passing a `thread_id` would keep the
  conversation server-side instead of re-sending it from the client, and `interrupt()` would
  give human-in-the-loop approval steps.

**Negative / accepted costs**

- 17 more installed packages (`langchain-core`, `langsmith`, `langgraph-checkpoint`, …)
  for an orchestration that was ~150 lines.
- `langsmith` imports the OpenTelemetry SDK at import time when it is installed, so
  `agent.loop` imports the graph module on the first turn, not at startup.
- The scripted fake model still works, but tests now exercise LangGraph's runner, and
  LangGraph's release cadence becomes something to track (pinned below 2).
- One behaviour change: a model that still asks for tools on the tool-less last round now
  ends the turn with `agent did not finish` without running those calls.
