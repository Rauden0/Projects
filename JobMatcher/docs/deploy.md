# Deploying

The recommended setup for a public demo is one small VPS (e.g. a Hetzner CX22: 2 vCPU, 4 GB RAM)
running the same compose file plus [`docker-compose.prod.yml`](../docker-compose.prod.yml):

- **Caddy** in front of `api:8000` with automatic Let's Encrypt TLS ([`Caddyfile`](../Caddyfile)).
  The API, Postgres and RabbitMQ stay published on `127.0.0.1` only.
- **No Ollama, no Phoenix.** A 7B model on two CPU cores takes minutes per answer. Use a hosted
  provider instead (see below); Phoenix stays an opt-in profile.
- **Memory limits** on every service, so one runaway process cannot take the host down.
- **The image from GHCR**, built and pushed by CI on every push to `main` ([ci.md](ci.md)).
  Nothing is built on the server.

## LLM provider

Pick one in `.env`:

- **Groq free tier** (OpenAI-compatible): `LLM_PROVIDER=openai_compat`,
  `LLM_BASE_URL=https://api.groq.com/openai/v1`, `LLM_MODEL` and `LLM_API_KEY`. Free, but rate
  limited; recorded at $0 in `llm_usage`.
- **Anthropic Haiku**, the cheapest paid option: `LLM_PROVIDER=anthropic`,
  `LLM_MODEL=claude-haiku-5-5`, `LLM_EFFORT=low` and `EXTRACTION_MODE=batch` (extraction through
  the Message Batches API at half price; start the `batch` profile). Keep
  `LLM_MONTHLY_BUDGET_USD` low; the cap blocks every LLM call once reached. Also set a spend
  limit on the workspace in the Claude Console.

See [`.env.example`](../.env.example) for all settings.

## First setup

```bash
# on the VPS, with Docker installed and DNS for the domain pointing at it
git clone <repo> && cd Projects/JobMatcher
cp .env.example .env
```

In `.env`, at least:

- `DOMAIN=jobs.example.com` and `JOBMATCHER_IMAGE=ghcr.io/<owner>/jobmatcher:latest`
- new `POSTGRES_PASSWORD`, `RABBITMQ_DEFAULT_USER`, `RABBITMQ_DEFAULT_PASS`, and the same values
  in `DATABASE_URL` and `AMQP_URL`
- the LLM provider settings above, `LOG_FORMAT=json`
- `SESSION_SECRET` (`python -c "import secrets; print(secrets.token_urlsafe(32))"`) and
  `AUTH_INSECURE_DEV=false`. With `AUTH_INSECURE_DEV=true` and no secret the API still starts,
  but signs session cookies with a key made at startup, so every deploy logs everybody out of
  their uploaded CV. Rotating `SESSION_SECRET` does the same on purpose.

### Authentication

The default `AUTH_MODE=session` needs no accounts: each browser gets an anonymous id in a
signed cookie and can only see and delete its own CV. Caddy serves HTTPS, so the cookie's
`Secure` flag holds; keep `SESSION_COOKIE_SECURE=true`.

For a deployment with real users, set `AUTH_MODE=oidc`, `OIDC_ISSUER` (the issuer URL exactly
as it appears in the tokens' `iss`) and `OIDC_AUDIENCE`, and put an authenticating proxy such
as oauth2-proxy between Caddy and the API that forwards the token as `Authorization: Bearer`
([ADR 0003](adr/0003-authentication.md)). The API fetches the issuer's keys over HTTPS, so it
needs outbound access to the issuer. `AUTH_PROTECT_SEARCH=true` also closes `/jobs/*`.

Do not publish `jobmatcher-mcp --http`; if you must, it refuses to bind anything but loopback
without `MCP_HTTP_TOKEN`.

`docker-compose.prod.yml` sets `FORWARDED_ALLOW_IPS` on the API to Caddy's fixed address, so
uvicorn takes the client address from `X-Forwarded-For` and the per-client chat limits count
real clients rather than the proxy. Never set it to `*` while the API port is reachable by
anything but the proxy: any client could then pick its own address.

If the GHCR package is private, `docker login ghcr.io` with a token that has `read:packages`.

## Deploy

```bash
alias dc='docker compose -f docker-compose.yml -f docker-compose.prod.yml'
dc pull && dc up -d                     # add --profile batch with EXTRACTION_MODE=batch
```

The same two commands deploy every later version: `migrate` runs before the app services start.
Pin `JOBMATCHER_IMAGE` to a `:<commit sha>` tag to roll back.

## Backups

A nightly `pg_dump` from the host's crontab, keeping a week:

```cron
30 3 * * * cd /root/Projects/JobMatcher && docker compose exec -T postgres pg_dump -U jobmatcher -Fc jobmatcher > /var/backups/jobmatcher-$(date +\%F).dump && find /var/backups -name 'jobmatcher-*.dump' -mtime +7 -delete
```

Restore with `pg_restore -U jobmatcher -d jobmatcher --clean` inside the postgres container.
Copy the dumps off the host (e.g. a Hetzner Storage Box) or they die with it.

## Limits

The memory limits in the override are ceilings for a 4 GB host; the API and the worker each load
the embedding model (~0.5 GB). Check `docker stats` after the first day and adjust.
