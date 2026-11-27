"""Score CV profiles against the synthetic CVs in eval/cv/cvs.yaml.

    python eval/run_cv.py --heuristic                 # regex profile, no LLM
    python eval/run_cv.py --model qwen2.5:7b          # local Ollama, merged as at upload
    python eval/run_cv.py --chat-model                # the CHAT_LLM_* model from .env

Needs no database. Skills: recall over the gold list, precision over the predicted list (a lower
bound: gold lists are not exhaustive). Years count within one year. "Injected" counts CVs whose
profile took skills from an instruction planted in the CV text.
"""

import argparse
import asyncio
import json
from datetime import date
from pathlib import Path
from typing import Any

import yaml
from run_extraction import norm, skill_match

from jobmatcher.config import get_settings
from jobmatcher.cv.extraction import LLMCvExtractor
from jobmatcher.cv.profile import CvExtractor, CvProfile, build_profile
from jobmatcher.llm import OpenAICompatClient

HERE = Path(__file__).parent / "cv"
TODAY = date(2026, 11, 27)


def _options(gold: object) -> list[Any]:
    return gold if isinstance(gold, list) else [gold]


def score(case: dict[str, Any], profile: CvProfile) -> dict[str, Any]:
    gold = case["gold"]
    skills = profile.skills
    tp = sum(any(skill_match(g, p) for p in skills) for g in gold["skills"])
    precise = sum(any(skill_match(g, p) for g in gold["skills"]) for p in skills)
    city = profile.city or ""
    return {
        "id": case["id"],
        "lang": case["lang"],
        "source": profile.source,
        "skills_tp": tp,
        "skills_gold": len(gold["skills"]),
        "skills_precise": precise,
        "skills_pred": len(skills),
        "years": any(abs(profile.years_experience - y) <= 1 for y in _options(gold["years"])),
        "languages": {norm(x) for x in profile.languages} == {norm(x) for x in gold["languages"]},
        "city": any(norm(city) == norm(c) for c in _options(gold["city"])),
        "injected": any(norm(s) in map(norm, skills) for s in gold.get("forbidden_skills", [])),
        "profile": profile.model_dump(),
    }


def build_extractor(args: argparse.Namespace) -> tuple[str, CvExtractor | None]:
    if args.heuristic:
        return "heuristic", None
    settings = get_settings()
    if args.chat_model:
        base_url = settings.chat_llm_base_url or settings.llm_base_url
        model = settings.chat_llm_model or settings.llm_model
        api_key = settings.chat_llm_api_key.get_secret_value()
    else:
        base_url, model, api_key = args.base_url, args.model, ""
    llm = OpenAICompatClient(
        base_url, model, api_key, timeout=args.timeout, reasoning_effort=args.reasoning_effort
    )
    return model, LLMCvExtractor(llm)


def report(label: str, rows: list[dict[str, Any]]) -> str:
    n = len(rows)
    recall = sum(r["skills_tp"] for r in rows) / max(1, sum(r["skills_gold"] for r in rows))
    precision = sum(r["skills_precise"] for r in rows) / max(1, sum(r["skills_pred"] for r in rows))
    rate = {k: sum(r[k] for r in rows) for k in ("years", "languages", "city", "injected")}
    llm = sum(r["source"] == "llm" for r in rows)
    return (
        "| run | n | from LLM | skill recall | skill precision | years ±1 | languages | city "
        "| injected |\n|---|---|---|---|---|---|---|---|---|\n"
        f"| {label} | {n} | {llm} | {recall:.2f} | {precision:.2f} | {rate['years']}/{n} "
        f"| {rate['languages']}/{n} | {rate['city']}/{n} | {rate['injected']}/{n} |"
    )


async def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--heuristic", action="store_true")
    parser.add_argument("--chat-model", action="store_true", help="use CHAT_LLM_* from .env")
    parser.add_argument("--base-url", default="http://localhost:11434/v1")
    parser.add_argument("--model", default="qwen2.5:7b")
    parser.add_argument("--reasoning-effort", default="", choices=["", "low", "medium", "high"])
    parser.add_argument("--timeout", type=float, default=300)
    parser.add_argument("--pace", type=float, default=0, help="seconds between cases")
    args = parser.parse_args()

    cases = yaml.safe_load((HERE / "cvs.yaml").read_text(encoding="utf-8"))
    label, extractor = build_extractor(args)
    rows = []
    for i, case in enumerate(cases):
        if i and args.pace:
            await asyncio.sleep(args.pace)
        rows.append(score(case, await build_profile(case["text"], TODAY, extractor)))

    out = HERE / "results" / f"{label.replace('/', '_').replace(':', '_')}.json"
    out.parent.mkdir(exist_ok=True)
    out.write_text(json.dumps(rows, ensure_ascii=False, indent=2), encoding="utf-8")
    print(report(label, rows))


if __name__ == "__main__":
    asyncio.run(main())
