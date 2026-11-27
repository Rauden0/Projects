# Message contracts

JobMatcher exchanges JSON messages with JobIngestor over the `jobs` topic exchange. The
Pydantic models in `src/jobmatcher/domain.py` are the source of truth; the JSON Schemas in
[`contracts/`](contracts/) are exported from them with `model_json_schema()` and
`tests/unit/test_contracts.py` fails when the two drift apart. After changing a model:

```bash
UPDATE_CONTRACTS=1 pytest tests/unit/test_contracts.py
```

and send the updated schema to the JobIngestor side together with the change.

## Inbound (JobIngestor → JobMatcher)

| Routing key | Queue | Model | Schema |
|---|---|---|---|
| `job.discovered` | `job.discovered` | `RawPosting` | [job.discovered.json](contracts/job.discovered.json) |
| `source.crawled` | `job.source_crawled` | `SourceCrawled` | [source.crawled.json](contracts/source.crawled.json) |

Both carry the optional envelope fields of `InboundEvent`:

| Field | Type | Default | Use |
|---|---|---|---|
| `schema_version` | int ≥ 1 | `1` | contract version the producer wrote |
| `event_id` | string | `null` | producer's id of the event, for logs and deduplication |
| `crawled_at` | RFC 3339 timestamp | `null` | when the data was fetched; a naive timestamp is read as UTC |

`crawled_at` orders events. A `job.discovered` crawled before the stored posting's
`crawled_at` (a stale retry from `job.discovered.retry.*`) changes nothing, and a complete
`source.crawled` closes only jobs whose `crawled_at` (else `last_seen_at`) is before its own
`crawled_at`, so it cannot close a posting crawled after the listing. When either side has
no `crawled_at` the old behaviour applies: last write wins, and everything not listed closes.

### Compatibility rules

- Version 1 is the current contract. Messages without the envelope fields are version 1.
- Additive changes keep the version: a new field is optional (or has a default) and
  consumers ignore fields they do not know, so either side can deploy first.
- Renaming, removing or retyping a field, or making an optional field required, is a
  breaking change: bump `schema_version`, have the consumer accept both versions first,
  then switch the producer.

## Outbound (JobMatcher → consumers)

The outbox relay publishes to the `jobs` exchange. No consumer exists yet (a future
notifier), so there are no queues for these routing keys.

| Routing key | When | `data` | Schema |
|---|---|---|---|
| `job.created` | a posting was indexed for the first time | `JobChanged` | [job.created.json](contracts/job.created.json) |
| `job.updated` | a known posting changed and was re-indexed | `JobChanged` | [job.updated.json](contracts/job.updated.json) |
| `job.closed` | a complete listing no longer lists the posting | `JobClosed` | [job.closed.json](contracts/job.closed.json) |

Every body is an `EventEnvelope`, modelled on CloudEvents:

```json
{
  "event_id": "4711",
  "type": "job.created",
  "version": 1,
  "occurred_at": "2026-11-27T10:00:00.123456Z",
  "data": {"job_id": 42, "kind": "greenhouse", "source": "acme", "external_id": "123", "url": "https://..."}
}
```

- `event_id` is the outbox row id, also set as the AMQP `message_id`. Delivery is
  at-least-once, so consumers deduplicate on it.
- `occurred_at` is when the change committed, not when it was published.
- `type` equals the routing key and the AMQP `type` property.
- `version` follows the same compatibility rules as the inbound `schema_version`.

These replace the earlier `job.indexed` (bare `{job_id, source, external_id}`, which did not
tell a new job from a changed one) and the bare `job.closed` `{job_id}`; nothing consumed
either.
