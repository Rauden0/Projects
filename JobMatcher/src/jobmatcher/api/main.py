import base64
import hashlib
import re
from pathlib import Path

from fastapi import FastAPI
from fastapi.middleware.gzip import GZipMiddleware
from fastapi.responses import FileResponse

from jobmatcher import metrics
from jobmatcher.api.deps import get_authenticator
from jobmatcher.api.errors import install_error_handlers
from jobmatcher.api.middleware import (
    RequestIdMiddleware,
    SecurityHeadersMiddleware,
    SessionCookieMiddleware,
)
from jobmatcher.api.routes import chat, health, jobs
from jobmatcher.config import configure_logging, configure_tracing

STATIC = Path(__file__).parent / "static"
CDN_SCRIPTS = (
    "https://cdn.jsdelivr.net/npm/marked@12.0.2/marked.min.js",
    "https://cdn.jsdelivr.net/npm/dompurify@3.4.17/dist/purify.min.js",
)


def _inline_hashes(html: str, tag: str) -> str:
    blocks = re.findall(rf"<{tag}>(.*?)</{tag}>", html, flags=re.DOTALL)
    digests = (base64.b64encode(hashlib.sha256(b.encode()).digest()).decode() for b in blocks)
    return " ".join(f"'sha256-{d}'" for d in digests)


def page_csp(html: str) -> str:
    return "; ".join(
        (
            "default-src 'none'",
            f"script-src {' '.join(CDN_SCRIPTS)} {_inline_hashes(html, 'script')}",
            f"style-src {_inline_hashes(html, 'style')}",
            "connect-src 'self'",
            "img-src 'self' data:",
            "base-uri 'none'",
            "form-action 'self'",
            "frame-ancestors 'none'",
        )
    )


def create_app() -> FastAPI:
    configure_logging()
    configure_tracing()
    get_authenticator()
    index_csp = page_csp((STATIC / "index.html").read_text(encoding="utf-8"))
    app = FastAPI(title="Job Matcher", version="0.1.0")
    app.add_middleware(GZipMiddleware, minimum_size=1024)
    app.add_middleware(SessionCookieMiddleware)
    app.add_middleware(SecurityHeadersMiddleware)
    app.add_middleware(RequestIdMiddleware)
    install_error_handlers(app)
    app.include_router(health.router)
    app.include_router(jobs.router)
    app.include_router(chat.router)
    metrics.instrument(app)

    @app.get("/", include_in_schema=False)
    async def index() -> FileResponse:
        return FileResponse(STATIC / "index.html", headers={"Content-Security-Policy": index_csp})

    return app


app = create_app()
