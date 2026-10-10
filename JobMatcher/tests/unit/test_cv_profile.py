from datetime import date

import pytest

from jobmatcher.cv.profile import CvProfile, build_profile, compare

CV = """Martin Novak - Python Developer, Brno
Languages: Czech (native), English (C1)
Experience
Python Developer, Acme — 09/2024 – Present
Flask, FastAPI, SQLAlchemy, PostgreSQL, RabbitMQ, Docker, asyncio
Education
Master's degree, 09/2025 – 06/2027
"""


@pytest.mark.asyncio
async def test_build_profile() -> None:
    profile = await build_profile(CV, today=date(2026, 10, 1))
    assert {"python", "fastapi", "postgresql", "rabbitmq", "docker", "asyncio"} <= set(
        profile.skills
    )
    assert profile.years_experience == 2.1  # education range is not counted
    assert profile.languages == ["Czech", "English"]
    assert profile.city == "Brno"


def test_compare_splits_required_and_nice_to_have() -> None:
    profile = CvProfile(skills=["python", "postgresql", "docker"], years_experience=2, city="Brno")
    result = compare(
        profile,
        job_stack=["python", "postgresql", "kubernetes", "terraform"],
        requirements_text="3+ years of Python and PostgreSQL, Kubernetes",
        nice_to_have_text="Terraform is a plus",
        years_required=3,
        remote_policy="hybrid",
        office_city="Prague",
    )
    assert result.matched_skills == ["postgresql", "python"]
    assert result.missing_required == ["kubernetes"]
    assert result.missing_nice_to_have == ["terraform"]
    assert result.years_gap == 1
    assert result.location_fit == "relocation_or_commute"


def test_compare_remote_fits_anywhere() -> None:
    result = compare(CvProfile(), [], "", "", None, "remote_region", "Berlin")
    assert result.location_fit == "remote"
    assert result.years_gap == 0


def test_compare_prefers_extracted_skills_and_reads_cv_text() -> None:
    profile = CvProfile(skills=["python"], years_experience=2, city="Brno")
    result = compare(
        profile,
        job_stack=["python"],
        requirements_text="",
        nice_to_have_text="",
        years_required=None,
        remote_policy="hybrid",
        office_city="Brno, Czech Republic",
        required_skills=["Python", "Linux", "distributed systems"],
        nice_to_have_skills=["Go"],
        cv_text="Built distributed systems on Linux servers.",
    )
    assert result.matched_skills == ["distributed systems", "linux", "python"]
    assert result.missing_required == []
    assert result.missing_nice_to_have == ["go"]
    assert result.location_fit == "same_city"


def test_free_form_skills_match_cv_tools() -> None:
    profile = CvProfile(skills=["python", "asyncio", "fastapi", "pytest", "postgresql"])
    cv = "Built REST APIs with FastAPI, async workers, pytest, PostgreSQL partitioning."
    result = compare(
        profile, [], "", "", None, None, None,
        required_skills=["async programming", "rest apis", "testing", "postgres", "graphql"],
        cv_text=cv,
    )
    assert result.missing_required == ["graphql"]
