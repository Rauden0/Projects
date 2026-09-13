package service

import (
	"context"
	"fmt"

	"github.com/google/uuid"

	"github.com/Rauden0/bubutracker-api/internal/domain"
)

type TrackingService struct {
	users    UserRepository
	tracking TrackingRepository
}

func NewTrackingService(users UserRepository, tracking TrackingRepository) *TrackingService {
	return &TrackingService{users: users, tracking: tracking}
}

func (s *TrackingService) ListTracked(ctx context.Context, trackerID uuid.UUID) ([]domain.User, error) {
	return s.tracking.GetTrackedUsers(ctx, trackerID)
}

// ListIncomingRequests lists other users' pending requests to track the
// current user - the consent inbox they accept or reject from.
func (s *TrackingService) ListIncomingRequests(ctx context.Context, trackedUserID uuid.UUID) ([]domain.User, error) {
	return s.tracking.GetIncomingRequests(ctx, trackedUserID)
}

// ListFollowers lists users currently, with consent, tracking the current
// user, so they have ongoing visibility into (and can revoke) who has
// access, not just at request time.
func (s *TrackingService) ListFollowers(ctx context.Context, trackedUserID uuid.UUID) ([]domain.User, error) {
	return s.tracking.GetFollowers(ctx, trackedUserID)
}

// AddTracking looks up the target user by email and files a pending request
// to track them, rejecting self-tracking, unknown emails, and a duplicate
// request/relationship (pending or already accepted). The target must
// accept the request (AcceptTracking) before it grants any visibility.
func (s *TrackingService) AddTracking(ctx context.Context, trackerID uuid.UUID, email string) error {
	email = normalizeEmail(email)
	if email == "" {
		return fmt.Errorf("%w: email is required", domain.ErrInvalidArgument)
	}

	tracked, err := s.users.GetByEmail(ctx, email)
	if err != nil {
		return err // domain.ErrNotFound propagates as-is
	}

	if tracked.ID == trackerID {
		return domain.ErrSelfTracking
	}

	alreadyTracking, err := s.tracking.Get(ctx, trackerID, tracked.ID)
	if err != nil {
		return err
	}
	if alreadyTracking {
		return domain.ErrAlreadyExists
	}

	return s.tracking.Add(ctx, trackerID, tracked.ID)
}

// AcceptTracking lets trackedUserID (the current user) approve a pending
// request from trackerID, granting them visibility into trackedUserID's
// location. Returns domain.ErrNotFound if there's no such pending request.
func (s *TrackingService) AcceptTracking(ctx context.Context, trackedUserID, trackerID uuid.UUID) error {
	return s.tracking.Accept(ctx, trackerID, trackedUserID)
}

// RemoveTracking deletes the (tracker, trackedUserID) edge regardless of
// its status. It serves four call sites - the tracker canceling their own
// pending request or stopping active tracking, and the tracked user
// rejecting a pending request or revoking consent already given - all of
// which reduce to "this edge shouldn't exist anymore". It's idempotent by
// nature of the underlying DELETE: removing an edge that doesn't exist
// affects zero rows rather than erroring, so there's no "not found" case to
// special-case for any of them.
func (s *TrackingService) RemoveTracking(ctx context.Context, trackerID, trackedUserID uuid.UUID) error {
	return s.tracking.Remove(ctx, trackerID, trackedUserID)
}
