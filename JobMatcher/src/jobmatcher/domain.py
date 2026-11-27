from dataclasses import dataclass
from datetime import UTC, datetime
from typing import Literal

from pydantic import BaseModel, Field, field_validator

Seniority = Literal["junior", "medior", "senior", "staff"]
RemotePolicy = Literal["onsite", "hybrid", "remote_country", "remote_region", "remote_global"]
ContractType = Literal["employment", "contract", "freelance"]
ExtractionStatus = Literal["ok", "pending", "fallback", "batch_failed"]
MAX_YEARS_REQUIRED = 30
MAX_SALARY = 2_147_483_647
CONTRACT_VERSION = 1


class InboundEvent(BaseModel):
    """Fields every inbound event may carry. All optional, so producers that predate them
    keep working; unknown fields are ignored so producers can add fields first."""

    schema_version: int = Field(default=CONTRACT_VERSION, ge=1)
    event_id: str | None = Field(default=None, max_length=200)
    crawled_at: datetime | None = Field(
        default=None, description="When JobIngestor fetched the data; orders stale redeliveries"
    )

    @field_validator("crawled_at")
    @classmethod
    def _assume_utc(cls, value: datetime | None) -> datetime | None:
        if value is not None and value.tzinfo is None:
            return value.replace(tzinfo=UTC)
        return value


class RawPosting(InboundEvent):
    """Body of a `job.discovered` message, as published by JobIngestor."""

    kind: str
    company: str
    # Source this posting was crawled from, when it differs from the employer (job boards
    # list many employers under one configured source). Defaults to ``company``.
    source: str | None = None
    external_id: str
    url: str
    title: str
    location: str | None
    description_html: str


class SourceCrawled(InboundEvent):
    """Body of a `source.crawled` message: the external ids one crawl of a source listed."""

    kind: str
    source: str
    external_ids: list[str]
    complete: bool


EVENT_VERSION = 1
JOB_CREATED = "job.created"
JOB_UPDATED = "job.updated"
JOB_CLOSED = "job.closed"


class JobChanged(BaseModel):
    """`data` of job.created / job.updated: the job is searchable in its new version."""

    job_id: int
    kind: str
    source: str
    external_id: str
    url: str


class JobClosed(BaseModel):
    """`data` of job.closed: the job left its source's listing."""

    job_id: int
    external_id: str


class EventEnvelope[DataT](BaseModel):
    """CloudEvents-like wrapper of every event JobMatcher publishes. ``event_id`` is stable
    across redeliveries, so consumers deduplicate on it."""

    event_id: str
    type: str
    version: int = EVENT_VERSION
    occurred_at: datetime
    data: DataT


class JobFacts(BaseModel):
    seniority: Seniority | None = None
    years_required_min: int | None = Field(default=None, ge=0, le=MAX_YEARS_REQUIRED)
    remote_policy: RemotePolicy | None = None
    office_city: str | None = None
    salary_min: int | None = Field(default=None, ge=0, le=MAX_SALARY)
    salary_max: int | None = Field(default=None, ge=0, le=MAX_SALARY)
    currency: str | None = Field(default=None, min_length=3, max_length=3)
    contract_type: ContractType | None = None
    tech_stack: list[str] = Field(default_factory=list)
    required_skills: list[str] = Field(default_factory=list)
    nice_to_have_skills: list[str] = Field(default_factory=list)
    languages_required: list[str] = Field(default_factory=list)


@dataclass(frozen=True, slots=True)
class Chunk:
    ord: int
    section: str | None
    content: str
