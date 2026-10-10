from jobmatcher.search.hybrid import SearchFilters, _filter_sql, _or_tsquery


def test_or_tsquery_dedupes_and_strips_punctuation() -> None:
    assert _or_tsquery("Python, remote! python backend") == "python | remote | backend"
    assert _or_tsquery("!!!") == "''"


def test_filter_sql_binds_every_value() -> None:
    params: dict[str, object] = {}
    sql = _filter_sql(
        SearchFilters(
            remote_policy=["remote_region"],
            city="Brno",
            max_years_required=3,
            seniority=["medior"],
            tech=["Python"],
        ),
        params,
    )
    assert "Brno" not in sql  # values are bound, never interpolated
    assert params == {
        "remote_policy": ["remote_region"],
        "city": "Brno",
        "max_years": 3,
        "seniority": ["medior"],
        "tech": ["python"],
    }


def test_no_filters_only_open_jobs() -> None:
    assert _filter_sql(SearchFilters(), {}) == "j.status = 'open'"
