from jobmatcher.ingest.normalize import compact_for_llm

POSTING = """## About us
We are a fast growing company founded in 2010.
## Requirements
- 3+ years of Python
- PostgreSQL
- PostgreSQL
## What we offer
- Salary 80 000 - 100 000 Kč
- Multisport card
- Free snacks
## Responsibilities
- Build APIs
We are an equal opportunity employer.
"""


def test_drops_boilerplate_sections_but_keeps_salary() -> None:
    text = compact_for_llm(POSTING)
    assert "fast growing" not in text
    assert "Multisport" not in text
    assert "80 000 - 100 000 Kč" in text
    assert "- Build APIs" in text
    assert "equal opportunity" not in text


def test_removes_duplicate_lines() -> None:
    assert compact_for_llm(POSTING).count("PostgreSQL") == 1


def test_truncates_on_line_boundary() -> None:
    text = "\n".join(f"line {i}" for i in range(100))
    compact = compact_for_llm(text, limit=50)
    assert len(compact) <= 50
    assert all(line.removeprefix("line ").isdigit() for line in compact.splitlines())


def test_short_text_is_unchanged() -> None:
    assert compact_for_llm("## Requirements\n- Go") == "## Requirements\n- Go"
