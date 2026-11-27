# TODO — enterprise hardening (JobMatcher)

Work on branch `chore/enterprise-hardening` (from `dev`). Tick items as they land, one commit each.

## Audit
- [ ] Structure: clear boundaries between api / ingest / search / agent / db; no business logic in routes
- [ ] Remove comments and docstrings that only restate code; no dead code (legacy scheduler + `sources/` still needed?)
- [ ] Error handling: typed errors, no bare `except Exception`, consistent HTTP error schema
- [ ] Config: settings validated, `.env.example` complete, no secrets in repo
- [ ] Logging / observability: structured logs, request ids, LLM call latency + token metrics
- [ ] Add `CODING_GUIDELINES.md` + `CLAUDE.md` consistent with JobIngestor; CI workflow (lint, mypy, tests)

## Tests
- [ ] Unit: coverage report, fill gaps
- [ ] Integration: run the existing suite against real Postgres + RabbitMQ, fix what fails
- [ ] E2E: `job.discovered` in → indexed job → `/jobs/search` and `/chat` out

## LLM / evaluation
- [ ] Run the local model (Ollama `qwen2.5:7b`) on the RTX 3060 Ti and smoke-test extraction + chat
- [ ] Extraction eval set: labelled postings (cs + en, IT and non-IT) → field accuracy for seniority, years, remote policy, salary, skills
- [ ] Retrieval eval: run `eval/run_retrieval.py`, extend golden set, record hit@5 / recall@10 / MRR per mode
- [ ] Agent eval: scripted questions → tool-call correctness, citation validity, latency, tokens
- [ ] Compare local model vs hosted model; write results to `eval/RESULTS.md`
- [ ] Decision record: plain loop vs LangChain / LangGraph / PydanticAI (see `docs/adr/`)
