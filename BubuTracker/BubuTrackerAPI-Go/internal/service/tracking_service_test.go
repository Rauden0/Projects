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

func TestTrackingService_AddTracking_Succeeds(t *testing.T) {
	users := newFakeUserRepo()
	tracker := domain.User{ID: uuid.New(), Email: "tracker@example.com"}
	target := domain.User{ID: uuid.New(), Email: "target@example.com"}
	users.seed(tracker)
	users.seed(target)

	tracking := newFakeTrackingRepo()
	tracking.users[target.ID] = target
	svc := service.NewTrackingService(users, tracking)

	require.NoError(t, svc.AddTracking(context.Background(), tracker.ID, target.Email))

	tracked, err := svc.ListTracked(context.Background(), tracker.ID)
	require.NoError(t, err)
	require.Len(t, tracked, 1)
	assert.Equal(t, target.Email, tracked[0].Email)
}

func TestTrackingService_RemoveTracking_IsIdempotent(t *testing.T) {
	svc := service.NewTrackingService(newFakeUserRepo(), newFakeTrackingRepo())

	err := svc.RemoveTracking(context.Background(), uuid.New(), uuid.New())

	assert.NoError(t, err)
}
