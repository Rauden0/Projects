FROM python:3.12-slim

ENV PYTHONDONTWRITEBYTECODE=1 PYTHONUNBUFFERED=1 PIP_NO_CACHE_DIR=1 HF_HOME=/models
WORKDIR /app

RUN useradd --create-home --uid 10001 app && mkdir -p /models && chown app:app /models

# CPU-only torch keeps the image ~1.5 GB smaller than the default CUDA build.
RUN pip install torch --index-url https://download.pytorch.org/whl/cpu

# Dependencies first, against an empty package, so source edits reuse this layer.
COPY pyproject.toml ./
RUN mkdir -p src/jobmatcher && touch src/jobmatcher/__init__.py \
    && pip install ".[llm,embeddings,otel,mcp,metrics]" \
    && rm -rf src

COPY src ./src
RUN pip install --no-deps --force-reinstall .

COPY alembic.ini ./
COPY alembic ./alembic

USER app

CMD ["uvicorn", "jobmatcher.api.main:app", "--host", "0.0.0.0", "--port", "8000", "--proxy-headers"]
