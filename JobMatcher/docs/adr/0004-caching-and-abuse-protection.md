# ADR 0004: Response caching and abuse protection

- **Status:** Accepted
- **Date:** 2026-11-27
- **Scope:** `caching.py`, `search/embeddings.py`, `search/cache.py`, `agent/answers.py`,
  `agent/session.py`, `budget/limits.py`, `db/cache_store.py`, `api/` (deps, middleware,
  captcha, routes), `retention.py`, migration 0008

## Context

The public demo has three expensive anonymous requests: `/chat` (several LLM calls, the only
thing that costs money), `POST /cv` (an LLM extraction and an embedding) and `/jobs/search`
(a CPU-bound e5 query encoding plus a hybrid SQL query). Only `/chat` was limited, per client
and per process, by requests per minute and tokens per day. Nothing capped how many chat
streams ran at once, how long one could run, how large a request body could be, or how often
a script could upload CVs or hammer search. And nothing was cached, although the same
questions and searches repeat (the example prompt in the UI, demo visitors trying the same
thing).

Caching an LLM answer is only correct when nothing it depends on has changed: the message,
the history, the CV profile, the job index, the model, the prompt and the tools. And a
CV-based answer contains CV-derived content, so it must never reach another caller.

## Decision

### Caches

| Cache | Where | Key | Lifetime | Bound |
|---|---|---|---|---|
| Query embeddings | `CachedEmbedder` in `get_embedder()`, per process | whitespace-normalised (NFC) query text | until evicted (vectors never go stale for one model) | `QUERY_EMBEDDING_CACHE_SIZE` (2048) LRU |
| `/jobs/search` responses | `SearchCache`, per process | normalised `q`, filters, `k`, `mode`, **index version** | `SEARCH_CACHE_TTL_S` (60 s) | `SEARCH_CACHE_SIZE` (1024) LRU |
| First chat turns | `chat_answer_cache` table, shared | see below | `CHAT_ANSWER_CACHE_TTL_S` (600 s) | expired rows purged by `retention` |

Setting a size or TTL to 0 turns that cache off.

**Index version** = `max(outbox.id)`, one index-only lookup per request. Every
`job.created` / `job.updated` / `job.closed` is written to the outbox in the same transaction
as the job, so a new, changed or closed job changes the version and misses every cached
response at once. Fact re-extraction (`batch_extract`, `reextract`) and `canonical_job_id`
updates write no outbox row; their effect on cached responses is bounded by the TTL (60 s for
search, 10 min for chat). The outbox purge cannot make the version repeat an older value
unless every row is deleted, and the version then changes anyway (to `NULL`).

`/jobs/search` sends `Cache-Control: public, max-age=60` (the same staleness bound, for
browsers and a CDN), or `private, max-age=60` when `AUTH_PROTECT_SEARCH=true` makes the
response depend on a token. Search sets no cookie, so a shared cache never stores one.

**Chat answer cache.** Only a **first turn** (empty `history`) is cached: a later turn depends
on the conversation, which would have to be part of the key and almost never repeats. The key
is SHA-256 over

- `PROMPT_VERSION`: a hash of the system prompt, the tool definitions, `MAX_ROUNDS`,
  `MAX_CALLS_PER_ROUND` and a manual `GRAPH_VERSION` (bump it when the graph changes behaviour
  without changing the prompt),
- the chat client class and model (`AnthropicClient:claude-haiku-5-5`); a client without a
  model name is never cached,
- the message, whitespace-normalised and case-folded,
- with a CV: the `cv_id`, **the principal** and a hash of the profile summary the prompt
  receives; without a CV: none of these, so no-CV answers are **shared** between callers
  (they depend on nothing personal),
- the index version.

A CV can only be used by its owner (ADR 0003), and the key also contains the owner, so a
CV-based answer cannot be served to anyone else even if a `cv_id` leaked. `chat_answer_cache.
cv_id` references `cv` with `ON DELETE CASCADE`: deleting a CV (or its retention) deletes its
cached answers. Keys are hashes; the table stores the replayable events only.

Stored: the turn's events (`tool_call`, `tool_result`, `answer`), only when the turn ended in
an `answer` with non-empty text and **no `unverified_urls`**. Errors, timeouts, budget stops,
disconnected streams and answers citing URLs the tools did not return are never stored. A hit
replays the same SSE event sequence with `"cached": true` added to every event; the UI notes
it. A replayed turn **counts against the per-minute request limit** but uses no tokens and
skips the budget check (it costs nothing).

Postgres rather than in-process for this cache, because the answers are the expensive part,
should survive a deploy, and one replica's answer should serve the others. A failed write is
logged and the stream goes on.

### Abuse limits

**Named rate buckets** (`budget/limits.RequestLimiter`): each bucket has one or more rates,
all of which must admit a hit; a hit refused by one is given back to the others.

| Bucket | Default | Client key |
|---|---|---|
| `chat` | `CHAT_REQUESTS_PER_MINUTE` (6) + `CHAT_DAILY_TOKENS_PER_CLIENT` | `ClientKeyDep` (address in session mode, principal in oidc) |
| `cv_upload` | `CV_UPLOADS_PER_MINUTE` (5) and `CV_UPLOADS_PER_DAY` (20) | `ClientKeyDep` |
| `cv_delete` | `CV_DELETES_PER_MINUTE` (10) | `ClientKeyDep` |
| `search` | `SEARCH_REQUESTS_PER_MINUTE` (60) | the token's principal if one is sent in oidc mode, else the address (search needs no identity) |

A refusal is `429` with `Retry-After` (`RateLimited`, a `ClientLimited` that also names the
guard for metrics).

**Backends** (`RATE_LIMIT_BACKEND`): `memory` (default) keeps sliding windows per process, so
with N replicas a client gets up to N times the limit. `postgres` uses the `rate_limit` table
(migration 0008): fixed windows, one atomic
`INSERT … ON CONFLICT DO UPDATE SET hits = hits + 1 WHERE hits < limit RETURNING hits` per
hit, shared by all replicas. Fixed windows allow up to twice the rate across a window
boundary; that is accepted for an abuse limit. Keys are SHA-256 of bucket, window and
client, so the table holds no addresses; rows expire with their window and `retention`
deletes them. If the database is unreachable the postgres store lets requests through (the
request needs the database anyway and fails there). The chat's daily token limit always
reads `llm_usage`, which is already shared.

**Concurrent chat streams** (`StreamSlots`): at most `CHAT_MAX_STREAMS` (32) per process and
`CHAT_MAX_STREAMS_PER_CLIENT` (3). Over the client's share: `429 rate_limited`; over the
total: `503 server_busy`, both with `Retry-After`. It refuses instead of queueing (an
`asyncio.Semaphore` would make requests wait while holding a connection). A slot is released
when the stream ends; one that is never released (the client went away before the stream
started) is reclaimed after the stream timeout plus 30 s.

**Stream timeout**: a turn still running after `CHAT_STREAM_TIMEOUT_S` (300) ends with an
`error` event; closing the agent's generator cancels its pending LLM or tool call. It is
above `LLM_TIMEOUT_S` (180) because a turn makes several calls; lower both for a hosted model.

**Duplicate messages** (`DuplicateGuard`): the same message (normalised, same CV and history)
from the same client within `CHAT_DUPLICATE_WINDOW_S` (10 s) is `429`, unless it is a cached
first turn, which is simply replayed. This catches double submits and a script replaying
one request; it is per process.

**Request body size** (`BodyLimitMiddleware`): bodies over `MAX_REQUEST_BODY_BYTES` (256 KiB)
get `413 payload_too_large`, by `Content-Length` or while reading a chunked body, before
anything is parsed. A 50,000-character CV and a full chat history fit, also JSON-escaped.

**CAPTCHA** (off by default): with `TURNSTILE_SECRET` set, `POST /cv` needs an
`X-Turnstile-Token` header that Cloudflare's siteverify accepts (server-side call with
`httpx`, the client address as `remoteip`); `403 captcha_failed` otherwise, `503
captcha_unavailable` when Cloudflare cannot be reached. With `TURNSTILE_SITE_KEY` set the
page renders the widget and its CSP allows `challenges.cloudflare.com`. Only CV uploads: chat
is already limited and gated by the budget, and a challenge per question would ruin the UI.
The rate limit runs before the verification, so a flood does not turn into siteverify calls.

**Metrics**: `jobmatcher_cache_lookups_total{cache=query_embedding|search|chat_answer,
result=hit|miss}` and `jobmatcher_rejected_requests_total{reason}` with `rate_<bucket>`,
`daily_tokens`, `concurrent_streams`, `server_busy`, `duplicate`, `body_too_large`,
`captcha` and `chat_timeout`.

### At the proxy

Application limits see the client address only when uvicorn trusts the proxy's
`X-Forwarded-For` (`FORWARDED_ALLOW_IPS`, already set for Caddy in `docker-compose.prod.yml`).
Caddy has no rate limiter in the standard build; the `caddy-ratelimit` plugin (a custom
build with `xcaddy`) could add a coarse per-address limit in front of everything, e.g.
`rate_limit { zone api { key {remote_host} events 120 window 1m } }`, and Caddy's
`request_body { max_size 256KB }` duplicates the body limit at the edge. Neither is
configured here: the application limits cover the expensive endpoints and work without a
custom Caddy image. Cloudflare in front would add its own rate limiting and bot protection.

## Consequences

- Repeated demo questions cost nothing and answer instantly; repeated searches skip the e5
  encode and the SQL query.
- A cached chat answer can be up to 10 minutes behind a re-extraction of job facts, and a
  search response up to 60 seconds; new, changed and closed jobs invalidate both at once.
- The model's answer to an identical first question no longer varies between callers within
  the TTL. The same answer would be produced again for the same inputs most of the time
  anyway; to compare model runs, set `CHAT_ANSWER_CACHE_TTL_S=0`.
- With several replicas, set `RATE_LIMIT_BACKEND=postgres`; stream slots and duplicate
  detection stay per process (each replica caps its own load, which is what they protect).
- Migration 0008 adds `chat_answer_cache` and `rate_limit`. Both hold hashes and expire;
  `retention` deletes expired rows.

## Alternatives considered

- **Redis** for caches and limits: one more service to run and secure for a demo-sized
  deployment; Postgres already handles the shared state at this load.
- **Caching every chat turn** keyed by the full history: correct, but the hit rate is near
  zero and the rows would hold whole conversations.
- **HTTP caching of `/chat`**: a POST with a streamed body; not cacheable by proxies.
- **slowapi / fastapi-limiter**: per-route decorators over the same in-memory or Redis
  counters; they do not cover the daily token limit or stream slots, and the named buckets
  are a small amount of code.
