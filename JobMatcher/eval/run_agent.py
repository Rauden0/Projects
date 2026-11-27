"""Scripted questions to the chat agent over the frozen fixtures.

    python eval/run_agent.py --model qwen2.5:7b          # local Ollama
    python eval/run_agent.py --chat-model                # the CHAT_LLM_* model from .env

Seeds EVAL_DATABASE_URL (default jobmatcher_eval; truncated) with eval/fixtures/postings.yaml
under https:// URLs, so the agent's unverified-URL check applies, then runs every case in
eval/agent/cases.yaml through the production loop and tools. Reports per case: tools called,
expected tools used, a relevant posting cited, citations valid (every URL in the answer came
from a tool), rounds, latency and tokens. Writes the transcripts to eval/agent/results/.
"""

import argparse
import asyncio
import json
import re
import statistics
from dataclasses import asdict, dataclass, field
from datetime import date
from pathlib import Path
from typing import Any

import yaml
from run_retrieval import load_fixtures, use_eval_database

from jobmatcher.agent.loop import SYSTEM, run_agent
from jobmatcher.agent.tools import Toolbox
from jobmatcher.config import get_settings
from jobmatcher.cv.profile import build_profile
from jobmatcher.db.models import Cv
from jobmatcher.db.session import get_sessionmaker
from jobmatcher.llm import OpenAICompatClient
from jobmatcher.observability import Stopwatch, UsageMeter
from jobmatcher.search.embeddings import Embedder, get_embedder

HERE = Path(__file__).parent / "agent"
URL_PREFIX = "https://jobs.example/"


@dataclass
class CaseResult:
    id: str
    tools: list[str] = field(default_factory=list)
    tool_inputs: list[dict[str, Any]] = field(default_factory=list)
    tool_errors: int = 0
    expected_tools: bool = False
    filters_ok: bool = True
    relevant_cited: bool = False
    citations_valid: bool = False
    cited: list[str] = field(default_factory=list)
    unverified: list[str] = field(default_factory=list)
    rounds: int = 0
    latency_s: float = 0.0
    input_tokens: int = 0
    output_tokens: int = 0
    max_prompt_tokens: int = 0
    error: str | None = None
    answer: str = ""


async def seed_cv(text: str, embedder: Embedder) -> tuple[Any, str]:
    profile = await build_profile(text, today=date(2026, 11, 27))
    async with get_sessionmaker()() as session:
        cv = Cv(
            raw_text=text,
            profile=profile.model_dump(),
            embedding=await embedder.embed_query(text),
        )
        session.add(cv)
        await session.commit()
        return cv.id, json.dumps(profile.model_dump(), ensure_ascii=False)


def make_llm(args: argparse.Namespace, meter: UsageMeter) -> tuple[str, OpenAICompatClient]:
    settings = get_settings()
    if args.chat_model:
        base_url = settings.chat_llm_base_url or settings.llm_base_url
        model = settings.chat_llm_model or settings.llm_model
        api_key = settings.chat_llm_api_key.get_secret_value()
    else:
        base_url, model, api_key = args.base_url, args.model, ""
    llm = OpenAICompatClient(
        base_url,
        model,
        api_key,
        args.timeout,
        purpose="chat",
        reasoning_effort=args.reasoning_effort,
        meter=meter,
    )
    return args.label or model, llm


def filters_match(inputs: list[dict[str, Any]], expected: dict[str, Any]) -> bool:
    if not expected:
        return True
    return any(all(i.get(k) == v for k, v in expected.items()) for i in inputs)


async def run_case(
    case: dict[str, Any],
    llm: OpenAICompatClient,
    meter: UsageMeter,
    embedder: Embedder,
    cv: tuple[Any, str],
    system_template: str,
) -> CaseResult:
    meter.calls.clear()
    result = CaseResult(id=case["id"])
    cv_id, summary = cv if case.get("with_cv") else (None, "")
    watch = Stopwatch()
    async with get_sessionmaker()() as session:
        toolbox = Toolbox(session, embedder, cv_id)
        events = run_agent(llm, toolbox, case["question"], [], summary, system_template)
        async for event in events:
            if event.type == "tool_call":
                result.tools.append(event.data["name"])
                result.tool_inputs.append(event.data["input"])
            elif event.type == "tool_result":
                result.tool_errors += event.data["is_error"]
            elif event.type == "answer":
                result.answer = event.data["text"]
                result.unverified = event.data["unverified_urls"]
                result.rounds = event.data["rounds"]
            else:
                result.error = event.data["message"]
    result.latency_s = watch.elapsed_s
    result.input_tokens, result.output_tokens = meter.input_tokens, meter.output_tokens
    result.max_prompt_tokens = max((c.input_tokens for c in meter.calls), default=0)

    result.cited = sorted(set(re.findall(rf"{re.escape(URL_PREFIX)}([\w-]+)", result.answer)))
    relevant = set(case["relevant"])
    result.expected_tools = set(case["expect_tools"]) <= set(result.tools)
    search_inputs = [
        i for t, i in zip(result.tools, result.tool_inputs, strict=True) if t == "search_jobs"
    ]
    result.filters_ok = filters_match(search_inputs, case.get("expect_filters", {}))
    result.relevant_cited = bool(relevant & set(result.cited)) if relevant else not result.cited
    result.citations_valid = result.error is None and not result.unverified
    return result


def read_prompt(path: str) -> str:
    return Path(path).read_text(encoding="utf-8").strip()


def pct(values: list[bool]) -> str:
    return f"{100 * sum(values) / len(values):.0f}%"


def yes(values: list[bool]) -> str:
    return f"{sum(values)}/{len(values)}"


def report(label: str, results: list[CaseResult], repeats: int) -> str:
    lines = [
        f"### {label} ({repeats} run{'s' if repeats > 1 else ''} per case)",
        "",
        "| case | tools (first run) | expected tools | filters | relevant cited "
        "| citations valid | rounds | latency |",
        "|---|---|---|---|---|---|---|---|",
    ]
    by_case: dict[str, list[CaseResult]] = {}
    for r in results:
        by_case.setdefault(r.id, []).append(r)
    for case_id, runs in by_case.items():
        tools = ", ".join(runs[0].tools) or "-"
        lines.append(
            f"| {case_id} | {tools} | {yes([r.expected_tools for r in runs])} "
            f"| {yes([r.filters_ok for r in runs])} | {yes([r.relevant_cited for r in runs])} "
            f"| {yes([r.citations_valid for r in runs])} "
            f"| {statistics.mean(r.rounds for r in runs):.1f} "
            f"| {statistics.mean(r.latency_s for r in runs):.1f}s |"
        )
    latencies = [r.latency_s for r in results]
    lines += [
        "",
        f"Expected tools {pct([r.expected_tools for r in results])} · filters "
        f"{pct([r.filters_ok for r in results])} · relevant cited "
        f"{pct([r.relevant_cited for r in results])} · citations valid "
        f"{pct([r.citations_valid for r in results])} · mean rounds "
        f"{statistics.mean(r.rounds for r in results):.1f} · latency p50 "
        f"{statistics.median(latencies):.1f}s, max {max(latencies):.1f}s · tokens/question "
        f"{statistics.mean(r.input_tokens for r in results):.0f} in, "
        f"{statistics.mean(r.output_tokens for r in results):.0f} out · max prompt "
        f"{max(r.max_prompt_tokens for r in results)} tokens · errors "
        f"{sum(r.error is not None for r in results)}",
    ]
    return "\n".join(lines)


async def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--chat-model", action="store_true", help="use CHAT_LLM_* from .env")
    parser.add_argument("--base-url", default="http://localhost:11434/v1")
    parser.add_argument("--model", default="qwen2.5:7b")
    parser.add_argument("--timeout", type=float, default=300)
    parser.add_argument("--label")
    parser.add_argument("--reasoning-effort", default="", choices=["", "low", "medium", "high"])
    parser.add_argument("--prompt", help="system prompt template ({profile}) to try instead")
    parser.add_argument("--only", help="run one case id")
    parser.add_argument("--repeat", type=int, default=3, help="runs per case (sampling varies)")
    parser.add_argument("--pace", type=float, default=0, help="seconds between cases (rate limits)")
    parser.add_argument("--no-seed", action="store_true", help="reuse the seeded database")
    args = parser.parse_args()
    use_eval_database()

    spec = yaml.safe_load((HERE / "cases.yaml").read_text(encoding="utf-8"))
    cases = [c for c in spec["cases"] if args.only in (None, c["id"])]
    embedder = get_embedder()
    if not args.no_seed:
        await load_fixtures(embedder, URL_PREFIX)
    cv = await seed_cv(spec["cv"], embedder)
    meter = UsageMeter()
    label, llm = make_llm(args, meter)

    system_template = SYSTEM
    if args.prompt:
        system_template = read_prompt(args.prompt)
        label += f" + {Path(args.prompt).stem}"
    results = []
    for _ in range(args.repeat):
        for case in cases:
            results.append(await run_case(case, llm, meter, embedder, cv, system_template))
            await asyncio.sleep(args.pace)
    out = HERE / "results" / f"{re.sub(r'[^\w.-]+', '_', label)}.json"
    out.parent.mkdir(exist_ok=True)
    out.write_text(
        json.dumps([asdict(r) for r in results], ensure_ascii=False, indent=1), encoding="utf-8"
    )
    print(report(label, results, args.repeat))


if __name__ == "__main__":
    asyncio.run(main())
