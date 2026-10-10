from datetime import datetime

from pydantic import BaseModel, ConfigDict


class JobDetail(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: int
    title: str
    company: str
    url: str
    location_raw: str | None
    description: str
    seniority: str | None
    years_required_min: int | None
    remote_policy: str | None
    office_city: str | None
    salary_min: int | None
    salary_max: int | None
    currency: str | None
    contract_type: str | None
    tech_stack: list[str]
    status: str
    first_seen_at: datetime
    last_seen_at: datetime
