from pathlib import Path

import yaml

from jobmatcher.search.metrics import hit_at_k, recall_at_k, reciprocal_rank

EVAL = Path(__file__).parents[2] / "eval"


def test_metrics() -> None:
    ranked = ["a", "b", "c", "d"]
    assert hit_at_k(ranked, {"c"}, 2) == 0.0
    assert hit_at_k(ranked, {"c"}, 3) == 1.0
    assert recall_at_k(ranked, {"a", "z"}, 10) == 0.5
    assert reciprocal_rank(ranked, {"b", "d"}) == 0.5
    assert reciprocal_rank(ranked, {"z"}) == 0.0


def test_golden_set_references_existing_fixtures() -> None:
    postings = yaml.safe_load((EVAL / "fixtures" / "postings.yaml").read_text("utf-8"))
    ids = {p["id"] for p in postings}
    golden = yaml.safe_load((EVAL / "golden.yaml").read_text("utf-8"))
    for case in golden:
        assert set(case["relevant"]) <= ids, case["query"]
