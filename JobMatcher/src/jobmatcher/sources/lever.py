from collections.abc import AsyncIterator

from jobmatcher.sources.base import RawPosting, SourceAdapter


class LeverAdapter(SourceAdapter):
    kind = "lever"

    async def list_postings(self) -> AsyncIterator[RawPosting]:
        url = f"https://api.lever.co/v0/postings/{self.config.company}"
        response = await self.client.get(url, params={"mode": "json"})
        response.raise_for_status()

        for job in response.json():
            categories = job.get("categories") or {}
            location = categories.get("location")
            if job.get("workplaceType") == "remote":
                location = f"{location or ''} (remote)".strip()
            if not self.wants_location(location):
                continue

            sections = [job.get("description") or ""]
            for block in job.get("lists") or []:
                heading, items = block.get("text", ""), block.get("content", "")
                sections.append(f"<h3>{heading}</h3><ul>{items}</ul>")
            sections.append(job.get("additional") or "")

            yield RawPosting(
                kind=self.kind,
                company=self.config.company,
                external_id=job["id"],
                url=job["hostedUrl"],
                title=job["text"],
                location=location,
                description_html="\n".join(sections),
            )
