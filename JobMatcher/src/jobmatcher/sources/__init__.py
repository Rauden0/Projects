from jobmatcher.sources.ashby import AshbyAdapter
from jobmatcher.sources.base import PostingRef, RawPosting, SourceAdapter, SourceConfig
from jobmatcher.sources.greenhouse import GreenhouseAdapter
from jobmatcher.sources.jobscz import JobsCzAdapter
from jobmatcher.sources.lever import LeverAdapter
from jobmatcher.sources.workday import WorkdayAdapter

ADAPTERS: dict[str, type[SourceAdapter]] = {
    "greenhouse": GreenhouseAdapter,
    "lever": LeverAdapter,
    "ashby": AshbyAdapter,
    "workday": WorkdayAdapter,
    "jobscz": JobsCzAdapter,
}

__all__ = [
    "ADAPTERS",
    "AshbyAdapter",
    "GreenhouseAdapter",
    "JobsCzAdapter",
    "LeverAdapter",
    "PostingRef",
    "RawPosting",
    "SourceAdapter",
    "SourceConfig",
    "WorkdayAdapter",
]
