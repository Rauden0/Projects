"""The chat agent's entry point, prompt and limits. The agent itself is the LangGraph state
graph in `agent.graph`."""

from collections.abc import AsyncIterator
from dataclasses import dataclass
from typing import Any, Literal

from jobmatcher.agent.tools import Toolbox
from jobmatcher.llm import LLMClient

MAX_ROUNDS = 4
# Calls of one round run one after another: the tools share the request's DB session.
MAX_CALLS_PER_ROUND = 3
COMPACT_CHARS = 600
UNTRUSTED_OPEN, UNTRUSTED_CLOSE = "<untrusted_data>\n", "\n</untrusted_data>"
LLM_ERROR_MESSAGE = "The language model request failed, try again later."
TOOL_FAILURE_MESSAGE = "The job index is unavailable, try again later."
BUDGET_MESSAGE = "The chat budget is used up, try again later."
UNFINISHED_MESSAGE = "agent did not finish"
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


async def run_agent(
    llm: LLMClient,
    toolbox: Toolbox,
    user_message: str,
    history: list[dict[str, Any]],
    profile_summary: str,
    system_template: str = SYSTEM,
) -> AsyncIterator[AgentEvent]:
    # langgraph imports langchain-core, whose langsmith client imports the OpenTelemetry SDK
    # when it is installed; loading it on the first turn keeps the SDK out of every import path
    # that never chats, as `telemetry` promises.
    from jobmatcher.agent import graph

    system = system_template.format(profile=profile_summary or "not provided")
    async for event in graph.run_turn(llm, toolbox, system, user_message, history):
        yield event
