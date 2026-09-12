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

// AddTracking looks up the target user by email and starts tracking them,
// rejecting self-tracking, unknown emails, and duplicate tracking entries.
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

// RemoveTracking is idempotent by nature of the underlying DELETE: removing
// an edge that doesn't exist affects zero rows rather than erroring, so
// there's no "not found" case here to special-case.
func (s *TrackingService) RemoveTracking(ctx context.Context, trackerID, trackedUserID uuid.UUID) error {
	return s.tracking.Remove(ctx, trackerID, trackedUserID)
}
