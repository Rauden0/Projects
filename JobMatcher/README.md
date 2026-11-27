# Job Matcher

Indexes job postings crawled by [JobIngestor](../JobIngestor) for hybrid (semantic +
full-text) search in PostgreSQL, and exposes an LLM agent that answers questions like
*"Which remote Python backend roles fit my CV, and what am I missing?"* — citing the
postings it used.

See [ARCHITECTURE.md](ARCHITECTURE.md) for the design and the trade-offs behind it.

## Stack

Python 3.12 · FastAPI · SQLAlchemy 2 (async) · PostgreSQL 16 + pgvector · Alembic ·
RabbitMQ (aio-pika) · Pydantic v2 · multilingual-e5 embeddings · Ollama or Claude (tool use) ·
pytest · Docker Compose · GitHub Actions

## What's inside

- **Ingestion** — `job.discovered` messages from JobIngestor → RabbitMQ → worker that normalizes,
  deduplicates by content hash, extracts structured facts (seniority, years required,
  remote policy, salary, stack) with an LLM validated by Pydantic, chunks by section and
  embeds. Retries via per-delay TTL queues, then a dead-letter queue.
- **Transactional outbox** — `job.created` / `job.updated` / `job.closed` events are written in the same
  transaction as the data and published by a relay using `FOR UPDATE SKIP LOCKED`.
- **Hybrid search** — hard filters, then pgvector (HNSW, cosine) and Postgres full-text
  candidates fused with Reciprocal Rank Fusion, in a single SQL statement.
- **Agent** — a LangGraph state graph (`model` and `tools` nodes; provider calls still go
  through `LLMClient`) with `search_jobs`, `get_job` and
  `compare_with_cv`; round and parallelism limits; tool errors fed back to the model;
  answer URLs checked against tool results; events streamed over SSE.
- **MCP server** — `search_jobs` and `get_job` served over the Model Context Protocol from the
  same `Toolbox`, so Claude Desktop, Claude Code or an IDE can search the index directly.
- **Evaluation** — extraction (35 labelled cs/en postings, IT and non-IT), retrieval
  (41 golden questions, per search mode), agent (scripted questions, tool use and citation
  validity) and prompt-injection (13 postings that carry instructions to the model) evals
  against the local model; see [eval/RESULTS.md](eval/RESULTS.md).
- **Operations** — one JSON error shape (`{detail, code, request_id}`), `X-Request-ID` on every
  request and log line, `llm_call` / `chat_turn` log events with latency and tokens
  (`LOG_FORMAT=json` for log shippers), every LLM call in the `llm_usage` ledger, OpenTelemetry
  traces of chat turns, LLM and tool calls (Phoenix as the local viewer), settings validated at
  startup.

## Run it

Runs fully locally by default — the LLM is [Ollama](https://ollama.com), no API key needed.

```bash
cp .env.example .env
docker compose up --build
docker compose exec ollama ollama pull qwen2.5:7b   # once, ~4.7 GB
```

Ollama runs on the CPU by default. With an NVIDIA GPU (and the NVIDIA Container Toolkit) add the
override: `docker compose -f docker-compose.yml -f docker-compose.gpu.yml up --build`.

Any OpenAI-compatible endpoint works (`LLM_BASE_URL`, e.g. Groq or Gemini), and Anthropic
via `LLM_PROVIDER=anthropic` — see [`.env.example`](.env.example). Without a reachable LLM,
ingestion falls back to heuristic extraction; only `/chat` needs the model.

- Chat UI: http://localhost:8000 (paste a CV, then ask)
- API docs: http://localhost:8000/docs
- RabbitMQ UI: http://localhost:15672 (`RABBITMQ_DEFAULT_USER` / `RABBITMQ_DEFAULT_PASS`,
  guest / guest by default)

Every port is published on `127.0.0.1` only. For a public deployment (one VPS, Caddy with TLS,
a hosted LLM, the image from GHCR, nightly backups) see [docs/deploy.md](docs/deploy.md).
Behind a proxy set `FORWARDED_ALLOW_IPS` to the proxy's address: uvicorn runs with
`--proxy-headers` and trusts `X-Forwarded-For` only from there, so the per-client chat limits
count real clients instead of the proxy. Never `*` while the API is reachable directly.

Crawling runs in [JobIngestor](../JobIngestor): a separate service with a source registry, a
planner and horizontally scaled crawlers. It publishes `job.discovered` (only new or changed
postings) to the same RabbitMQ, so the worker here indexes them unchanged, and `source.crawled`
after each crawl, which the worker uses to reopen listed jobs and close missing ones when the
listing was complete.

```bash
docker compose up --build                       # JobMatcher: api, worker, relay, RabbitMQ
cd ../JobIngestor && docker compose up --build  # JobIngestor: registry api, planner, crawlers
docker compose exec api job-ingestor seed sources.yaml
```

JobMatcher has no crawler of its own any more: the adapters and the scheduler that used to live
here were removed once JobIngestor took over, so without JobIngestor nothing new is indexed
(the API still serves what is already in the database).

```bash
# search without the LLM
curl "localhost:8000/jobs/search?q=python%20backend&remote_policy=remote_region&max_years_required=3"

# upload a CV, then chat (Server-Sent Events); the CV belongs to the session cookie
curl -c jar -b jar -X POST localhost:8000/cv -H "content-type: application/json" -d '{"text": "..."}'
curl -c jar -b jar -N -X POST localhost:8000/chat -H "content-type: application/json" \
     -d '{"cv_id": "<id>", "message": "Remote Python roles that fit me?"}'
```

## Authentication

Every uploaded CV belongs to the caller that uploaded it; chatting with it or deleting it from
anywhere else is a 404, as if it did not exist ([ADR 0003](docs/adr/0003-authentication.md)).
`AUTH_MODE` picks who the caller is:

- **`session`** (default): no accounts. The API gives each browser an anonymous id in a
  signed `HttpOnly`, `SameSite=Lax`, `Secure` cookie. Set `SESSION_SECRET` (32+ characters,
  `python -c "import secrets; print(secrets.token_urlsafe(32))"`); without it the API refuses
  to start, unless `AUTH_INSECURE_DEV=true` (what `.env.example` sets for local use), which
  signs with a key made at startup, so a restart forgets every session. Browsers keep Secure
  cookies on `http://localhost`; set `SESSION_COOKIE_SECURE=false` only for plain http on
  another host. Clearing cookies loses access to the uploaded CV (upload it again).
- **`oidc`**: every `/cv` and `/chat` call needs `Authorization: Bearer <JWT>` from
  `OIDC_ISSUER` for `OIDC_AUDIENCE`, checked against the issuer's published keys. The caller is
  the token's `sub`, which also keys the chat limits. `/jobs/*` stays public unless
  `AUTH_PROTECT_SEARCH=true`. The UI asks for a token to paste when it gets a 401; for real
  users, put an authenticating proxy such as oauth2-proxy in front (see the ADR).

`POST` requests need `Content-Type: application/json` and cross-site requests are refused
(CSRF protection for the cookie). CVs uploaded before owners were recorded belong to nobody
and are only removed by retention.

## Caching and abuse limits

Decided in [ADR 0004](docs/adr/0004-caching-and-abuse-protection.md).

- **Caches.** Query embeddings (LRU, `QUERY_EMBEDDING_CACHE_SIZE`), `/jobs/search` responses
  (`SEARCH_CACHE_TTL_S`, 60 s, also sent as `Cache-Control`) and first chat turns without
  history (`CHAT_ANSWER_CACHE_TTL_S`, 10 min, in Postgres). Every new, changed or closed job
  invalidates the search and chat caches at once; a replayed chat answer has `"cached": true`
  on its events, counts against the per-minute limit and costs no tokens. An answer that used
  a CV is only ever replayed to the CV's owner; deleting the CV deletes it.
- **Limits.** Per client: chat requests and tokens (above), CV uploads
  (`CV_UPLOADS_PER_MINUTE`, `CV_UPLOADS_PER_DAY`), CV deletes, searches
  (`SEARCH_REQUESTS_PER_MINUTE`), concurrent chat streams (`CHAT_MAX_STREAMS_PER_CLIENT`, and
  `CHAT_MAX_STREAMS` in total, then 503), and the same chat message twice within
  `CHAT_DUPLICATE_WINDOW_S`. Refusals are 429 with `Retry-After`. Chat turns end after
  `CHAT_STREAM_TIMEOUT_S`; request bodies over `MAX_REQUEST_BODY_BYTES` get 413. With several
  API replicas set `RATE_LIMIT_BACKEND=postgres` so they share the rate windows.
- **CAPTCHA.** Set `TURNSTILE_SECRET` and `TURNSTILE_SITE_KEY` to require a Cloudflare
  Turnstile check on CV uploads; off by default.
- **Metrics.** `jobmatcher_cache_lookups_total{cache,result}` and
  `jobmatcher_rejected_requests_total{reason}`.

## Personal data

`POST /cv` stores the CV text, its extracted profile and an embedding. `DELETE /cv/{cv_id}`
(the "Smazat CV" button in the UI) removes them at once; otherwise the `retention` service
deletes CVs `CV_RETENTION_DAYS` (default 30) after upload. Chat limits store the client IP (in
`AUTH_MODE=oidc` the token subject) on `llm_usage` rows, and the same service clears it after
two days. Cached chat answers (10 minutes) and rate limit windows (at most a day) are keyed
by hashes; the same service deletes them once expired, and an answer that used a CV goes
with the CV. The session cookie holds a random id and nothing else. At upload the CV text (its
first 12,000 characters) is sent once to the configured extraction LLM to build the profile;
set `CV_LLM_EXTRACTION=false` to keep the CV text local and use the keyword-based profile
instead. Chat messages and the CV profile (skills, years, languages, city, seniority, desired
roles) are sent to the configured chat LLM. With a hosted provider that provider is a
processor of that data.

## Use it from an MCP client

`jobmatcher-mcp` serves the agent's read-only tools, `search_jobs` and `get_job`, over MCP,
with the same schemas and code the agent uses. `compare_with_cv` stays with `/chat`, because
it needs a CV uploaded in that conversation. The server only needs Postgres with indexed jobs;
it reads `DATABASE_URL` (default: the Compose Postgres on localhost) and loads the embedding
model at startup.

```bash
pip install -e ".[mcp,embeddings]"
jobmatcher-mcp                                  # stdio (also: python -m jobmatcher.mcp_server)
jobmatcher-mcp --http --port 8001               # streamable HTTP at http://127.0.0.1:8001/mcp
```

The HTTP transport binds to `127.0.0.1` unless you pass `--host`. On any other address it
refuses to start without `MCP_HTTP_TOKEN` (32+ characters); with a token set, clients must send
`Authorization: Bearer <token>`.

Claude Desktop starts stdio servers itself, without your shell's virtualenv or `.env`, so
give the absolute path to the script and the environment it needs. Add this to
`claude_desktop_config.json` (Windows: `%APPDATA%\Claude\`, macOS:
`~/Library/Application Support/Claude/`):

```json
{
  "mcpServers": {
    "jobmatcher": {
      "command": "C:\\path\\to\\JobMatcher\\.venv\\Scripts\\jobmatcher-mcp.exe",
      "env": {
        "DATABASE_URL": "postgresql+asyncpg://jobmatcher:jobmatcher@localhost:5432/jobmatcher"
      }
    }
  }
}
```

On Linux and macOS the command is `.venv/bin/jobmatcher-mcp`. VS Code takes the same `command`
and `env` under `"servers"` in `.vscode/mcp.json`. For Claude Code:

```bash
claude mcp add jobmatcher -e DATABASE_URL=postgresql+asyncpg://... -- /path/to/.venv/bin/jobmatcher-mcp
claude mcp add --transport http jobmatcher http://127.0.0.1:8001/mcp   # or a running --http server
```

Each tool call opens its own database session. A missing job, invalid arguments or an
unreachable database come back as `isError` tool results the client's model can read, and
every call is logged as `mcp_tool_call` with its duration.

## LLM costs

Crawling never calls an LLM; the ingestor only fetches and publishes, and only postings whose
fingerprint changed reach extraction. Embeddings run locally.

With `LLM_PROVIDER=anthropic` and `claude-haiku-5-5` ($0.10 / $0.50 per million tokens):

| Step | Tokens | Cost |
|---|---|---|
| Extraction, one posting | ~1.4k in (boilerplate dropped, capped at 6,000 chars), ~80 out (short-key JSON) | ~$0.00018, ~$0.00009 with `EXTRACTION_MODE=batch` |
| Chat, one question | ~10k in, half from the prompt cache, ~1k out | ~$0.001-0.003 |

Every call is written to `llm_usage` with its cost. `LLM_MONTHLY_BUDGET_USD` (default 40) is a
hard cap: extraction stops at `LLM_EXTRACTION_BUDGET_SHARE` of it (jobs then keep heuristic
facts and are retried later) and chat answers 503 at the full amount. Chat is also limited per
client IP: `CHAT_REQUESTS_PER_MINUTE` and `CHAT_DAILY_TOKENS_PER_CLIENT`. Behind a reverse
proxy, run uvicorn with `--proxy-headers --forwarded-allow-ips` so the limits see the real
client address. Set a spend limit on the workspace in the Claude Console as well.
The cap is checked before every call, whatever the provider: `LLM_MONTHLY_BUDGET_USD=0` blocks
all LLM calls, free local models included, so to run on Ollama without a cap leave it at a
positive value. Only models listed in `budget/pricing.py` have a cost; any other model is
recorded at $0 (a missing Claude model is logged as `llm_model_unpriced`) and never reaches the
cap.

## Tracing

Every `/chat` turn is one OpenTelemetry trace: a `chat` root span with a `chat {model}` span per
LLM call and an `execute_tool {name}` span per tool call. Extraction calls in the worker get
their own `chat {model}` spans. LLM spans use the GenAI semantic-convention attributes:
provider, request and response model, finish reason, and input, output, cache-read and
cache-write tokens. The root span carries the request id, so a trace can be found from a log
line.

Tracing is off unless `OTEL_EXPORTER_OTLP_ENDPOINT` is set; without it no SDK is loaded and
the spans cost nothing. Export is plain OTLP over HTTP, so any backend that accepts it works
(Phoenix, Langfuse, Datadog, a Collector). Locally, Phoenix runs as one container:

```bash
docker compose --profile phoenix up -d phoenix
# in .env: OTEL_EXPORTER_OTLP_ENDPOINT=http://phoenix:6006 (http://localhost:6006 from the host)
docker compose up -d api worker                 # recreate them with the new setting
```

The UI is at http://localhost:6006, without a login: Phoenix 22 turns auth on by default, which
rejects OTLP without an API key, so the profile turns it off (with Phoenix's own usage telemetry
and its agent assistant) and publishes the port on localhost only. Traces land in Phoenix's
`default` project; the API reports as service `jobmatcher`, the worker as `jobmatcher-worker`
(`OTEL_SERVICE_NAME`). The image installs the `otel` extra (SDK and OTLP exporter); a local
install needs `.[otel]`, which `dev` includes, and refuses to start with the endpoint set but
the extra missing.

Spans carry no prompt, completion, CV or posting text by default. `OTEL_CAPTURE_CONTENT=true`
adds the system prompt, the messages, the model output and tool arguments and results, which
is meant for local debugging only. The messages are stored in the provider's own format, so
Phoenix shows them as attributes rather than as a chat view. Message Batches extraction
(`EXTRACTION_MODE=batch`) is not traced.

## Branches

`dev` for development, `release/x.y` cut from it for staging, `main` for production (tagged on
merge). Fixes on a release branch are merged back into `dev`.

## Develop

See [CODING_GUIDELINES.md](CODING_GUIDELINES.md).

```bash
python -m venv .venv && .venv/Scripts/pip install -e ".[dev,embeddings,llm,mcp]"   # bin/ on Linux
ruff check src tests eval alembic && ruff format --check src tests eval alembic
mypy src
pytest -q -m "not integration and not e2e"          # unit tests, no services
```

Integration and e2e tests need Postgres and RabbitMQ, and only ever touch a database named
`*_test` and their own RabbitMQ vhost, because they truncate tables:

```bash
docker compose up -d postgres rabbitmq ollama
docker compose exec postgres psql -U jobmatcher -c "CREATE DATABASE jobmatcher_test"
docker compose exec postgres psql -U jobmatcher -d jobmatcher_test -c "CREATE EXTENSION vector"
docker compose exec rabbitmq rabbitmqctl add_vhost jobmatcher_test
docker compose exec rabbitmq rabbitmqctl set_permissions -p jobmatcher_test guest ".*" ".*" ".*"
DATABASE_URL=postgresql+asyncpg://jobmatcher:jobmatcher@localhost:5432/jobmatcher_test alembic upgrade head

pytest -q --cov=jobmatcher                          # unit + integration + e2e (scripted model)
E2E_LLM=local pytest -q -m e2e                      # e2e with the local Ollama model
```

Override the targets with `TEST_DATABASE_URL` and `TEST_AMQP_URL`. The e2e test publishes a
gzipped `job.discovered` message, lets the real worker index it, finds it via `/jobs/search`
and asks `/chat`, which must cite it.

## Evaluate

```bash
python eval/run_extraction.py --model qwen2.5:7b    # structured extraction, per field
python eval/run_retrieval.py                        # hit@5 / recall@10 / MRR per search mode
python eval/run_agent.py --model qwen2.5:7b         # tool use, citations, latency, tokens
python eval/run_agent.py --model qwen2.5:7b --cases eval/agent/injection.yaml   # prompt injection
```

They use the `jobmatcher_eval` database (`EVAL_DATABASE_URL`). Commands, numbers and the
conclusions drawn from them are in [eval/RESULTS.md](eval/RESULTS.md). Headlines with
qwen2.5:7b on an RTX 3060 Ti:

| | |
|---|---|
| Extraction | 95.5% mean field accuracy, 100% valid JSON, 1.4 s per posting (heuristic baseline 59.2%) |
| Retrieval | hybrid hit@5 0.83, recall@10 0.90, MRR 0.79 over 41 questions; weakest on English queries for Czech postings |
| Agent | relevant posting cited in 85% of 33 runs, no invented URLs, 3.0 rounds and ~4.6k input tokens per question |
| Prompt injection | 13 postings with instructions to the model, 6 runs each: payload followed in 9.0% of runs before hardening, 2.6% with tool results as untrusted data without control tokens or links |

## Roadmap

- Terraform deployment to GCP (Cloud Run, Cloud SQL) with plan/apply in CI
- Descriptions for jobs.cz ads that redirect to JavaScript-only employer sites
