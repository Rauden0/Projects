# Deploying

The recommended setup for a public demo is one small VPS (e.g. a Hetzner CX22: 2 vCPU, 4 GB RAM,
about €4.50 a month) running the same compose file plus
[`docker-compose.prod.yml`](../docker-compose.prod.yml):

- **Caddy** in front of `api:8000` with automatic Let's Encrypt TLS ([`Caddyfile`](../Caddyfile)).
  Only ports 80 and 443 are public; the API, Postgres and RabbitMQ stay on `127.0.0.1`.
- **No Ollama, no Phoenix.** A 7B model on two CPU cores takes minutes per answer. Use a hosted
  provider instead (see below); Phoenix and Prometheus stay opt-in profiles.
- **Memory limits, restart policies and log rotation** on every service, so one runaway process
  cannot take the host down and the disk does not fill with container logs.
- **The image from GHCR**, built and pushed by CI on every push to `main` ([ci.md](ci.md)).
  Nothing is built on the server.

Everything the server needs is in [`deploy/`](../deploy):

| File | What it does |
|---|---|
| `bootstrap-vps.sh` | once, as root on a fresh Ubuntu 24.04: Docker, a `deploy` user with your SSH key, ufw (22/80/443), unattended upgrades, 2 GB swap, `/opt/projects` and `/var/backups/jobmatcher` |
| `gen-env.sh DOMAIN IMAGE` | writes `.env` from `.env.example` with fresh Postgres, RabbitMQ and `SESSION_SECRET` secrets and the production settings; refuses to overwrite an existing one |
| `deploy.sh [TAG]` | pulls, starts, waits for every service to be healthy and for `/readyz`; a tag pins `JOBMATCHER_IMAGE` in `.env` first |
| `backup.sh`, `jobmatcher-backup.{service,timer}` | nightly verified `pg_dump`, a week kept, optional rsync off the host |
| `smoke.sh` | the whole production stack on your machine behind Caddy's internal CA (see below) |

## LLM provider

Pick one in `.env`. `gen-env.sh` writes the Groq settings; only `LLM_API_KEY` is left to fill in.

- **Groq free tier** (OpenAI-compatible): `LLM_PROVIDER=openai_compat`,
  `LLM_BASE_URL=https://api.groq.com/openai/v1`, `LLM_MODEL` and `LLM_API_KEY`. Free, but rate
  limited; recorded at $0 in `llm_usage`.
- **Anthropic Haiku**, the cheapest paid option: `LLM_PROVIDER=anthropic`,
  `LLM_MODEL=claude-haiku-5-5`, `LLM_EFFORT=low` and `EXTRACTION_MODE=batch` (extraction through
  the Message Batches API at half price; `deploy.sh` then starts the `batch` profile). Keep
  `LLM_MONTHLY_BUDGET_USD` low; the cap blocks every LLM call once reached. Also set a spend
  limit on the workspace in the Claude Console.

See [`.env.example`](../.env.example) for all settings.

## First setup

1. Create the VPS (Ubuntu 24.04, your SSH key for root) and point an A (and AAAA) record for
   the domain at it.
2. As root: `bash bootstrap-vps.sh "ssh-ed25519 AAAA... you@laptop"`. Read it first; it disables
   SSH password logins.
3. As `deploy`:

   ```bash
   git clone --filter=blob:none --sparse https://github.com/<owner>/Projects.git /opt/projects
   cd /opt/projects && git sparse-checkout set JobMatcher && cd JobMatcher
   deploy/gen-env.sh jobs.example.com ghcr.io/<owner>/jobmatcher:latest
   $EDITOR .env                            # LLM_API_KEY, optionally the Anthropic settings
   echo "$GHCR_READ_TOKEN" | docker login ghcr.io -u <owner> --password-stdin   # private package only
   deploy/deploy.sh
   ```

   A private repository needs a read-only deploy key for the clone (`ssh-keygen -t ed25519`, add
   the public key under the repository's *Settings > Deploy keys*, clone over `git@github.com:`).
   A private GHCR package needs a classic token with only `read:packages`; making the package
   public (*Package settings > Change visibility*) needs none.
4. `sudo cp deploy/jobmatcher-backup.{service,timer} /etc/systemd/system/ && sudo systemctl
   daemon-reload && sudo systemctl enable --now jobmatcher-backup.timer`

`.env` carries `COMPOSE_FILE=docker-compose.yml:docker-compose.prod.yml` and
`COMPOSE_PROJECT_NAME=jobmatcher`, so every plain `docker compose ...` in that directory uses the
production override.

Postgres and RabbitMQ read their passwords only when their volumes are first created. Changing
them later needs `ALTER USER jobmatcher PASSWORD '...'` and `rabbitmqctl change_password`, then
the same values in `DATABASE_URL` and `AMQP_URL`.

### Authentication

The default `AUTH_MODE=session` needs no accounts: each browser gets an anonymous id in a
signed cookie and can only see and delete its own CV. `gen-env.sh` generates `SESSION_SECRET`
and `docker-compose.prod.yml` forces `AUTH_INSECURE_DEV=false` and `SESSION_COOKIE_SECURE=true`
on the API, so it refuses to start without a secret instead of signing with a key that dies on
every deploy. Rotating `SESSION_SECRET` logs everybody out of their uploaded CV, on purpose.

For a deployment with real users, set `AUTH_MODE=oidc`, `OIDC_ISSUER` (the issuer URL exactly
as it appears in the tokens' `iss`) and `OIDC_AUDIENCE`, and put an authenticating proxy such
as oauth2-proxy between Caddy and the API that forwards the token as `Authorization: Bearer`
([ADR 0003](adr/0003-authentication.md)). The API fetches the issuer's keys over HTTPS, so it
needs outbound access to the issuer. `AUTH_PROTECT_SEARCH=true` also closes `/jobs/*`.

Do not publish `jobmatcher-mcp --http`; if you must, it refuses to bind anything but loopback
without `MCP_HTTP_TOKEN`.

`docker-compose.prod.yml` sets `FORWARDED_ALLOW_IPS` on the API to Caddy's fixed address
(`172.30.0.10` on the compose network `172.30.0.0/24`), so uvicorn takes the client address from
`X-Forwarded-For` and the per-client chat limits count real clients rather than the proxy.
Never set it to `*` while the API port is reachable by anything but the proxy: any client could
then pick its own address. `/metrics` answers 404 to anything that came through Caddy; the
`metrics` profile's Prometheus scrapes the containers directly.

## Deploy

```bash
deploy/deploy.sh               # the image in .env, normally :latest
deploy/deploy.sh 3f2c1ab...    # pin a commit sha: deploy or roll back to exactly that image
```

`migrate` runs before the app services start, and `deploy.sh` fails unless every service is
healthy within five minutes and `/readyz` reports the database and the broker. A migration that
cannot be undone by the previous image is a reason to restore the backup rather than to roll
back. `git pull` first when the compose files or the `Caddyfile` changed.

### From CI

The workflow's `deploy` job runs after the image is pushed on `main` and does the same over SSH:
`git pull --ff-only` and `deploy/deploy.sh <sha>`. It is skipped (not failed) until these
repository secrets exist:

| Secret | Value |
|---|---|
| `DEPLOY_HOST` | the VPS address |
| `DEPLOY_USER` | `deploy` |
| `DEPLOY_SSH_KEY` | a private key made for CI (`ssh-keygen -t ed25519 -f jobmatcher-ci -N ''`); its public half goes into `~deploy/.ssh/authorized_keys` |
| `DEPLOY_KNOWN_HOSTS` | the output of `ssh-keyscan -t ed25519 <host>`, checked against the host's fingerprint |
| `DEPLOY_PATH` | optional, default `/opt/projects/JobMatcher` |

## Smoke test on your machine

```bash
deploy/smoke.sh                 # KEEP=1 leaves it running, SMOKE_HTTPS_PORT=18443 by default
```

It builds the image as `jobmatcher:smoke`, writes an `.env` with `gen-env.sh` outside the
checkout, starts the production stack as compose project `jmprod` with
[`deploy/smoke.override.yml`](../deploy/smoke.override.yml) (no host ports except Caddy's on
`127.0.0.1`, its own subnet) and checks `/healthz`, `/readyz` and a CV upload through Caddy, that
`/metrics` is not public, that the worker, relay and retention stay up and that `docker compose
stop` is quick. It removes the stack, its volumes and the image afterwards.

## Backups

`deploy/backup.sh`, run nightly at 03:30 by the systemd timer, writes
`/var/backups/jobmatcher/jobmatcher-<UTC time>.dump` (`pg_dump -Fc`), checks it with
`pg_restore --list` before keeping it, and deletes dumps older than `BACKUP_KEEP_DAYS` (7).
`systemctl list-timers jobmatcher-backup.timer` shows the next run,
`journalctl -u jobmatcher-backup` the last ones.

Copy the dumps off the host or they die with it: set `BACKUP_RSYNC_TARGET` in the service (for
example a Hetzner Storage Box, about €3.20 a month for 1 TB, reachable over SSH with a key of the
`deploy` user), or enable the provider's server backups.

Restore:

```bash
docker compose stop api worker outbox-relay retention
docker compose exec -T postgres pg_restore -U jobmatcher -d jobmatcher --clean --if-exists \
  < /var/backups/jobmatcher/jobmatcher-<time>.dump
docker compose start api worker outbox-relay retention
```

## Limits

The memory limits in the override are ceilings for a 4 GB host; the API and the worker each load
the embedding model (~0.5 GB), downloaded into the `models` volume on their first start. Check
`docker stats` after the first day and adjust.

RabbitMQ is not reachable from outside the host. A JobIngestor on another machine needs
`AMQP_BIND` plus a firewall rule limited to its address, or better a WireGuard/Tailscale link.
