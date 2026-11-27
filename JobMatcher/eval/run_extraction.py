"""Score structured extraction against the labelled postings in eval/extraction/postings.yaml.

    python eval/run_extraction.py --heuristic                     # regex baseline, no LLM
    python eval/run_extraction.py --model qwen2.5:7b              # local Ollama
    python eval/run_extraction.py --model qwen2.5:7b --no-structured-output
    python eval/run_extraction.py --chat-model                    # the CHAT_LLM_* model from .env

Prints markdown tables and writes every prediction to eval/extraction/results/<label>.json.
Needs no database: the extractor is called directly with the production prompt and schema.
"""

import argparse
import asyncio
import json
import re
import statistics
import unicodedata
from dataclasses import asdict, dataclass, field
from pathlib import Path
from typing import Any

import yaml

from jobmatcher.config import get_settings
from jobmatcher.domain import JobFacts
from jobmatcher.errors import ExtractionError, LLMError
from jobmatcher.ingest.extraction import Extractor, HeuristicExtractor, LLMExtractor
from jobmatcher.llm import OpenAICompatClient
from jobmatcher.observability import Stopwatch, UsageMeter

HERE = Path(__file__).parent / "extraction"
FIELDS = ("seniority", "years", "remote", "city", "salary_min", "salary_max", "currency")


@dataclass
class CaseResult:
    id: str
    domain: str
    lang: str
    ok: bool
    attempts: int
    latency_s: float
    input_tokens: int
    output_tokens: int
    fields: dict[str, bool] = field(default_factory=dict)
    skills_tp: int = 0
    skills_pred: int = 0
    skills_gold: int = 0
    predicted: dict[str, Any] = field(default_factory=dict)


def norm(value: object) -> str:
    text = unicodedata.normalize("NFKD", str(value)).encode("ascii", "ignore").decode()
    return re.sub(r"\s+", " ", text.casefold()).strip()


def city_key(value: str) -> str:
    first = re.split(r",|\s[-–]\s|\(", norm(value))[0]
    return re.sub(r"\s*\d+$", "", first).strip()


def _same_city(predicted: str, gold: str) -> bool:
    """'Brno-Bohunice' or 'Brno – Slatina' count as Brno: the search filter matches by
    substring, so a district suffix does not change what the posting is found for."""
    pred, want = city_key(predicted), city_key(gold)
    return pred == want or re.match(rf"{re.escape(want)}", pred) is not None


def matches(name: str, predicted: object, gold: object) -> bool:
    options = gold if isinstance(gold, list) else [gold]
    for option in options:
        if option is None or predicted is None:
            if option is None and predicted in (None, ""):
                return True
            continue
        if name == "city" and _same_city(str(predicted), str(option)):
            return True
        if name == "currency" and norm(predicted).upper() == str(option).upper():
            return True
        if name not in ("city", "currency") and norm(predicted) == norm(option):
            return True
    return False


def skill_match(gold: str | list[str], predicted: str) -> bool:
    synonyms = gold if isinstance(gold, list) else [gold]
    return any(_same_skill(norm(s), norm(predicted)) for s in synonyms)


def _same_skill(a: str, b: str) -> bool:
    if a == b:
        return True
    short, long = sorted((a, b), key=len)
    return len(short) >= 3 and short in long


def predicted_fields(facts: JobFacts) -> dict[str, Any]:
    return {
        "seniority": facts.seniority,
        "years": facts.years_required_min,
        "remote": facts.remote_policy,
        "city": facts.office_city,
        "salary_min": facts.salary_min,
        "salary_max": facts.salary_max,
        "currency": facts.currency,
        "skills": sorted({*facts.tech_stack, *facts.required_skills, *facts.nice_to_have_skills}),
    }


def score(case: dict[str, Any], facts: JobFacts | None, result: CaseResult) -> None:
    gold = case["gold"]
    predicted = predicted_fields(facts or JobFacts())
    result.predicted = predicted
    result.fields = {name: matches(name, predicted[name], gold[name]) for name in FIELDS}
    gold_skills, pred_skills = gold["skills"], predicted["skills"]
    result.skills_gold = len(gold_skills)
    result.skills_pred = len(pred_skills)
    result.skills_tp = sum(any(skill_match(g, p) for p in pred_skills) for g in gold_skills)
    precise = sum(any(skill_match(g, p) for g in gold_skills) for p in pred_skills)
    result.predicted["_precise_skills"] = precise


def rescore(path: str, cases: list[dict[str, Any]]) -> list[CaseResult]:
    """Scores saved predictions against the current gold set (after gold or matcher changes)."""
    by_id = {c["id"]: c for c in cases}
    results = []
    for saved in json.loads(Path(path).read_text(encoding="utf-8")):
        p = saved["predicted"]
        facts = JobFacts(
            seniority=p["seniority"],
            years_required_min=p["years"],
            remote_policy=p["remote"],
            office_city=p["city"],
            salary_min=p["salary_min"],
            salary_max=p["salary_max"],
            currency=p["currency"],
            tech_stack=p["skills"],
        )
        result = CaseResult(
            **{k: saved[k] for k in ("id", "domain", "lang", "ok", "attempts", "latency_s")},
            input_tokens=saved["input_tokens"],
            output_tokens=saved["output_tokens"],
        )
        score(by_id[saved["id"]], facts, result)
        results.append(result)
    return results


async def run_case(extractor: Extractor, meter: UsageMeter, case: dict[str, Any]) -> CaseResult:
    meter.calls.clear()
    watch = Stopwatch()
    facts: JobFacts | None
    try:
        facts = await extractor.extract(case["title"], case["location"], case["text"])
        ok = True
    except (ExtractionError, LLMError):
        facts, ok = None, False
    result = CaseResult(
        id=case["id"],
        domain="it" if case["domain"] == "it" else "non-it",
        lang=case["lang"],
        ok=ok,
        attempts=len(meter.calls),
        latency_s=watch.elapsed_s,
        input_tokens=meter.input_tokens,
        output_tokens=meter.output_tokens,
    )
    score(case, facts, result)
    return result


def pct(values: list[bool]) -> str:
    return f"{100 * sum(values) / len(values):.0f}%" if values else "-"


def quantile(values: list[float], q: float) -> float:
    if len(values) < 2:
        return values[0] if values else 0.0
    return statistics.quantiles(values, n=100, method="inclusive")[int(q * 100) - 1]


def skills_prf(results: list[CaseResult]) -> tuple[float, float, float]:
    tp = sum(r.skills_tp for r in results)
    gold = sum(r.skills_gold for r in results)
    pred = sum(r.skills_pred for r in results)
    precise = sum(r.predicted["_precise_skills"] for r in results)
    recall = tp / gold if gold else 0.0
    precision = precise / pred if pred else 0.0
    f1 = 2 * precision * recall / (precision + recall) if precision + recall else 0.0
    return precision, recall, f1


def report(label: str, results: list[CaseResult], is_llm: bool) -> str:
    lines = [f"### {label}", ""]
    lines += ["| subset | n | " + " | ".join(FIELDS) + " | all fields | skills P / R / F1 |"]
    lines += ["|---" * (len(FIELDS) + 4) + "|"]
    subsets = {"all": results}
    for key in ("it", "non-it"):
        subsets[key] = [r for r in results if r.domain == key]
    for key in ("cs", "en"):
        subsets[key] = [r for r in results if r.lang == key]
    for name, subset in subsets.items():
        accuracy = [pct([r.fields[f] for r in subset]) for f in FIELDS]
        exact = pct([all(r.fields.values()) for r in subset])
        p, r_, f1 = skills_prf(subset)
        skills = f"{p:.2f} / {r_:.2f} / {f1:.2f}" if any(r.skills_pred for r in subset) else "none"
        lines.append(
            f"| {name} | {len(subset)} | " + " | ".join(accuracy) + f" | {exact} | {skills} |"
        )
    field_mean = statistics.mean(sum(r.fields.values()) / len(FIELDS) for r in results)
    lines += ["", f"Mean field accuracy: **{100 * field_mean:.1f}%**"]
    if is_llm:
        latencies = [r.latency_s for r in results]
        first_try = [r.ok and r.attempts == 1 for r in results]
        lines += [
            "",
            "| valid JSON 1st try | valid after retry | latency p50 | latency p95 "
            "| tokens in (avg) | tokens out (avg) |",
            "|---|---|---|---|---|---|",
            f"| {pct(first_try)} | {pct([r.ok for r in results])} "
            f"| {quantile(latencies, 0.5):.1f}s | {quantile(latencies, 0.95):.1f}s "
            f"| {statistics.mean(r.input_tokens for r in results):.0f} "
            f"| {statistics.mean(r.output_tokens for r in results):.0f} |",
        ]
    return "\n".join(lines)


def build_extractor(args: argparse.Namespace, meter: UsageMeter) -> tuple[str, Extractor, bool]:
    if args.heuristic:
        return "heuristic", HeuristicExtractor(), False
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
        timeout=args.timeout,
        structured_output=not args.no_structured_output,
        reasoning_effort=args.reasoning_effort,
        meter=meter,
    )
    suffix = "" if not args.no_structured_output else " (schema in prompt only)"
    if args.prompt:
        suffix += f" + {Path(args.prompt).stem}"
        system = Path(args.prompt).read_text(encoding="utf-8").strip()
        return args.label or f"{model}{suffix}", LLMExtractor(llm, system), True
    return args.label or f"{model}{suffix}", LLMExtractor(llm), True


async def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--heuristic", action="store_true")
    parser.add_argument("--chat-model", action="store_true", help="use CHAT_LLM_* from .env")
    parser.add_argument("--base-url", default="http://localhost:11434/v1")
    parser.add_argument("--model", default="qwen2.5:7b")
    parser.add_argument("--no-structured-output", action="store_true")
    parser.add_argument("--reasoning-effort", default="", choices=["", "low", "medium", "high"])
    parser.add_argument("--timeout", type=float, default=300)
    parser.add_argument("--prompt", help="file with a system prompt to try instead of the default")
    parser.add_argument("--label")
    parser.add_argument("--limit", type=int)
    parser.add_argument("--rescore", help="re-score a saved results file, no model calls")
    parser.add_argument("--pace", type=float, default=0, help="seconds between cases (rate limits)")
    args = parser.parse_args()

    cases = yaml.safe_load((HERE / "postings.yaml").read_text(encoding="utf-8"))[: args.limit]
    if args.rescore:
        print(report(args.label or Path(args.rescore).stem, rescore(args.rescore, cases), True))
        return
    meter = UsageMeter()
    label, extractor, is_llm = build_extractor(args, meter)
    if is_llm:
        await run_case(extractor, meter, cases[0])  # warm-up: loads the model into memory
    results = []
    for case in cases:
        results.append(await run_case(extractor, meter, case))
        await asyncio.sleep(args.pace)

    out = HERE / "results" / f"{re.sub(r'[^\w.-]+', '_', label)}.json"
    out.parent.mkdir(exist_ok=True)
    out.write_text(
        json.dumps([asdict(r) for r in results], ensure_ascii=False, indent=1), encoding="utf-8"
    )
    print(report(label, results, is_llm))


if __name__ == "__main__":
    asyncio.run(main())
