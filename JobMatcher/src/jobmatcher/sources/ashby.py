from collections.abc import AsyncIterator

from jobmatcher.sources.base import RawPosting, SourceAdapter


class AshbyAdapter(SourceAdapter):
    kind = "ashby"

    async def list_postings(self) -> AsyncIterator[RawPosting]:
        url = f"https://api.ashbyhq.com/posting-api/job-board/{self.config.company}"
        response = await self.client.get(url)
        response.raise_for_status()

        for job in response.json().get("jobs", []):
            location = job.get("location")
            if job.get("isRemote"):
                location = f"{location or ''} (remote)".strip()
            if not self.wants_location(location):
                continue
            yield RawPosting(
                kind=self.kind,
                company=self.config.company,
                external_id=job["id"],
                url=job["jobUrl"],
                title=job["title"],
                location=location,
                description_html=job.get("descriptionHtml") or "",
            )
