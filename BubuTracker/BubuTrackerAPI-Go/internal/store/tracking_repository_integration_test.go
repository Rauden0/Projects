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
	require.NoError(t, tracking.Accept(ctx, tracker.ID, zed.ID))
	require.NoError(t, tracking.Accept(ctx, tracker.ID, amy.ID))

	result, err := tracking.GetTrackedUsers(ctx, tracker.ID)

	require.NoError(t, err)
	require.Len(t, result, 2)
	assert.Equal(t, "amy@example.com", result[0].Email)
	assert.Equal(t, "zed@example.com", result[1].Email)
}

// TestTrackingRepository_PendingRequestGrantsNoVisibilityUntilAccepted is
// the direct regression test for the consent model: Add alone must not
// make a tracked user show up in GetTrackedUsers, and Accept is what
// transitions it - covering both directions plus the request/follower
// inboxes each side sees.
func TestTrackingRepository_PendingRequestGrantsNoVisibilityUntilAccepted(t *testing.T) {
	_, queries := setupDB(t)
	users := store.NewUserRepository(queries)
	tracking := store.NewTrackingRepository(queries)
	ctx := context.Background()

	tracker, err := users.UpsertByAuth0Subject(ctx, "auth0|1", "tracker@example.com", "T", "T")
	require.NoError(t, err)
	target, err := users.UpsertByAuth0Subject(ctx, "auth0|2", "target@example.com", "X", "Y")
	require.NoError(t, err)

	require.NoError(t, tracking.Add(ctx, tracker.ID, target.ID))

	tracked, err := tracking.GetTrackedUsers(ctx, tracker.ID)
	require.NoError(t, err)
	assert.Empty(t, tracked, "a pending request must not grant tracking visibility")

	incoming, err := tracking.GetIncomingRequests(ctx, target.ID)
	require.NoError(t, err)
	require.Len(t, incoming, 1)
	assert.Equal(t, "tracker@example.com", incoming[0].Email)

	require.NoError(t, tracking.Accept(ctx, tracker.ID, target.ID))

	tracked, err = tracking.GetTrackedUsers(ctx, tracker.ID)
	require.NoError(t, err)
	require.Len(t, tracked, 1)
	assert.Equal(t, "target@example.com", tracked[0].Email)

	followers, err := tracking.GetFollowers(ctx, target.ID)
	require.NoError(t, err)
	require.Len(t, followers, 1)
	assert.Equal(t, "tracker@example.com", followers[0].Email)

	incoming, err = tracking.GetIncomingRequests(ctx, target.ID)
	require.NoError(t, err)
	assert.Empty(t, incoming, "an accepted request must no longer appear as pending")
}

func TestTrackingRepository_Accept_ReturnsNotFoundWhenNoPendingRequest(t *testing.T) {
	_, queries := setupDB(t)
	users := store.NewUserRepository(queries)
	tracking := store.NewTrackingRepository(queries)
	ctx := context.Background()

	tracker, err := users.UpsertByAuth0Subject(ctx, "auth0|1", "tracker@example.com", "T", "T")
	require.NoError(t, err)
	target, err := users.UpsertByAuth0Subject(ctx, "auth0|2", "target@example.com", "X", "Y")
	require.NoError(t, err)

	err = tracking.Accept(ctx, tracker.ID, target.ID)

	assert.ErrorIs(t, err, domain.ErrNotFound)
}
