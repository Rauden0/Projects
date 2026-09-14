package service

import (
	"context"
	"errors"
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

func (s *TrackingService) ListIncomingRequests(ctx context.Context, trackedUserID uuid.UUID) ([]domain.User, error) {
	return s.tracking.GetIncomingRequests(ctx, trackedUserID)
}

func (s *TrackingService) ListOutgoingRequests(ctx context.Context, trackerID uuid.UUID) ([]domain.User, error) {
	return s.tracking.GetOutgoingRequests(ctx, trackerID)
}

func (s *TrackingService) ListFollowers(ctx context.Context, trackedUserID uuid.UUID) ([]domain.User, error) {
	return s.tracking.GetFollowers(ctx, trackedUserID)
}

func (s *TrackingService) AddTracking(ctx context.Context, trackerID uuid.UUID, email string) error {
	email = normalizeEmail(email)
	if email == "" {
		return fmt.Errorf("%w: email is required", domain.ErrInvalidArgument)
	}

	tracked, err := s.users.GetByEmail(ctx, email)
	if err != nil {
		if errors.Is(err, domain.ErrNotFound) {
			// Silently no-op instead of surfacing not-found: an HTTP-visible
			// difference between "no such account" and "request sent" would let
			// a caller enumerate registered emails by probing this endpoint.
			return nil
		}
		return err
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

func (s *TrackingService) AcceptTracking(ctx context.Context, trackedUserID, trackerID uuid.UUID) error {
	return s.tracking.Accept(ctx, trackerID, trackedUserID)
}

func (s *TrackingService) RemoveTracking(ctx context.Context, trackerID, trackedUserID uuid.UUID) error {
	return s.tracking.Remove(ctx, trackerID, trackedUserID)
}
