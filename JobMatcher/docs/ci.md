# CI

The workflow lives at the repository root, `../.github/workflows/jobmatcher-ci.yml`, because
the git root is `Projects/`. It currently runs `ruff check`, migrations and `pytest` against
service containers, on pushes to `main` and on pull requests.

What it does not check yet, and the replacement below adds:

| Gap | Effect today |
|---|---|
| `mypy src` never runs | eight strict-mode errors had accumulated unnoticed |
| no `ruff format --check` | 26 files had drifted from the formatter |
| tests ran against `DATABASE_URL` | fine in CI, but the same command truncated the real database locally; tests now need `TEST_DATABASE_URL` (a `*_test` database) |
| no RabbitMQ vhost for tests | the broker tests use `TEST_AMQP_URL` with the `jobmatcher_test` vhost |
| no coverage | `pytest-cov` reports it now |
| only `main` pushes | `dev` is the working branch |

Proposed `.github/workflows/jobmatcher-ci.yml` (to be applied by whoever owns the root
workflows; it was left untouched here because changes are scoped to `JobMatcher/`):

```yaml
name: Job Matcher CI

on:
  push:
    branches: [main, dev]
    paths: ["JobMatcher/**", ".github/workflows/jobmatcher-ci.yml"]
  pull_request:
    paths: ["JobMatcher/**", ".github/workflows/jobmatcher-ci.yml"]

defaults:
  run:
    working-directory: JobMatcher

jobs:
  test:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: pgvector/pgvector:pg16
        env:
          POSTGRES_USER: jobmatcher
          POSTGRES_PASSWORD: jobmatcher
          POSTGRES_DB: jobmatcher_test
        ports: ["5432:5432"]
        options: >-
          --health-cmd pg_isready --health-interval 5s --health-timeout 5s --health-retries 10
      rabbitmq:
        image: rabbitmq:3.13
        env:
          RABBITMQ_DEFAULT_VHOST: jobmatcher_test
        ports: ["5672:5672"]
        options: >-
          --health-cmd "rabbitmq-diagnostics -q check_port_connectivity"
          --health-interval 10s --health-timeout 5s --health-retries 10

    env:
      DATABASE_URL: postgresql+asyncpg://jobmatcher:jobmatcher@localhost:5432/jobmatcher_test
      TEST_DATABASE_URL: postgresql+asyncpg://jobmatcher:jobmatcher@localhost:5432/jobmatcher_test
      TEST_AMQP_URL: amqp://guest:guest@localhost:5672/jobmatcher_test

    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-python@v5
        with:
          python-version: "3.12"
          cache: pip
          cache-dependency-path: JobMatcher/pyproject.toml
      - run: pip install -e ".[dev]"
      - name: Lint and format
        run: |
          ruff check src tests eval alembic
          ruff format --check src tests eval alembic
      - name: Types
        run: mypy src
      - name: Migrate
        run: alembic upgrade head
      - name: Tests (unit, integration, e2e with a scripted model)
        run: pytest -q --cov=jobmatcher --cov-report=term-missing:skip-covered
```

The LLM evals are not part of CI: they need a GPU (or a paid API) and take minutes. Run them
before changing a prompt or a model and record the numbers in `eval/RESULTS.md`.
