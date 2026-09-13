package service_test

import (
	"context"
	"testing"

	"github.com/google/uuid"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"

	"github.com/Rauden0/bubutracker-api/internal/domain"
	"github.com/Rauden0/bubutracker-api/internal/service"
)

func TestTrackingService_AddTracking_RejectsUnknownEmail(t *testing.T) {
	users := newFakeUserRepo()
	svc := service.NewTrackingService(users, newFakeTrackingRepo())

	err := svc.AddTracking(context.Background(), uuid.New(), "ghost@example.com")

	assert.ErrorIs(t, err, domain.ErrNotFound)
}

func TestTrackingService_AddTracking_MatchesEmailCaseInsensitively(t *testing.T) {
	users := newFakeUserRepo()
	tracker := domain.User{ID: uuid.New(), Email: "tracker@example.com"}
	target := domain.User{ID: uuid.New(), Email: "target@example.com"}
	users.seed(tracker)
	users.seed(target)

	tracking := newFakeTrackingRepo()
	tracking.users[target.ID] = target
	svc := service.NewTrackingService(users, tracking)

	err := svc.AddTracking(context.Background(), tracker.ID, "Target@Example.COM")

	assert.NoError(t, err)
}

func TestTrackingService_AddTracking_RejectsSelfTracking(t *testing.T) {
	users := newFakeUserRepo()
	me := domain.User{ID: uuid.New(), Email: "me@example.com"}
	users.seed(me)
	svc := service.NewTrackingService(users, newFakeTrackingRepo())

	err := svc.AddTracking(context.Background(), me.ID, "me@example.com")

	assert.ErrorIs(t, err, domain.ErrSelfTracking)
}

func TestTrackingService_AddTracking_RejectsDuplicate(t *testing.T) {
	users := newFakeUserRepo()
	tracker := domain.User{ID: uuid.New(), Email: "tracker@example.com"}
	target := domain.User{ID: uuid.New(), Email: "target@example.com"}
	users.seed(tracker)
	users.seed(target)

	tracking := newFakeTrackingRepo()
	tracking.users[target.ID] = target
	svc := service.NewTrackingService(users, tracking)

	require.NoError(t, svc.AddTracking(context.Background(), tracker.ID, target.Email))

	err := svc.AddTracking(context.Background(), tracker.ID, target.Email)
	assert.ErrorIs(t, err, domain.ErrAlreadyExists)
}

func TestTrackingService_AddTracking_CreatesPendingRequestNotVisibleYet(t *testing.T) {
	users := newFakeUserRepo()
	tracker := domain.User{ID: uuid.New(), Email: "tracker@example.com"}
	target := domain.User{ID: uuid.New(), Email: "target@example.com"}
	users.seed(tracker)
	users.seed(target)

	tracking := newFakeTrackingRepo()
	tracking.users[target.ID] = target
	tracking.users[tracker.ID] = tracker
	svc := service.NewTrackingService(users, tracking)

	require.NoError(t, svc.AddTracking(context.Background(), tracker.ID, target.Email))

	tracked, err := svc.ListTracked(context.Background(), tracker.ID)
	require.NoError(t, err)
	assert.Empty(t, tracked, "a pending request must not grant tracking visibility yet")

	requests, err := svc.ListIncomingRequests(context.Background(), target.ID)
	require.NoError(t, err)
	require.Len(t, requests, 1)
	assert.Equal(t, tracker.Email, requests[0].Email)
}

func TestTrackingService_AcceptTracking_GrantsVisibility(t *testing.T) {
	users := newFakeUserRepo()
	tracker := domain.User{ID: uuid.New(), Email: "tracker@example.com"}
	target := domain.User{ID: uuid.New(), Email: "target@example.com"}
	users.seed(tracker)
	users.seed(target)

	tracking := newFakeTrackingRepo()
	tracking.users[target.ID] = target
	tracking.users[tracker.ID] = tracker
	svc := service.NewTrackingService(users, tracking)
	require.NoError(t, svc.AddTracking(context.Background(), tracker.ID, target.Email))

	require.NoError(t, svc.AcceptTracking(context.Background(), target.ID, tracker.ID))

	tracked, err := svc.ListTracked(context.Background(), tracker.ID)
	require.NoError(t, err)
	require.Len(t, tracked, 1)
	assert.Equal(t, target.Email, tracked[0].Email)

	followers, err := svc.ListFollowers(context.Background(), target.ID)
	require.NoError(t, err)
	require.Len(t, followers, 1)
	assert.Equal(t, tracker.Email, followers[0].Email)

	requests, err := svc.ListIncomingRequests(context.Background(), target.ID)
	require.NoError(t, err)
	assert.Empty(t, requests, "an accepted request must no longer appear as pending")
}

func TestTrackingService_AcceptTracking_RejectsWhenNoPendingRequest(t *testing.T) {
	svc := service.NewTrackingService(newFakeUserRepo(), newFakeTrackingRepo())

	err := svc.AcceptTracking(context.Background(), uuid.New(), uuid.New())

	assert.ErrorIs(t, err, domain.ErrNotFound)
}

func TestTrackingService_RemoveTracking_IsIdempotent(t *testing.T) {
	svc := service.NewTrackingService(newFakeUserRepo(), newFakeTrackingRepo())

	err := svc.RemoveTracking(context.Background(), uuid.New(), uuid.New())

	assert.NoError(t, err)
}

func TestTrackingService_RemoveTracking_RejectsAPendingRequest(t *testing.T) {
	users := newFakeUserRepo()
	tracker := domain.User{ID: uuid.New(), Email: "tracker@example.com"}
	target := domain.User{ID: uuid.New(), Email: "target@example.com"}
	users.seed(tracker)
	users.seed(target)

	tracking := newFakeTrackingRepo()
	tracking.users[target.ID] = target
	tracking.users[tracker.ID] = tracker
	svc := service.NewTrackingService(users, tracking)
	require.NoError(t, svc.AddTracking(context.Background(), tracker.ID, target.Email))

	// The tracked user rejects by removing the edge from their side.
	require.NoError(t, svc.RemoveTracking(context.Background(), tracker.ID, target.ID))

	requests, err := svc.ListIncomingRequests(context.Background(), target.ID)
	require.NoError(t, err)
	assert.Empty(t, requests)

	// Rejected, not blocked forever: the tracker can request again.
	assert.NoError(t, svc.AddTracking(context.Background(), tracker.ID, target.Email))
}
