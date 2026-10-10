import html
from collections.abc import AsyncIterator

from jobmatcher.sources.base import RawPosting, SourceAdapter


class GreenhouseAdapter(SourceAdapter):
    kind = "greenhouse"

    async def list_postings(self) -> AsyncIterator[RawPosting]:
        url = f"https://boards-api.greenhouse.io/v1/boards/{self.config.company}/jobs"
        response = await self.client.get(url, params={"content": "true"})
        response.raise_for_status()

        for job in response.json().get("jobs", []):
            location = (job.get("location") or {}).get("name")
            if not self.wants_location(location):
                continue
            yield RawPosting(
                kind=self.kind,
                company=self.config.company,
                external_id=str(job["id"]),
                url=job["absolute_url"],
                title=job["title"],
                location=location,
                # Greenhouse returns the description HTML-escaped.
                description_html=html.unescape(job.get("content") or ""),
            )
