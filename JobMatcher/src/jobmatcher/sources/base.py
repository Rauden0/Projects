import re
from abc import ABC, abstractmethod
from collections.abc import AsyncIterator
from pathlib import Path

import httpx
import yaml
from pydantic import BaseModel


class SourceConfig(BaseModel):
    kind: str
    company: str
    location_filter: str | None = None


class PostingRef(BaseModel):
    """Enough to identify a posting and fetch its detail later."""

    kind: str
    company: str
    external_id: str
    url: str


class RawPosting(BaseModel):
    kind: str
    company: str
    external_id: str
    url: str
    title: str
    location: str | None
    description_html: str


class SourceAdapter(ABC):
    kind: str

    def __init__(self, config: SourceConfig, client: httpx.AsyncClient) -> None:
        self.config = config
        self.client = client
        self._location_re = (
            re.compile(config.location_filter, re.I) if config.location_filter else None
        )

    def wants_location(self, location: str | None) -> bool:
        if self._location_re is None:
            return True
        return bool(location and self._location_re.search(location))

    @abstractmethod
    def list_postings(self) -> AsyncIterator[RawPosting]:
        """Yield every posting matching the location filter, with full description."""


def load_sources(path: str | Path) -> list[SourceConfig]:
    data = yaml.safe_load(Path(path).read_text(encoding="utf-8"))
    return [SourceConfig.model_validate(s) for s in data["sources"]]
