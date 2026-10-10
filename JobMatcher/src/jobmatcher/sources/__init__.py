from jobmatcher.sources.ashby import AshbyAdapter
from jobmatcher.sources.base import PostingRef, RawPosting, SourceAdapter, SourceConfig
from jobmatcher.sources.greenhouse import GreenhouseAdapter
from jobmatcher.sources.lever import LeverAdapter

ADAPTERS: dict[str, type[SourceAdapter]] = {
    "greenhouse": GreenhouseAdapter,
    "lever": LeverAdapter,
    "ashby": AshbyAdapter,
}

__all__ = ["ADAPTERS", "PostingRef", "RawPosting", "SourceAdapter", "SourceConfig"]
