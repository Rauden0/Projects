package service

import (
	"context"

	"github.com/google/uuid"

	"github.com/Rauden0/bubutracker-api/internal/domain"
)

type UserRepository interface {
	GetByID(ctx context.Context, id uuid.UUID) (domain.User, error)
	GetByAuth0SubjectID(ctx context.Context, subjectID string) (domain.User, error)
	GetByEmail(ctx context.Context, email string) (domain.User, error)
	UpsertByAuth0Subject(ctx context.Context, subjectID, email, firstName, lastName string) (domain.User, error)
	UpdateProfile(ctx context.Context, id uuid.UUID, firstName, lastName, markerColor *string) (domain.User, error)
	Delete(ctx context.Context, id uuid.UUID) error
}

type LocationRepository interface {
	Upsert(ctx context.Context, userID uuid.UUID, latitude, longitude float64) (domain.Location, error)
	GetTrackedLocations(ctx context.Context, trackerID uuid.UUID) ([]domain.TrackedLocation, error)
}

type TrackingRepository interface {
	Get(ctx context.Context, trackerID, trackedUserID uuid.UUID) (bool, error)
	GetTrackedUsers(ctx context.Context, trackerID uuid.UUID) ([]domain.User, error)
	GetIncomingRequests(ctx context.Context, trackedUserID uuid.UUID) ([]domain.User, error)
	GetOutgoingRequests(ctx context.Context, trackerID uuid.UUID) ([]domain.User, error)
	GetFollowers(ctx context.Context, trackedUserID uuid.UUID) ([]domain.User, error)
	Add(ctx context.Context, trackerID, trackedUserID uuid.UUID) error
	Accept(ctx context.Context, trackerID, trackedUserID uuid.UUID) error
	Remove(ctx context.Context, trackerID, trackedUserID uuid.UUID) error
}
