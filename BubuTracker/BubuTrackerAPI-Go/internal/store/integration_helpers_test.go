//go:build integration

// Package store_test's integration suite runs against a real Postgres —
// exactly the layer that a fake repository can't verify. Every bug this
// project has actually shipped so far (the upsert race, the email-casing
// gap, the INNER JOIN silently dropping trackless users) lived in the SQL
// itself, not in the Go glue around it, which unit tests with fakes have no
// way to catch.
//
// Run via `make test-integration` (starts Postgres, applies migrations,
// runs `go test -tags=integration`) rather than plain `go test ./...`, so
// the ordinary unit test suite stays fast and needs no external services.
package store_test

import (
	"context"
	"os"
	"testing"

	"github.com/jackc/pgx/v5/pgxpool"
	"github.com/stretchr/testify/require"

	"github.com/Rauden0/bubutracker-api/internal/store/sqlc"
)

// setupDB connects to TEST_DATABASE_URL and truncates all tables so each
// test starts from an empty schema. It expects migrations to already be
// applied — see the package doc comment.
func setupDB(t *testing.T) (*pgxpool.Pool, *sqlc.Queries) {
	t.Helper()

	dsn := os.Getenv("TEST_DATABASE_URL")
	if dsn == "" {
		t.Skip("TEST_DATABASE_URL not set; run `make test-integration` to run this suite against a real Postgres")
	}

	ctx := context.Background()
	pool, err := pgxpool.New(ctx, dsn)
	require.NoError(t, err)
	t.Cleanup(pool.Close)

	require.NoError(t, pool.Ping(ctx))
	_, err = pool.Exec(ctx, "TRUNCATE user_tracking, locations, users RESTART IDENTITY CASCADE")
	require.NoError(t, err)

	return pool, sqlc.New(pool)
}
