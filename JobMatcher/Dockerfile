FROM python:3.12-slim

ENV PYTHONDONTWRITEBYTECODE=1 PYTHONUNBUFFERED=1 HF_HOME=/models
WORKDIR /app

# CPU-only torch keeps the image ~1.5 GB smaller than the default CUDA build.
RUN pip install --no-cache-dir torch --index-url https://download.pytorch.org/whl/cpu

COPY pyproject.toml ./
COPY src ./src
RUN pip install --no-cache-dir ".[llm,embeddings]"

COPY alembic.ini sources.yaml ./
COPY alembic ./alembic

CMD ["uvicorn", "jobmatcher.api.main:app", "--host", "0.0.0.0", "--port", "8000"]
