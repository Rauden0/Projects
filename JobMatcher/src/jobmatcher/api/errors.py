"""One JSON error shape for every failure: {"detail", "code", "request_id"[, "errors"]}.

`detail` stays a human-readable string (FastAPI's key, which clients already read); `code` is
stable and machine-readable.
"""

import logging
from typing import Any

from fastapi import FastAPI, Request
from fastapi.exceptions import RequestValidationError
from fastapi.responses import JSONResponse
from pydantic import BaseModel
from starlette.exceptions import HTTPException as StarletteHTTPException

from jobmatcher.errors import (
    AuthUnavailable,
    BudgetExceeded,
    ClientLimited,
    CsrfRejected,
    JobMatcherError,
    LLMError,
    LLMUnavailable,
    NotFound,
    Unauthenticated,
)
from jobmatcher.observability import request_id_var

log = logging.getLogger(__name__)


class ErrorOut(BaseModel):
    detail: str
    code: str
    request_id: str | None = None
    errors: list[dict[str, Any]] | None = None


_DOMAIN_ERRORS: dict[type[JobMatcherError], tuple[int, str]] = {
    NotFound: (404, "not_found"),
    Unauthenticated: (401, "unauthenticated"),
    CsrfRejected: (403, "csrf_rejected"),
    AuthUnavailable: (503, "auth_unavailable"),
    ClientLimited: (429, "rate_limited"),
    BudgetExceeded: (503, "budget_exhausted"),
    LLMUnavailable: (503, "llm_unavailable"),
    LLMError: (502, "llm_error"),
}

ERROR_RESPONSES: dict[int | str, dict[str, Any]] = {
    status: {"model": ErrorOut} for status in (401, 403, 404, 422, 429, 500, 502, 503)
}


def error_response(
    status: int,
    code: str,
    detail: str,
    headers: dict[str, str] | None = None,
    errors: list[dict[str, Any]] | None = None,
) -> JSONResponse:
    body = ErrorOut(detail=detail, code=code, request_id=request_id_var.get(), errors=errors)
    return JSONResponse(body.model_dump(exclude_none=True), status_code=status, headers=headers)


def _status_and_code(exc: JobMatcherError) -> tuple[int, str]:
    for cls in type(exc).__mro__:
        if cls in _DOMAIN_ERRORS:
            return _DOMAIN_ERRORS[cls]
    return 500, "internal_error"


def _headers(exc: JobMatcherError) -> dict[str, str] | None:
    if isinstance(exc, ClientLimited):
        return {"Retry-After": str(exc.retry_after_s)}
    if isinstance(exc, Unauthenticated):
        return {"WWW-Authenticate": "Bearer"}
    return None


async def _domain_error(_: Request, exc: Exception) -> JSONResponse:
    assert isinstance(exc, JobMatcherError)
    status, code = _status_and_code(exc)
    headers = _headers(exc)
    detail = "monthly LLM budget exhausted" if isinstance(exc, BudgetExceeded) else str(exc)
    if status >= 500:
        log.warning("%s: %s", code, exc)
    return error_response(status, code, detail, headers)


async def _http_error(_: Request, exc: Exception) -> JSONResponse:
    assert isinstance(exc, StarletteHTTPException)
    code = "not_found" if exc.status_code == 404 else f"http_{exc.status_code}"
    return error_response(exc.status_code, code, str(exc.detail), dict(exc.headers or {}))


async def _validation_error(_: Request, exc: Exception) -> JSONResponse:
    assert isinstance(exc, RequestValidationError)
    errors = [
        {"loc": list(e.get("loc", ())), "msg": e.get("msg", ""), "type": e.get("type", "")}
        for e in exc.errors()
    ]
    first = errors[0] if errors else {"loc": [], "msg": "invalid request"}
    detail = f"{'.'.join(str(p) for p in first['loc'])}: {first['msg']}".lstrip(": ")
    return error_response(422, "validation_error", detail, errors=errors)


async def _unhandled(_: Request, exc: Exception) -> JSONResponse:
    log.error("unhandled error", exc_info=exc)
    return error_response(500, "internal_error", "internal server error")


def install_error_handlers(app: FastAPI) -> None:
    app.add_exception_handler(JobMatcherError, _domain_error)
    app.add_exception_handler(StarletteHTTPException, _http_error)
    app.add_exception_handler(RequestValidationError, _validation_error)
    app.add_exception_handler(Exception, _unhandled)
