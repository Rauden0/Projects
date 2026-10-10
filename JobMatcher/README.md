# Job Matcher

Ingests job postings from public career-site APIs, indexes them for hybrid (semantic +
full-text) search in PostgreSQL, and exposes an LLM agent that answers questions like
*"Which remote Python backend roles fit my CV, and what am I missing?"* — citing the
postings it used.

See [ARCHITECTURE.md](ARCHITECTURE.md) for the design and the trade-offs behind it.

## Stack

Python 3.12 · FastAPI · SQLAlchemy 2 (async) · PostgreSQL 16 + pgvector · Alembic ·
RabbitMQ (aio-pika) · Pydantic v2 · multilingual-e5 embeddings · Ollama or Claude (tool use) ·
pytest · Docker Compose · GitHub Actions

## What's inside

- **Ingestion** — Greenhouse, Lever and Ashby adapters → RabbitMQ → worker that normalizes,
  deduplicates by content hash, extracts structured facts (seniority, years required,
  remote policy, salary, stack) with an LLM validated by Pydantic, chunks by section and
  embeds. Retries via per-delay TTL queues, then a dead-letter queue.
- **Transactional outbox** — `job.indexed` / `job.closed` events are written in the same
  transaction as the data and published by a relay using `FOR UPDATE SKIP LOCKED`.
- **Hybrid search** — hard filters, then pgvector (HNSW, cosine) and Postgres full-text
  candidates fused with Reciprocal Rank Fusion, in a single SQL statement.
- **Agent** — a plain tool-calling loop (no framework) with `search_jobs`, `get_job` and
  `compare_with_cv`; round and parallelism limits; tool errors fed back to the model;
  answer URLs checked against tool results; events streamed over SSE.
- **Evaluation** — a frozen fixture set and golden questions; hit@5, recall@10 and MRR per
  search mode.

## Run it

Runs fully locally by default — the LLM is [Ollama](https://ollama.com), no API key needed.

```bash
cp .env.example .env
docker compose up --build
docker compose exec ollama ollama pull qwen2.5:7b   # once, ~4.7 GB
```

Any OpenAI-compatible endpoint works (`LLM_BASE_URL`, e.g. Groq or Gemini), and Anthropic
via `LLM_PROVIDER=anthropic` — see [`.env.example`](.env.example). Without a reachable LLM,
ingestion falls back to heuristic extraction; only `/chat` needs the model.

- API docs: http://localhost:8000/docs
- RabbitMQ UI: http://localhost:15672 (guest / guest)

Companies to crawl live in [`sources.yaml`](sources.yaml).

```bash
# search without the LLM
curl "localhost:8000/jobs/search?q=python%20backend&remote_policy=remote_region&max_years_required=3"

# upload a CV, then chat (Server-Sent Events)
curl -X POST localhost:8000/cv -H "content-type: application/json" -d '{"text": "..."}'
curl -N -X POST localhost:8000/chat -H "content-type: application/json" \
     -d '{"cv_id": "<id>", "message": "Remote Python roles that fit me?"}'
```

## Develop

```bash
pip install -e ".[dev]"          # add ,embeddings for the real e5 model
ruff check src tests eval alembic
pytest -q                        # integration tests skip without Postgres
```

Integration tests need a migrated Postgres with pgvector and run in CI against service
containers.

## Evaluate retrieval

```bash
alembic upgrade head
python eval/run_retrieval.py            # configured embedder (e5 if installed)
python eval/run_retrieval.py --hashing  # dependency-free baseline
```

Prints a markdown table of hit@5, recall@10 and MRR for vector, full-text and hybrid search
over [`eval/golden.yaml`](eval/golden.yaml).

## Roadmap

- Workday adapter
- Minimal web UI for the chat stream
- Terraform deployment to GCP (Cloud Run, Cloud SQL) with plan/apply in CI
