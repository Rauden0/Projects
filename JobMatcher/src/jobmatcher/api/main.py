from fastapi import FastAPI

from jobmatcher.api.routes import chat, health, jobs


def create_app() -> FastAPI:
    app = FastAPI(title="Job Matcher", version="0.1.0")
    app.include_router(health.router)
    app.include_router(jobs.router)
    app.include_router(chat.router)
    return app


app = create_app()
