# Observability

Three signals, each optional beyond logs:

| Signal | Where | Turned on by |
|---|---|---|
| Logs | stdout, `LOG_FORMAT=json` for one JSON object per line with `request_id` | always |
| Traces | OTLP/HTTP spans for chat turns, LLM calls and tool calls (`telemetry.py`) | `OTEL_EXPORTER_OTLP_ENDPOINT` and the `otel` extra |
| Metrics | Prometheus text format (`metrics.py`) | the `metrics` extra |

## Metrics

Install `jobmatcher[metrics]` (`prometheus-client`). Without it every recorder in
`jobmatcher.metrics` is a no-op, the API has no `/metrics` route and nothing listens on
`METRICS_PORT`.

- **API**: `GET /metrics` on the API port. It answers 404 to anything carrying
  `X-Forwarded-For`, i.e. every request that came through Caddy, so the public site does not
  expose model names and spend; Prometheus scrapes `api:8000` directly.
- **Worker and outbox relay** have no HTTP server: with `METRICS_PORT` set they start one with
  `prometheus_client.start_http_server` (the compose file sets `9100`, reachable only on the
  compose network).
- **Local Prometheus**: `docker compose --profile metrics up -d`, UI on
  `http://localhost:9090`, scrape targets in [`prometheus.yml`](../prometheus.yml). The image
  must be built with the `metrics` extra (add it to the `pip install ".[...]"` line in the
  `Dockerfile`), otherwise the targets are up but serve nothing.

### Why `prometheus_client` and not the OpenTelemetry metrics SDK

Traces use OTel because spans need a vendor-neutral exporter and the GenAI conventions. For
metrics the only consumer is Prometheus, and `prometheus_client` is one small pure-Python
dependency with a pull endpoint built in. The OTel route needs the SDK, a
`PrometheusMetricReader` from `opentelemetry-exporter-prometheus` (still pre-1.0) and a meter
provider set up in every process, and its unit/suffix translation produces names that differ
from what the queries below expect. All metric names live in `metrics.py` and are recorded
from three chokepoints (`telemetry.llm_call`, `telemetry.end_chat`, the ASGI middleware) plus
one line per outcome in the worker and the relay, so switching to OTel later touches one module.

### Metric list

| Metric | Type | Labels | Recorded in |
|---|---|---|---|
| `jobmatcher_http_request_duration_seconds` | histogram | `method`, `route` (template, `<unmatched>` for 404s), `status` | `MetricsMiddleware`; for `/chat` this is the whole SSE stream |
| `jobmatcher_http_first_byte_seconds` | histogram | `method`, `route` | `MetricsMiddleware`; for `/chat` the first SSE event |
| `jobmatcher_chat_turns_total` | counter | `outcome` (`answer`, `error`, `incomplete`) | `telemetry.end_chat` |
| `jobmatcher_llm_calls_total` | counter | `purpose`, `model`, `provider`, `outcome` (`ok`, `error`, `budget_exceeded`) | `telemetry.llm_call` |
| `jobmatcher_llm_call_duration_seconds` | histogram | `purpose`, `model` | `telemetry.llm_call` (budget check and ledger write included) |
| `jobmatcher_llm_tokens_total` | counter | `purpose`, `model`, `kind` (`input` incl. cached, `output`) | `telemetry.llm_call` |
| `jobmatcher_llm_cost_usd_total` | counter | `purpose`, `model` | `budget.pricing.cost_usd` at list price; unpriced models count $0 |
| `jobmatcher_ingest_messages_total` | counter | `outcome` (`ok`, `retry`, `dead_letter`, `malformed`, `requeued`) | `ingest/worker.py`, one per settled `job.discovered` delivery |
| `jobmatcher_outbox_published_total` | counter | | `outbox/relay.py` |
| `jobmatcher_outbox_lag_seconds` | gauge | | `outbox/relay.py`: age of the oldest unpublished row at the last batch, 0 when empty |

Plus the default `process_*` and `python_gc_*` collectors of each process. Batch extraction
(`batch_extract`) goes through the Message Batches API, not `llm_call`, so its tokens are only
in `llm_usage`.

## SLIs and SLOs

Targets from the architecture review. Windows are 28 days unless stated.

| SLI | SLO | PromQL |
|---|---|---|
| Search latency | p95 `/jobs/search` < 300 ms | `histogram_quantile(0.95, sum by (le) (rate(jobmatcher_http_request_duration_seconds_bucket{route="/jobs/search"}[5m])))` |
| Chat time to first event | p95 < 2 s | `histogram_quantile(0.95, sum by (le) (rate(jobmatcher_http_first_byte_seconds_bucket{route="/chat"}[5m])))` |
| Chat completion | p95 < 20 s | `histogram_quantile(0.95, sum by (le) (rate(jobmatcher_http_request_duration_seconds_bucket{route="/chat"}[5m])))` |
| Chat error rate | < 2 % of turns | `sum(rate(jobmatcher_chat_turns_total{outcome!="answer"}[1h])) / sum(rate(jobmatcher_chat_turns_total[1h]))` |
| Ingest success | dead-lettered < 0.5 % of messages | `sum(rate(jobmatcher_ingest_messages_total{outcome=~"dead_letter\|malformed"}[1h])) / sum(rate(jobmatcher_ingest_messages_total{outcome=~"ok\|dead_letter\|malformed"}[1h]))` |
| Outbox lag | < 60 s | `max(jobmatcher_outbox_lag_seconds)`, and `up{job="outbox-relay"} == 0` (a dead relay stops updating the gauge) |
| API availability | 5xx < 0.5 % of requests | `sum(rate(jobmatcher_http_request_duration_seconds_count{status=~"5.."}[5m])) / sum(rate(jobmatcher_http_request_duration_seconds_count[5m]))` |
| Cost per chat turn | tracked, no target | `sum(increase(jobmatcher_llm_cost_usd_total{purpose="chat"}[1d])) / sum(increase(jobmatcher_chat_turns_total[1d]))` |
| Budget burn | alert at 80 % of the month | `sum by (purpose) (increase(jobmatcher_llm_cost_usd_total[30d]))` against `LLM_MONTHLY_BUDGET_USD` (× `LLM_EXTRACTION_BUDGET_SHARE` for extraction) |
| LLM errors | < 5 % of calls per purpose | `sum by (purpose, model) (rate(jobmatcher_llm_calls_total{outcome="error"}[15m])) / sum by (purpose, model) (rate(jobmatcher_llm_calls_total[15m]))` |

The `route` label is the FastAPI path template (`/jobs/{job_id}`), so its cardinality is the
number of routes. Check the template names with `count by (route) (jobmatcher_http_request_duration_seconds_count)`.

Example alert rule (put it in a `rule_files:` entry of `prometheus.yml`):

```yaml
groups:
  - name: jobmatcher
    rules:
      - alert: OutboxLagging
        expr: max(jobmatcher_outbox_lag_seconds) > 60 or absent(up{job="outbox-relay"} == 1)
        for: 5m
      - alert: IngestDeadLettering
        expr: >
          sum(rate(jobmatcher_ingest_messages_total{outcome=~"dead_letter|malformed"}[1h]))
          / sum(rate(jobmatcher_ingest_messages_total{outcome=~"ok|dead_letter|malformed"}[1h]))
          > 0.005
        for: 15m
      - alert: SearchSlow
        expr: >
          histogram_quantile(0.95, sum by (le)
            (rate(jobmatcher_http_request_duration_seconds_bucket{route="/jobs/search"}[5m]))) > 0.3
        for: 10m
```

### Not measured yet

- **Freshness** (`now − crawled_at` until searchable, p95 < 15 min) and the share of
  `extraction_status='fallback'` and of chat turns with `unverified_urls` live in Postgres; use
  SQL over `job` and the `chat_turn` log lines until they get their own metrics.
- **Queue and DLQ depth**: RabbitMQ 3.13 ships the `rabbitmq_prometheus` plugin
  (`rabbitmq-plugins enable rabbitmq_prometheus`, port 15692); add it as a scrape target rather
  than polling queues from the worker.
- Indexing time per phase (extract / embed / DB), HNSW latency, open jobs per source.

## Evals

The retrieval eval runs in CI with a threshold ([ci.md](ci.md)). The extraction and agent
evals need an LLM and run on the development machine; see [ci.md](ci.md) and
[`eval/RESULTS.md`](../eval/RESULTS.md).
