//go:build integration

package store_test

import (
	"context"
	"testing"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"

	"github.com/Rauden0/bubutracker-api/internal/store"
)

func TestLocationRepository_Upsert_InsertsThenUpdatesInPlace(t *testing.T) {
	_, queries := setupDB(t)
	users := store.NewUserRepository(queries)
	locations := store.NewLocationRepository(queries)
	ctx := context.Background()

	user, err := users.UpsertByAuth0Subject(ctx, "auth0|1", "a@example.com", "A", "B")
	require.NoError(t, err)

	first, err := locations.Upsert(ctx, user.ID, 50.1, 14.4)
	require.NoError(t, err)
	assert.Equal(t, 50.1, first.Latitude)

	second, err := locations.Upsert(ctx, user.ID, 51.5, 0.1)
	require.NoError(t, err)
	assert.Equal(t, 51.5, second.Latitude)
	assert.Equal(t, 0.1, second.Longitude)
	assert.False(t, second.UpdatedAt.Before(first.UpdatedAt))
}

func TestLocationRepository_Upsert_RejectsOutOfRangeCoordinates(t *testing.T) {
	// Proves chk_latitude_range / chk_longitude_range hold at the DB layer.
	_, queries := setupDB(t)
	users := store.NewUserRepository(queries)
	locations := store.NewLocationRepository(queries)
	ctx := context.Background()

	user, err := users.UpsertByAuth0Subject(ctx, "auth0|1", "a@example.com", "A", "B")
	require.NoError(t, err)

	_, err = locations.Upsert(ctx, user.ID, 999, 0)
	assert.Error(t, err)

	_, err = locations.Upsert(ctx, user.ID, 0, 999)
	assert.Error(t, err)
}

func TestLocationRepository_GetTrackedLocations_IncludesUsersWithoutLocation(t *testing.T) {
	_, queries := setupDB(t)
	users := store.NewUserRepository(queries)
	locations := store.NewLocationRepository(queries)
	tracking := store.NewTrackingRepository(queries)
	ctx := context.Background()

	tracker, err := users.UpsertByAuth0Subject(ctx, "auth0|tracker", "tracker@example.com", "T", "T")
	require.NoError(t, err)
	hasLoc, err := users.UpsertByAuth0Subject(ctx, "auth0|hasloc", "hasloc@example.com", "H", "H")
	require.NoError(t, err)
	noLoc, err := users.UpsertByAuth0Subject(ctx, "auth0|noloc", "noloc@example.com", "N", "N")
	require.NoError(t, err)

	_, err = locations.Upsert(ctx, hasLoc.ID, 50.1, 14.4)
	require.NoError(t, err)

	require.NoError(t, tracking.Add(ctx, tracker.ID, hasLoc.ID))
	require.NoError(t, tracking.Add(ctx, tracker.ID, noLoc.ID))
	require.NoError(t, tracking.Accept(ctx, tracker.ID, hasLoc.ID))
	require.NoError(t, tracking.Accept(ctx, tracker.ID, noLoc.ID))

	result, err := locations.GetTrackedLocations(ctx, tracker.ID)
	require.NoError(t, err)
	require.Len(t, result, 2, "both tracked users must appear regardless of whether they've reported a location")

	byEmail := map[string]int{}
	for i, tl := range result {
		byEmail[tl.User.Email] = i
	}

	withLoc := result[byEmail["hasloc@example.com"]]
	require.NotNil(t, withLoc.Location)
	assert.Equal(t, 50.1, withLoc.Location.Latitude)

	without := result[byEmail["noloc@example.com"]]
	assert.Nil(t, without.Location)
}

func TestLocationRepository_GetTrackedLocations_ExcludesPendingRequests(t *testing.T) {
	_, queries := setupDB(t)
	users := store.NewUserRepository(queries)
	locations := store.NewLocationRepository(queries)
	tracking := store.NewTrackingRepository(queries)
	ctx := context.Background()

	tracker, err := users.UpsertByAuth0Subject(ctx, "auth0|tracker", "tracker@example.com", "T", "T")
	require.NoError(t, err)
	target, err := users.UpsertByAuth0Subject(ctx, "auth0|target", "target@example.com", "X", "Y")
	require.NoError(t, err)

	_, err = locations.Upsert(ctx, target.ID, 50.1, 14.4)
	require.NoError(t, err)
	require.NoError(t, tracking.Add(ctx, tracker.ID, target.ID))

	result, err := locations.GetTrackedLocations(ctx, tracker.ID)
	require.NoError(t, err)
	assert.Empty(t, result, "a pending request must grant no location visibility")
}

func TestLocationRepository_DeletingUserCascadesLocation(t *testing.T) {
	pool, queries := setupDB(t)
	users := store.NewUserRepository(queries)
	locations := store.NewLocationRepository(queries)
	ctx := context.Background()

	user, err := users.UpsertByAuth0Subject(ctx, "auth0|1", "a@example.com", "A", "B")
	require.NoError(t, err)
	_, err = locations.Upsert(ctx, user.ID, 1, 1)
	require.NoError(t, err)

	_, err = pool.Exec(ctx, "DELETE FROM users WHERE id = $1", user.ID)
	require.NoError(t, err)

	var count int
	err = pool.QueryRow(ctx, "SELECT count(*) FROM locations WHERE user_id = $1", user.ID).Scan(&count)
	require.NoError(t, err)
	assert.Equal(t, 0, count, "ON DELETE CASCADE should have removed the location row")
}
