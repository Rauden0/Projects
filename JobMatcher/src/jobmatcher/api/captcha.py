"""Optional Cloudflare Turnstile check on CV uploads (TURNSTILE_SECRET), the most expensive
anonymous request: an LLM extraction and an embedding per upload."""

import logging
from collections.abc import Callable

import httpx

from jobmatcher.errors import JobMatcherError

log = logging.getLogger(__name__)

VERIFY_URL = "https://challenges.cloudflare.com/turnstile/v0/siteverify"
ORIGIN = "https://challenges.cloudflare.com"
HEADER = "x-turnstile-token"
MAX_TOKEN_CHARS = 2048


class CaptchaRejected(JobMatcherError):
    """The request carries no Turnstile token or one Cloudflare does not accept."""


class CaptchaUnavailable(JobMatcherError):
    """Cloudflare's verification endpoint cannot be reached."""


class Turnstile:
    def __init__(
        self,
        secret: str,
        http: Callable[[], httpx.AsyncClient] = lambda: httpx.AsyncClient(timeout=5),
    ) -> None:
        self.secret = secret
        self.http = http

    async def verify(self, token: str, remote_ip: str | None) -> None:
        if not token or len(token) > MAX_TOKEN_CHARS:
            raise CaptchaRejected("complete the captcha first")
        form = {"secret": self.secret, "response": token}
        if remote_ip:
            form["remoteip"] = remote_ip
        try:
            async with self.http() as http:
                response = await http.post(VERIFY_URL, data=form)
                response.raise_for_status()
                result = response.json()
        except (httpx.HTTPError, ValueError) as exc:
            raise CaptchaUnavailable(f"captcha verification failed: {exc!r}") from exc
        if not isinstance(result, dict) or result.get("success") is not True:
            codes = result.get("error-codes") if isinstance(result, dict) else None
            log.info("captcha rejected: %s", codes)
            raise CaptchaRejected("captcha was not accepted, try again")
