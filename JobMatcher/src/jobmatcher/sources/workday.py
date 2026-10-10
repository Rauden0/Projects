from collections.abc import AsyncIterator

from jobmatcher.sources.base import RawPosting, SourceAdapter

PAGE_SIZE = 20
MAX_POSTINGS = 200


class WorkdayAdapter(SourceAdapter):
    """Workday career sites (``{tenant}.wd{n}.myworkdayjobs.com/{site}``).

    Listing and detail are separate calls. Large employers have thousands of postings, so the
    listing is narrowed by ``search_text`` and capped; ``location_filter`` is applied to the
    listing's location text before any detail is fetched.
    """

    kind = "workday"

    async def list_postings(self) -> AsyncIterator[RawPosting]:
        cfg = self.config
        if not (cfg.host and cfg.site):
            raise ValueError(f"workday source {cfg.company!r} needs host and site")
        api = f"https://{cfg.host}/wday/cxs/{cfg.company}/{cfg.site}"

        seen = 0
        for offset in range(0, MAX_POSTINGS, PAGE_SIZE):
            response = await self.client.post(
                f"{api}/jobs",
                json={
                    "appliedFacets": {},
                    "limit": PAGE_SIZE,
                    "offset": offset,
                    "searchText": cfg.search_text or "",
                },
            )
            response.raise_for_status()
            postings = response.json().get("jobPostings", [])
            for item in postings:
                if not self.wants_location(item.get("locationsText")):
                    continue
                detail = await self.client.get(f"{api}{item['externalPath']}")
                detail.raise_for_status()
                info = detail.json()["jobPostingInfo"]
                seen += 1
                yield RawPosting(
                    kind=self.kind,
                    company=cfg.company,
                    external_id=info.get("jobReqId") or item["externalPath"],
                    url=info.get("externalUrl")
                    or f"https://{cfg.host}/{cfg.site}{item['externalPath']}",
                    title=info["title"],
                    location=info.get("location") or item.get("locationsText"),
                    description_html=info.get("jobDescription") or "",
                )
            if len(postings) < PAGE_SIZE:
                break
