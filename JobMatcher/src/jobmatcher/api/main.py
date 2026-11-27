import base64
import hashlib
import re
from pathlib import Path

from fastapi import FastAPI
from fastapi.middleware.gzip import GZipMiddleware
from fastapi.responses import HTMLResponse

from jobmatcher import metrics
from jobmatcher.api.captcha import ORIGIN as CAPTCHA_ORIGIN
from jobmatcher.api.deps import get_authenticator
from jobmatcher.api.errors import install_error_handlers
from jobmatcher.api.middleware import (
    BodyLimitMiddleware,
    RequestIdMiddleware,
    SecurityHeadersMiddleware,
    SessionCookieMiddleware,
)
from jobmatcher.api.routes import chat, health, jobs
from jobmatcher.config import configure_logging, configure_tracing, get_settings

STATIC = Path(__file__).parent / "static"
CDN_SCRIPTS = (
    "https://cdn.jsdelivr.net/npm/marked@12.0.2/marked.min.js",
    "https://cdn.jsdelivr.net/npm/dompurify@3.4.17/dist/purify.min.js",
)


def _inline_hashes(html: str, tag: str) -> str:
    blocks = re.findall(rf"<{tag}>(.*?)</{tag}>", html, flags=re.DOTALL)
    digests = (base64.b64encode(hashlib.sha256(b.encode()).digest()).decode() for b in blocks)
    return " ".join(f"'sha256-{d}'" for d in digests)


SITE_KEY_META = '<meta name="turnstile-site-key" content="">'


def page_csp(html: str, captcha: bool = False) -> str:
    scripts = [*CDN_SCRIPTS, CAPTCHA_ORIGIN] if captcha else list(CDN_SCRIPTS)
    directives = [
        "default-src 'none'",
        f"script-src {' '.join(scripts)} {_inline_hashes(html, 'script')}",
        f"style-src {_inline_hashes(html, 'style')}",
        "connect-src 'self'",
        "img-src 'self' data:",
        "base-uri 'none'",
        "form-action 'self'",
        "frame-ancestors 'none'",
    ]
    if captcha:
        directives.append(f"frame-src {CAPTCHA_ORIGIN}")
    return "; ".join(directives)


def index_page(site_key: str) -> str:
    """The page, with the Turnstile site key for the widget; config allows only [A-Za-z0-9_-]."""
    html = (STATIC / "index.html").read_text(encoding="utf-8")
    if not site_key:
        return html
    return html.replace(SITE_KEY_META, SITE_KEY_META.replace('content=""', f'content="{site_key}"'))


def create_app() -> FastAPI:
    configure_logging()
    configure_tracing()
    get_authenticator()
    site_key = get_settings().turnstile_site_key
    page = index_page(site_key)
    index_csp = page_csp(page, captcha=bool(site_key))
    app = FastAPI(title="Job Matcher", version="0.1.0")
    app.add_middleware(GZipMiddleware, minimum_size=1024)
    app.add_middleware(BodyLimitMiddleware, max_bytes=get_settings().max_request_body_bytes)
    app.add_middleware(SessionCookieMiddleware)
    app.add_middleware(SecurityHeadersMiddleware)
    app.add_middleware(RequestIdMiddleware)
    install_error_handlers(app)
    app.include_router(health.router)
    app.include_router(jobs.router)
    app.include_router(chat.router)
    metrics.instrument(app)

    @app.get("/", include_in_schema=False)
    async def index() -> HTMLResponse:
        return HTMLResponse(page, headers={"Content-Security-Policy": index_csp})

    return app


app = create_app()
