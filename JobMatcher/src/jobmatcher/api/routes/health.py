import logging

import aio_pika
from aio_pika.exceptions import AMQPException
from fastapi import APIRouter, Response, status
from sqlalchemy import text
from sqlalchemy.exc import SQLAlchemyError

from jobmatcher.config import get_settings
from jobmatcher.db.session import get_engine

log = logging.getLogger(__name__)

router = APIRouter(tags=["health"])


@router.get("/healthz")
async def healthz() -> dict[str, str]:
    return {"status": "ok"}


@router.get("/readyz")
async def readyz(response: Response) -> dict[str, str]:
    checks = {"database": "ok", "broker": "ok"}

    try:
        async with get_engine().connect() as conn:
            await conn.execute(text("SELECT 1"))
    except (SQLAlchemyError, OSError) as exc:
        log.warning("readiness: database unavailable: %s", exc)
        checks["database"] = "unavailable"

    try:
        connection = await aio_pika.connect(get_settings().amqp_url, timeout=3)
        await connection.close()
    except (AMQPException, OSError, TimeoutError) as exc:
        log.warning("readiness: broker unavailable: %s", exc)
        checks["broker"] = "unavailable"

    if any(v != "ok" for v in checks.values()):
        response.status_code = status.HTTP_503_SERVICE_UNAVAILABLE
    return checks
