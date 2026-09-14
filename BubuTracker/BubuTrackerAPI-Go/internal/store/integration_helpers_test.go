//go:build integration

package store_test

import (
	"context"
	"os"
	"testing"

	"github.com/jackc/pgx/v5/pgxpool"
	"github.com/stretchr/testify/require"

	"github.com/Rauden0/bubutracker-api/internal/store/sqlc"
)

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
