# CI

The workflow lives at the repository root, `../.github/workflows/jobmatcher-ci.yml`, because
the git root is `Projects/`. It runs `ruff check`, `ruff format --check`, `mypy src`, migrations
and `pytest` with coverage against Postgres and RabbitMQ service containers, on pushes to `main`
and `dev` and on pull requests.

The `retrieval-eval` job runs in parallel with `test`: it installs the `embeddings` extra with
CPU-only torch, migrates a `jobmatcher_eval` service database, indexes the frozen fixtures with
`intfloat/multilingual-e5-small` and runs the 41 golden questions of
[`eval/golden.yaml`](../eval/golden.yaml). It fails when hybrid search (the production mode)
over all questions drops below hit@5 0.80 or recall@10 0.85, a few points under the numbers in
[`eval/RESULTS.md`](../eval/RESULTS.md) (0.83 / 0.90). Embedding on CPU is deterministic, so a
failure means a change to chunking, embedding text, SQL or ranking moved the results; rerun it
locally with the same command and update `RESULTS.md` (and the thresholds, if the drop is
intended). The model (~470 MB) is kept in the Actions cache under `~/.cache/huggingface`.

The `image` job then builds the `Dockerfile` with BuildKit (layers cached in the GitHub Actions
cache), so a broken image fails the pull request instead of the deploy. On pushes to `main` it
also pushes `ghcr.io/<owner>/jobmatcher:latest` and `:<commit sha>` with the workflow's
`GITHUB_TOKEN` (`packages: write`); [deploy.md](deploy.md) pulls that image. The owner is
lowercased because GHCR rejects upper-case repository names.

What the earlier workflow missed:

| Gap | Effect today |
|---|---|
| `mypy src` never runs | eight strict-mode errors had accumulated unnoticed |
| no `ruff format --check` | 26 files had drifted from the formatter |
| tests ran against `DATABASE_URL` | fine in CI, but the same command truncated the real database locally; tests now need `TEST_DATABASE_URL` (a `*_test` database) |
| no RabbitMQ vhost for tests | the broker tests use `TEST_AMQP_URL` with the `jobmatcher_test` vhost |
| no coverage | `pytest-cov` reports it now |
| only `main` pushes | `dev` is the working branch |
| the image was never built | a broken `Dockerfile` showed up only on deploy |

Current `.github/workflows/jobmatcher-ci.yml`:

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
      - run: pip install -e ".[dev,llm,mcp]"
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

  retrieval-eval:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: pgvector/pgvector:pg16
        env:
          POSTGRES_USER: jobmatcher
          POSTGRES_PASSWORD: jobmatcher
          POSTGRES_DB: jobmatcher_eval
        ports: ["5432:5432"]
        options: >-
          --health-cmd pg_isready --health-interval 5s --health-timeout 5s --health-retries 10

    env:
      DATABASE_URL: postgresql+asyncpg://jobmatcher:jobmatcher@localhost:5432/jobmatcher_eval
      EVAL_DATABASE_URL: postgresql+asyncpg://jobmatcher:jobmatcher@localhost:5432/jobmatcher_eval

    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-python@v5
        with:
          python-version: "3.12"
          cache: pip
          cache-dependency-path: JobMatcher/pyproject.toml
      - name: Install (CPU-only torch, as in the Dockerfile)
        run: |
          pip install torch --index-url https://download.pytorch.org/whl/cpu
          pip install -e ".[embeddings]"
      - uses: actions/cache@v4
        with:
          path: ~/.cache/huggingface
          key: hf-multilingual-e5-small-v1
      - name: Migrate
        run: alembic upgrade head
      - name: Retrieval eval (fails below hybrid hit@5 0.80 or recall@10 0.85)
        run: python eval/run_retrieval.py --min-hit5 0.80 --min-recall10 0.85

  image:
    needs: [test, retrieval-eval]
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write
    steps:
      - uses: actions/checkout@v4
      - uses: docker/setup-buildx-action@v3
      - name: Image name (GHCR needs a lowercase owner)
        run: echo "IMAGE=ghcr.io/${GITHUB_REPOSITORY_OWNER,,}/jobmatcher" >> "$GITHUB_ENV"
      - if: github.event_name == 'push' && github.ref == 'refs/heads/main'
        uses: docker/login-action@v3
        with:
          registry: ghcr.io
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}
      - uses: docker/build-push-action@v6
        with:
          context: JobMatcher
          push: ${{ github.event_name == 'push' && github.ref == 'refs/heads/main' }}
          tags: |
            ${{ env.IMAGE }}:latest
            ${{ env.IMAGE }}:${{ github.sha }}
          cache-from: type=gha
          cache-to: type=gha,mode=max
```

The install step includes the `mcp` extra because `src/jobmatcher/mcp_server.py` imports the
optional `mcp` SDK; without it `mypy src` fails on the import and `tests/unit/test_mcp_server.py`
is skipped.

The extraction and agent evals are not part of CI, not even nightly: they need an LLM (a GPU
running Ollama, or a paid API key in the repository secrets) and take minutes, and a hosted
runner has neither. Run them on the development machine before changing a prompt, a model or
the agent graph, and record the numbers in `eval/RESULTS.md`:

```bash
docker compose up -d postgres ollama
docker compose exec ollama ollama pull qwen2.5:7b
export EVAL_DATABASE_URL=postgresql+asyncpg://jobmatcher:jobmatcher@localhost:5432/jobmatcher_eval
python eval/run_extraction.py --model qwen2.5:7b     # field accuracy, skills P/R/F1, latency
python eval/run_agent.py --model qwen2.5:7b          # agent questions against the fixtures
python eval/run_agent.py --model qwen2.5:7b --cases eval/agent/injection.yaml   # injection suite
```

A self-hosted runner with a GPU could run them on a `schedule:` trigger with the same
commands; there is none today.
