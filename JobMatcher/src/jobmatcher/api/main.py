from pathlib import Path

from fastapi import FastAPI
from fastapi.middleware.gzip import GZipMiddleware
from fastapi.responses import FileResponse

from jobmatcher.api.errors import install_error_handlers
from jobmatcher.api.middleware import RequestIdMiddleware
from jobmatcher.api.routes import chat, health, jobs
from jobmatcher.config import configure_logging, configure_tracing

STATIC = Path(__file__).parent / "static"


def create_app() -> FastAPI:
    configure_logging()
    configure_tracing()
    app = FastAPI(title="Job Matcher", version="0.1.0")
    app.add_middleware(GZipMiddleware, minimum_size=1024)
    app.add_middleware(RequestIdMiddleware)
    install_error_handlers(app)
    app.include_router(health.router)
    app.include_router(jobs.router)
    app.include_router(chat.router)

    @app.get("/", include_in_schema=False)
    async def index() -> FileResponse:
        return FileResponse(STATIC / "index.html")

    return app


app = create_app()
