package store

import (
	"context"
	"errors"

	"github.com/google/uuid"

	"github.com/Rauden0/bubutracker-api/internal/domain"
	"github.com/Rauden0/bubutracker-api/internal/store/sqlc"
)

// TrackingRepository implements service.TrackingRepository backed by Postgres.
type TrackingRepository struct {
	q *sqlc.Queries
}

func NewTrackingRepository(q *sqlc.Queries) *TrackingRepository {
	return &TrackingRepository{q: q}
}

func (r *TrackingRepository) Get(ctx context.Context, trackerID, trackedUserID uuid.UUID) (bool, error) {
	_, err := r.q.GetTracking(ctx, sqlc.GetTrackingParams{
		TrackerID:     trackerID,
		TrackedUserID: trackedUserID,
	})
	if err != nil {
		if errors.Is(mapError(err), domain.ErrNotFound) {
			return false, nil
		}
		return false, mapError(err)
	}
	return true, nil
}

func (r *TrackingRepository) GetTrackedUsers(ctx context.Context, trackerID uuid.UUID) ([]domain.User, error) {
	users, err := r.q.GetTrackedUsers(ctx, trackerID)
	if err != nil {
		return nil, mapError(err)
	}
	return toDomainUsers(users), nil
}

func (r *TrackingRepository) GetIncomingRequests(ctx context.Context, trackedUserID uuid.UUID) ([]domain.User, error) {
	users, err := r.q.GetIncomingTrackingRequests(ctx, trackedUserID)
	if err != nil {
		return nil, mapError(err)
	}
	return toDomainUsers(users), nil
}

func (r *TrackingRepository) GetFollowers(ctx context.Context, trackedUserID uuid.UUID) ([]domain.User, error) {
	users, err := r.q.GetFollowers(ctx, trackedUserID)
	if err != nil {
		return nil, mapError(err)
	}
	return toDomainUsers(users), nil
}

func (r *TrackingRepository) Add(ctx context.Context, trackerID, trackedUserID uuid.UUID) error {
	err := r.q.AddTracking(ctx, sqlc.AddTrackingParams{
		TrackerID:     trackerID,
		TrackedUserID: trackedUserID,
	})
	return mapError(err)
}

// Accept transitions a pending request to accepted, returning
// domain.ErrNotFound if there was no such pending request (already
// accepted, rejected, or never existed) - unlike Remove, this is a
// meaningful state transition the caller needs clear feedback on, not a
// no-op-safe deletion.
func (r *TrackingRepository) Accept(ctx context.Context, trackerID, trackedUserID uuid.UUID) error {
	rows, err := r.q.AcceptTracking(ctx, sqlc.AcceptTrackingParams{
		TrackerID:     trackerID,
		TrackedUserID: trackedUserID,
	})
	if err != nil {
		return mapError(err)
	}
	if rows == 0 {
		return domain.ErrNotFound
	}
	return nil
}

func (r *TrackingRepository) Remove(ctx context.Context, trackerID, trackedUserID uuid.UUID) error {
	err := r.q.RemoveTracking(ctx, sqlc.RemoveTrackingParams{
		TrackerID:     trackerID,
		TrackedUserID: trackedUserID,
	})
	return mapError(err)
}
