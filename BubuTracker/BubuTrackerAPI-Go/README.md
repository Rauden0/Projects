# BubuTracker API (Go)

Go rewrite of the BubuTracker backend, replacing the previous ASP.NET Core
implementation. Auth0 remains the identity provider; the data store moved
from SQLite to PostgreSQL.

## Architecture

Layered, dependency-inverted design:

```
cmd/api        entrypoint: wires config, DB, auth, services, router; graceful shutdown
cmd/migrate    standalone migration runner (separate deploy step from the API)
internal/domain     framework-free business types and sentinel errors
internal/service    business logic; depends only on repository interfaces (ports.go)
internal/store       Postgres implementation of those interfaces (sqlc-generated queries)
internal/handler     HTTP handlers + chi router; translates domain errors to HTTP
internal/httpserver  transport-level helpers: JSON encoding, error mapping, middleware
internal/auth        Auth0 JWT/JWKS validation middleware
internal/config      environment-based configuration, fails fast on missing values
db/                  SQL migrations (golang-migrate) and sqlc queries/config
deploy/              Dockerfile and docker-compose.yml
api/openapi.yaml     API contract (schemas, error codes)
```

The service layer depends on `UserRepository` / `LocationRepository` /
`TrackingRepository` interfaces it declares itself (`internal/service/ports.go`);
`internal/store` is the only package that knows about Postgres or sqlc. This
keeps business rules unit-testable with in-memory fakes (see
`internal/service/*_test.go`) with no database required.

## API

All routes except `/healthz` and `/readyz` require `Authorization: Bearer <Auth0 access token>`.

| Method | Path                       | Description                              |
|--------|----------------------------|-------------------------------------------|
| GET    | /healthz                   | Liveness probe                            |
| GET    | /readyz                    | Readiness probe (pings the database)      |
| GET    | /api/v1/users/me           | Get (and lazily provision) my profile     |
| PATCH  | /api/v1/users/me           | Update my first/last name                 |
| POST   | /api/v1/locations/me       | Upsert my current location                 |
| GET    | /api/v1/locations/tracked  | Locations of the users I track            |
| GET    | /api/v1/tracking           | List the users I track                    |
| POST   | /api/v1/tracking           | Start tracking a user by email (rate limited: 10/min/user) |
| DELETE | /api/v1/tracking/{userID}  | Stop tracking a user                      |

Full request/response schemas and error codes: [`api/openapi.yaml`](api/openapi.yaml)
(open with the [Swagger Editor](https://editor.swagger.io) or any OpenAPI viewer).

## Local development

```bash
cp .env.example .env        # fill in your Auth0 domain/audience
make compose-up             # postgres + migrate + api
```

Or without Docker, against a local Postgres:

```bash
export $(cat .env | xargs)
go run ./cmd/migrate -direction up
go run ./cmd/api
```

## Regenerating the database layer

Query result types in `internal/store/sqlc` are generated from
`db/queries/*.sql` by [sqlc](https://sqlc.dev), configured in `db/sqlc.yaml`.
After changing a query or migration:

```bash
make sqlc-generate
```

## Testing

Two tiers, run differently on purpose:

```bash
make test              # unit tests: no Docker, no network, no external state
make test-integration   # internal/store against a real, disposable Postgres
```

**Unit tests** (`internal/service`, `internal/handler`, `internal/httpserver`,
`internal/config`, `internal/auth`) use fakes/httptest and run in
milliseconds. `internal/auth` is genuinely exercised, not just trusted: its
tests spin up an in-process fake OIDC provider (real RSA signing, real JWKS)
and check signature, audience, and expiry validation against real JWTs —
including negative cases like a token signed by an unknown key.

**Integration tests** (`internal/store`, build-tagged `integration`) run the
real SQL against a real Postgres. This is deliberate: every bug this project
has actually shipped (the upsert race, the email-casing gap, the tracked-
locations query silently dropping trackless users) lived in the SQL itself,
which a fake repository has no way to catch. `make test-integration` starts
Postgres via `deploy/docker-compose.yml`, applies migrations, runs the suite,
and tears it down. Without `TEST_DATABASE_URL` set, these tests skip
themselves rather than fail, so `go test ./...` stays safe to run anywhere.

CI (`.github/workflows/bubutracker-api-go-ci.yml`, repo root) runs both
tiers on every push/PR touching this project, against a Postgres service
container.

## Security notes for deployment

- **This binary speaks plain HTTP, not HTTPS.** It expects to sit behind a
  TLS-terminating reverse proxy or load balancer (standard in ECS/k8s/most
  PaaS setups). Deployed without one, bearer tokens and location data travel
  in cleartext — this is an operational requirement, not optional hardening.
- **Use a least-privilege DB role at runtime.** `deploy/docker-compose.yml`'s
  single `bubutracker` role both owns the schema (for local `cmd/migrate`
  convenience) and serves the app; in production, run migrations with a
  privileged role and point the running API at a separate role scoped to
  `SELECT/INSERT/UPDATE/DELETE` on `users`, `locations`, `user_tracking`
  only — no `CREATE`/`ALTER`/`DROP`.
- **Dependencies are scanned with [`govulncheck`](https://go.dev/blog/vuln)**,
  not just `go.sum`-pinned and forgotten; run it before upgrading dependencies
  and periodically otherwise, since new CVEs get published against versions
  already in `go.sum`.
- `POST /api/v1/tracking` is rate limited per user (see the API table above)
  specifically because its 404-vs-201/409 responses are otherwise an email
  enumeration oracle — the "add by email" UX and that leak are inherent to
  each other, so throttling is the mitigation, not eliminating the signal.
