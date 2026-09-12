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
| POST   | /api/v1/tracking           | Start tracking a user by email            |
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

```bash
make test
```

Service and handler layers are covered with fakes/httptest — no database or
Auth0 tenant needed to run them.
