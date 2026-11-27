# jobmatcher

Read and follow [CODING_GUIDELINES.md](CODING_GUIDELINES.md) for every change. The git root is
the parent `Projects/` folder: stage only files under `JobMatcher/`.

## Commands

```bash
python -m venv .venv && .venv/Scripts/pip install -e ".[dev,embeddings,llm]"
ruff check src tests eval alembic && ruff format --check src tests eval alembic
mypy src
pytest -q -m "not integration and not e2e"      # unit, no services

docker compose up -d postgres rabbitmq ollama   # once; models: ollama pull qwen2.5:7b
DATABASE_URL=postgresql+asyncpg://jobmatcher:jobmatcher@localhost:5432/jobmatcher_test alembic upgrade head
pytest -q                                       # + integration and e2e (TEST_DATABASE_URL, TEST_AMQP_URL)
E2E_LLM=local pytest -q -m e2e                  # e2e against the local model
```

Integration/e2e tests use the `jobmatcher_test` database and the `jobmatcher_test` RabbitMQ
vhost (`rabbitmqctl add_vhost jobmatcher_test`); evals use `jobmatcher_eval`. Never point
them at `jobmatcher`, which holds real indexed jobs.

## Evals

See [eval/RESULTS.md](eval/RESULTS.md) for the numbers and exact commands:
`eval/run_extraction.py`, `eval/run_retrieval.py`, `eval/run_agent.py`.
