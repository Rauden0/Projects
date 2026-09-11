package service

import (
	"context"

	"github.com/google/uuid"

	"github.com/Rauden0/bubutracker-api/internal/domain"
)

// The service layer depends only on these narrow interfaces, never on the
// concrete Postgres/sqlc implementation. That keeps business logic testable
// with in-memory fakes and keeps the storage engine swappable.

type UserRepository interface {
	GetByID(ctx context.Context, id uuid.UUID) (domain.User, error)
	GetByAuth0SubjectID(ctx context.Context, subjectID string) (domain.User, error)
	GetByEmail(ctx context.Context, email string) (domain.User, error)
	Create(ctx context.Context, u domain.User) (domain.User, error)
	Update(ctx context.Context, u domain.User) (domain.User, error)
}

type LocationRepository interface {
	Upsert(ctx context.Context, userID uuid.UUID, latitude, longitude float64) (domain.Location, error)
	GetTrackedLocations(ctx context.Context, trackerID uuid.UUID) ([]domain.TrackedLocation, error)
}

type TrackingRepository interface {
	Get(ctx context.Context, trackerID, trackedUserID uuid.UUID) (bool, error)
	GetTrackedUsers(ctx context.Context, trackerID uuid.UUID) ([]domain.User, error)
	Add(ctx context.Context, trackerID, trackedUserID uuid.UUID) error
	Remove(ctx context.Context, trackerID, trackedUserID uuid.UUID) error
}
