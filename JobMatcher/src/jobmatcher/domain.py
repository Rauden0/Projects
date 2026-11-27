from dataclasses import dataclass
from typing import Literal

from pydantic import BaseModel, Field

Seniority = Literal["junior", "medior", "senior", "staff"]
RemotePolicy = Literal["onsite", "hybrid", "remote_country", "remote_region", "remote_global"]
ContractType = Literal["employment", "contract", "freelance"]
ExtractionStatus = Literal["ok", "pending", "fallback", "batch_failed"]


class RawPosting(BaseModel):
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


class JobFacts(BaseModel):
    seniority: Seniority | None = None
    years_required_min: int | None = Field(default=None, ge=0, le=30)
    remote_policy: RemotePolicy | None = None
    office_city: str | None = None
    salary_min: int | None = Field(default=None, ge=0)
    salary_max: int | None = Field(default=None, ge=0)
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
