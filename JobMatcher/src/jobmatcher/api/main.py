from pathlib import Path

from fastapi import FastAPI
from fastapi.responses import FileResponse

from jobmatcher.api.routes import chat, health, jobs

STATIC = Path(__file__).parent / "static"


def create_app() -> FastAPI:
    app = FastAPI(title="Job Matcher", version="0.1.0")
    app.include_router(health.router)
    app.include_router(jobs.router)
    app.include_router(chat.router)

    @app.get("/", include_in_schema=False)
    async def index() -> FileResponse:
        return FileResponse(STATIC / "index.html")

    return app


app = create_app()
