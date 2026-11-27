from jobmatcher.domain import Chunk, JobFacts
from jobmatcher.ingest.chunking import chunk_posting, profile_chunk
from jobmatcher.ingest.normalize import content_hash, html_to_text

HTML = """
<p>We build developer tools.</p>
<h3>What you'll do</h3><ul><li>Build APIs</li><li>Own services</li></ul>
<h3>Requirements</h3><ul><li>3+ years of Python</li><li>PostgreSQL</li></ul>
<h3>Benefits</h3><p>Remote&nbsp;work</p>
"""


def test_html_to_text_keeps_structure() -> None:
    text = html_to_text(HTML)
    assert "## Requirements" in text
    assert "- 3+ years of Python" in text
    assert "Remote work" in text
    assert "\n\n\n" not in text


def test_content_hash_ignores_whitespace_and_case() -> None:
    a = content_hash("Dev", "Brno", "Python  rocks")
    assert a == content_hash("dev", "brno", "python rocks")
    assert content_hash("Dev", "Brno", "Python") != content_hash("Dev", "Prague", "Python")


def test_chunks_are_labelled_by_section() -> None:
    chunks = chunk_posting("Python Engineer", "Acme", html_to_text(HTML))
    sections = [c.section for c in chunks]
    assert sections == ["about", "responsibilities", "requirements", "benefits"]
    assert all(c.content.startswith("Python Engineer — Acme\n") for c in chunks)
    assert [c.ord for c in chunks] == list(range(len(chunks)))


def test_long_section_is_split() -> None:
    text = "## Requirements\n" + " ".join(["python"] * 800)
    chunks = chunk_posting("T", "C", text)
    assert len(chunks) == 3
    assert {c.section for c in chunks} == {"requirements"}


def test_empty_posting_still_has_one_chunk() -> None:
    assert len(chunk_posting("T", "C", "")) == 1


BOILERPLATE = """
<h3>Requirements</h3><ul><li>Python</li></ul>
<h3>What we offer</h3><ul><li>Free coffee</li><li>Salary 80 000 Kč</li><li>Home office</li></ul>
<h3>About us</h3><p>We have been great since 1990.</p>
<h3>Your profile:</h3><p>Kafka in production</p>
<p>Your personal data are processed according to GDPR.</p>
"""


def test_boilerplate_is_not_chunked_except_pay_and_work_mode() -> None:
    chunks = chunk_posting("T", "C", html_to_text(BOILERPLATE))
    text = "\n".join(c.content for c in chunks)
    assert "coffee" not in text and "1990" not in text and "GDPR" not in text
    assert "80 000 Kč" in text and "Home office" in text
    assert "Kafka in production" in text


def test_profile_chunk_names_seniority_and_skills_once() -> None:
    facts = JobFacts(
        seniority="senior",
        remote_policy="hybrid",
        tech_stack=["python", "kafka"],
        required_skills=["python", "sql"],
    )
    chunk = profile_chunk("Backend Engineer", "Acme", facts, ord=3)
    assert chunk == Chunk(
        3, "profile", "Backend Engineer — Acme\nsenior, hybrid\npython, sql, kafka"
    )


def test_no_profile_chunk_without_facts() -> None:
    assert profile_chunk("T", "C", JobFacts(), ord=0) is None
