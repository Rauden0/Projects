# Coding guidelines

These rules apply to every change in JobMatcher. When a rule and existing code disagree, the
rule wins and the code gets refactored. They follow [JobIngestor's](../JobIngestor/CODING_GUIDELINES.md)
where the two codebases work the same way, and say so where they deliberately differ.

## Language and tooling

- Python 3.12+, everything typed. `mypy --strict` passes on `src/` (stricter than JobIngestor).
- ruff for lint (`E F I UP B ASYNC BLE`) and formatting; line length 100.
- No `if TYPE_CHECKING:` blocks. Import what you use.
- English for code, names, docs and commit messages. Prompts and fixtures may be Czech.

## Errors (exceptions, not `Result`)

JobMatcher uses exceptions where JobIngestor uses `Result`. The code is a pipeline of
async steps whose failures are handled in a few known places (the HTTP layer, the queue
consumer, the agent loop), and mixing in `Result` without migrating everything would give two
conventions. So:

- Failures a caller is expected to handle derive from `jobmatcher.errors.JobMatcherError`:
  `NotFound`, `LLMUnavailable`, `LLMError`, `ExtractionError`, `ToolError`, `BudgetExceeded`,
  `ClientLimited`. One class per failure the caller handles differently; carry the data needed
  to act on it (`retry_after_s`, `status_code`). Anything else is a bug.
- Third-party exceptions (httpx, the Anthropic SDK, asyncpg, aio-pika) are converted at the
  boundary that calls them: `llm.py` raises only `LLMError`; health checks catch
  `SQLAlchemyError` / `OSError` / `AMQPException`.
- Catch only what you can recover from. `except Exception` is allowed only at a process
  boundary where the alternative is losing work (the queue consumer that retries or
  dead-letters), with `# noqa: BLE001` and the reason. ruff `BLE` enforces this.
- The HTTP layer maps errors in one place, `api/errors.py`, to
  `{"detail", "code", "request_id"[, "errors"]}`. Routes never build error responses or raise
  `HTTPException` for domain failures.

## Structure

| Module | Contains | May import |
|---|---|---|
| `domain` | `RawPosting` (the `job.discovered` contract), `JobFacts`, `Chunk`, enum literals | pydantic only |
| `errors`, `observability`, `telemetry`, `config` | error types, logging / request ids / LLM call records, spans, settings | stdlib, pydantic, `opentelemetry-api` (the SDK only inside `telemetry.otlp_provider`) |
| `db` | models, session factory, repository functions | `domain`, `errors` |
| `llm`, `budget` | provider clients, budget guard, usage ledger | `db`, `config`, `errors`, `observability` |
| `ingest` | worker, indexer, extraction, normalization, chunking, listing sync, batch | everything below `api` |
| `search`, `cv`, `agent` | retrieval, CV profile and comparison, tools and the LangGraph agent | `db`, `domain`, `llm`, `search`; `langgraph` only in `agent` |
| `api` | FastAPI app, routes, dependencies, error mapping, middleware | everything |
| `retention` | process deleting CVs and usage client addresses past their retention | `db`, `config` |
| `mcp_server` | MCP entry point serving `search_jobs` / `get_job` from `Toolbox` | `agent`, `db`, `search`, `config`, `errors`, `observability` |

- Routes parse input, call one service function and shape the response. No queries, no
  orchestration, no business rules in `api/routes/` (see `cv/service.py`, `agent/session.py`).
- `db` and `search` never import from `ingest`; shared types live in `domain`.
- Collaborators come in through constructors or FastAPI dependencies (`api/deps.py`); `get_*`
  factories with `lru_cache` build the production instances. There is no DI container.
- Crawling belongs to JobIngestor. JobMatcher only consumes `job.discovered` and
  `source.crawled`; change `domain.RawPosting` only together with the producer.

## LLM code

- Every provider call goes through an `LLMClient` from `llm.py`, which checks the budget,
  records usage in `llm_usage`, logs `llm_call` with latency and tokens, opens a
  `chat {model}` span and raises `LLMError`.
- The chat agent is a LangGraph `StateGraph` in `agent/graph.py`
  ([ADR 0002](docs/adr/0002-langgraph.md)). Use LangGraph for orchestration only: nodes call
  `LLMClient`, never a LangChain chat model, and keep plain dicts in the state rather than
  LangChain message types. Collaborators go in the run's `context`, events out through the
  stream writer.
- LLM output is untrusted input: validate it with Pydantic, normalise what is harmless
  (currency symbols), and never let one bad field discard the rest.
- A prompt change needs an eval run (`eval/run_extraction.py`, `eval/run_agent.py`) and the
  numbers in `eval/RESULTS.md`.

## Style

- Prefer pure functions and immutable data (`@dataclass(frozen=True, slots=True)`, Pydantic
  models). Classes for stateful collaborators (clients, repositories, the toolbox).
- At most two levels of nesting inside a function; early returns and small helpers.
- Almost no comments. A comment explains a non-obvious external constraint (a provider quirk,
  why a filter keeps NULLs), never what the next line does. No docstrings that repeat the
  signature.

## Logging and tracing

- `logging.getLogger(__name__)`; configure once per process with `config.configure_logging()`
  (`LOG_LEVEL`, `LOG_FORMAT=text|json`). Every line carries the request id when there is one.
- Log events that matter for operations with a stable name and `extra` fields: `llm_call`,
  `chat_turn`, `http_request`. Never log secrets, CV text or full prompts.
- Spans come from the helpers in `telemetry.py` (`llm_call`, `tool_call`, `start_chat`), which
  hold every attribute name; the GenAI conventions are not stable yet. Content goes on a span
  only behind `OTEL_CAPTURE_CONTENT`, and errors as `error.type` without the message.
- Never hold a span current across a `yield`; iterate the generator through
  `telemetry.within` instead.

## Tests

| Level | Where | Needs | Run |
|---|---|---|---|
| unit | `tests/unit` | nothing | `pytest -m "not integration and not e2e"` |
| integration | `tests/integration` | migrated Postgres at `TEST_DATABASE_URL`, RabbitMQ at `TEST_AMQP_URL` | `pytest -m integration` |
| e2e | `tests/e2e` | same; `E2E_LLM=local` adds a real Ollama model | `pytest -m e2e` |

- Markers are applied by directory in `tests/conftest.py`.
- Tests that touch Postgres only run against a database named `*_test` (and the eval runners
  against `*_eval` / `*_test`); the guard refuses anything else, because they truncate tables.
- Fakes (scripted models, in-memory toolboxes, recording exchanges) instead of mocks; the
  network is never called from unit tests.
- Test names: `test_<what>_<condition>_<expected>` when it adds clarity.

## Git

- Small commits, one logical change each, type-prefixed subject: `add:`, `fix:`,
  `refactor:`, `test:`, `docs:`, `chore:`.
- `dev` is the working branch; feature branches start from it. `release/x.y` is cut from `dev`;
  `main` is production. Never commit straight to `main`.
