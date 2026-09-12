//go:build integration

package store_test

import (
	"context"
	"testing"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"

	"github.com/Rauden0/bubutracker-api/internal/domain"
	"github.com/Rauden0/bubutracker-api/internal/store"
)

func TestTrackingRepository_AddThenGet(t *testing.T) {
	_, queries := setupDB(t)
	users := store.NewUserRepository(queries)
	tracking := store.NewTrackingRepository(queries)
	ctx := context.Background()

	tracker, err := users.UpsertByAuth0Subject(ctx, "auth0|1", "tracker@example.com", "T", "T")
	require.NoError(t, err)
	target, err := users.UpsertByAuth0Subject(ctx, "auth0|2", "target@example.com", "X", "Y")
	require.NoError(t, err)

	exists, err := tracking.Get(ctx, tracker.ID, target.ID)
	require.NoError(t, err)
	assert.False(t, exists)

	require.NoError(t, tracking.Add(ctx, tracker.ID, target.ID))

	exists, err = tracking.Get(ctx, tracker.ID, target.ID)
	require.NoError(t, err)
	assert.True(t, exists)
}

func TestTrackingRepository_Add_DuplicateReturnsAlreadyExists(t *testing.T) {
	_, queries := setupDB(t)
	users := store.NewUserRepository(queries)
	tracking := store.NewTrackingRepository(queries)
	ctx := context.Background()

	tracker, err := users.UpsertByAuth0Subject(ctx, "auth0|1", "tracker@example.com", "T", "T")
	require.NoError(t, err)
	target, err := users.UpsertByAuth0Subject(ctx, "auth0|2", "target@example.com", "X", "Y")
	require.NoError(t, err)

	require.NoError(t, tracking.Add(ctx, tracker.ID, target.ID))

	err = tracking.Add(ctx, tracker.ID, target.ID)

	assert.ErrorIs(t, err, domain.ErrAlreadyExists)
}

func TestTrackingRepository_Add_SelfTrackingRejectedByConstraint(t *testing.T) {
	// The service layer already rejects self-tracking with a clear error;
	// this proves the chk_no_self_tracking CHECK constraint holds too, as a
	// second line of defense.
	_, queries := setupDB(t)
	users := store.NewUserRepository(queries)
	tracking := store.NewTrackingRepository(queries)
	ctx := context.Background()

	user, err := users.UpsertByAuth0Subject(ctx, "auth0|1", "solo@example.com", "S", "S")
	require.NoError(t, err)

	err = tracking.Add(ctx, user.ID, user.ID)

	assert.Error(t, err)
}

func TestTrackingRepository_Remove_IsIdempotent(t *testing.T) {
	_, queries := setupDB(t)
	users := store.NewUserRepository(queries)
	tracking := store.NewTrackingRepository(queries)
	ctx := context.Background()

	tracker, err := users.UpsertByAuth0Subject(ctx, "auth0|1", "tracker@example.com", "T", "T")
	require.NoError(t, err)
	target, err := users.UpsertByAuth0Subject(ctx, "auth0|2", "target@example.com", "X", "Y")
	require.NoError(t, err)

	// Removing an edge that was never added must not error.
	require.NoError(t, tracking.Remove(ctx, tracker.ID, target.ID))

	require.NoError(t, tracking.Add(ctx, tracker.ID, target.ID))
	require.NoError(t, tracking.Remove(ctx, tracker.ID, target.ID))
	require.NoError(t, tracking.Remove(ctx, tracker.ID, target.ID), "removing twice must still not error")

	exists, err := tracking.Get(ctx, tracker.ID, target.ID)
	require.NoError(t, err)
	assert.False(t, exists)
}

func TestTrackingRepository_GetTrackedUsers_OrderedByEmail(t *testing.T) {
	_, queries := setupDB(t)
	users := store.NewUserRepository(queries)
	tracking := store.NewTrackingRepository(queries)
	ctx := context.Background()

	tracker, err := users.UpsertByAuth0Subject(ctx, "auth0|1", "tracker@example.com", "T", "T")
	require.NoError(t, err)
	zed, err := users.UpsertByAuth0Subject(ctx, "auth0|z", "zed@example.com", "Z", "Z")
	require.NoError(t, err)
	amy, err := users.UpsertByAuth0Subject(ctx, "auth0|a", "amy@example.com", "A", "A")
	require.NoError(t, err)

	require.NoError(t, tracking.Add(ctx, tracker.ID, zed.ID))
	require.NoError(t, tracking.Add(ctx, tracker.ID, amy.ID))

	result, err := tracking.GetTrackedUsers(ctx, tracker.ID)

	require.NoError(t, err)
	require.Len(t, result, 2)
	assert.Equal(t, "amy@example.com", result[0].Email)
	assert.Equal(t, "zed@example.com", result[1].Email)
}
