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

### Compatibility rules

- Version 1 is the current contract. Messages without the envelope fields are version 1.
- Additive changes keep the version: a new field is optional (or has a default) and
  consumers ignore fields they do not know, so either side can deploy first.
- Renaming, removing or retyping a field, or making an optional field required, is a
  breaking change: bump `schema_version`, have the consumer accept both versions first,
  then switch the producer.
