# TODO — enterprise hardening (JobMatcher)

Work on branch `chore/enterprise-hardening` (from `dev`). Tick items as they land, one commit each.

## Audit
- [x] Structure: clear boundaries between api / ingest / search / agent / db; no business logic in routes
  - [x] shared types (`RawPosting`, `JobFacts`, `Chunk`, enums) in `jobmatcher.domain`; db/search/api no longer import ingest
  - [x] CV creation → `cv/service.py`, chat turn orchestration → `agent/session.py`, job lookup → `repository.get_job`
- [x] Remove comments and docstrings that only restate code; no dead code (legacy scheduler + `sources/` still needed?)
  - [x] removed scheduler, `sources/` adapters, `sources.yaml`, crawl settings, `legacy-scheduler` profile, unused `MAX_DELIVERY_ATTEMPTS` (JobIngestor owns crawling)
  - [x] ruff format applied (26 files had drifted)
- [x] Error handling: typed errors, no bare `except Exception`, consistent HTTP error schema
  - [x] `jobmatcher.errors` hierarchy; `LLMError` wraps httpx / SDK failures
  - [x] broad catches narrowed (agent loop, indexer, reextract, readyz); ruff `BLE` enforces it
  - [x] `{detail, code, request_id[, errors]}` for every error, `detail` stays a string for the web clients
  - [x] chat history validated as plain user/assistant turns (no injected tool_use / system)
- [x] Config: settings validated, `.env.example` complete, no secrets in repo
- [x] Logging / observability: structured logs, request ids, LLM call latency + token metrics
  - [x] `llm_call`, `chat_turn`, `http_request` events; `LOG_FORMAT=json`; `X-Request-ID`
  - [x] OpenAI-compatible providers (Ollama, Groq) now recorded in `llm_usage` — per-client token limits and the budget did not apply to them before
  - [x] the API process configures logging at all (it did not)
  - [ ] OpenTelemetry GenAI spans + optional Phoenix compose profile (ADR 0001) — not done: optional, the structured `llm_call` / `chat_turn` logs cover the numbers for now
- [x] mypy strict green (8 errors had accumulated; CI never ran mypy)
- [x] Add `CODING_GUIDELINES.md` + `CLAUDE.md` consistent with JobIngestor; CI workflow (lint, mypy, tests)
  - [x] CI: the workflow lives at `Projects/.github/` (outside this folder), so the replacement is documented in `docs/ci.md` instead of edited

## Tests
- [x] Unit: coverage report, fill gaps (config, LLM clients, API errors, readyz, currency)
- [x] Integration: run the existing suite against real Postgres + RabbitMQ, fix what fails
  - [x] tests ran against `DATABASE_URL` and truncated the real database; now `TEST_DATABASE_URL` (`*_test` only)
  - [x] two tests lacked the marker; markers now applied by directory
  - [x] worker against a real RabbitMQ vhost: gzip body indexed, retry queue, DLQ
- [x] E2E: `job.discovered` in → indexed job → `/jobs/search` and `/chat` out (scripted model; `E2E_LLM=local` passes with qwen2.5:7b)

## LLM / evaluation
- [x] Run the local model (Ollama `qwen2.5:7b`) on the RTX 3060 Ti and smoke-test extraction + chat (100% GPU)
- [x] Extraction eval set: labelled postings (cs + en, IT and non-IT) → field accuracy for seniority, years, remote policy, salary, skills
  - [x] found and fixed: `"cur": "Kč"` failed validation and discarded every Czech extraction with a salary
  - [x] prompt v2 adopted
- [x] Retrieval eval: run `eval/run_retrieval.py`, extend golden set, record hit@5 / recall@10 / MRR per mode
- [x] Agent eval: scripted questions → tool-call correctness, citation validity, latency, tokens
- [x] Compare local model vs hosted model; write results to `eval/RESULTS.md`
- [x] Decision record: plain loop vs LangChain / LangGraph / PydanticAI — [ADR 0001](docs/adr/0001-llm-framework.md): keep the loop, add tracing and evals

## ADR 0001 follow-ups
- [x] F1 doc drift: `MAX_ROUNDS` = 4, sequential tool calls, two providers, §14 row links the ADR
- [x] F2 `response_format` json_schema on the OpenAI-compatible path
- [ ] F3 OpenTelemetry GenAI spans (`chat`, `chat {model}`, `execute_tool {name}`) + optional `phoenix` compose profile
- [x] F4 extraction eval (`eval/run_extraction.py`)
- [x] F5 agent eval (`eval/run_agent.py`)
- [ ] F6 prompt-injection suite: postings with injected instructions, the agent must not follow them
- [ ] F7 MCP server exposing `search_jobs` / `get_job` (exists on `main` from before the refactor, not on `dev`)
- [x] F8 budget parity for OpenAI-compatible providers
- [ ] F9 DSPy / GEPA offline for the local extraction prompt — later, only if the extraction numbers call for it
