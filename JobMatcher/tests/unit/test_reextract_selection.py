from sqlalchemy.dialects import postgresql

from jobmatcher.ingest.reextract import selection


def sql(include_empty: bool, include_outdated: bool) -> str:
    return str(selection(include_empty, include_outdated).compile(dialect=postgresql.dialect()))


def test_default_selects_only_failed_extractions() -> None:
    assert "extraction_version" not in sql(False, False)


def test_outdated_selects_older_and_unversioned_extractions() -> None:
    clause = sql(False, True)
    assert "job.extraction_version IS NULL" in clause
    assert "job.extraction_version <" in clause
