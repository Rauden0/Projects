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
	// UpsertByAuth0Subject creates the user on first sign-in or converges
	// its cached email otherwise, atomically. Safe under concurrent calls
	// for the same subject.
	UpsertByAuth0Subject(ctx context.Context, subjectID, email, firstName, lastName string) (domain.User, error)
	// UpdateProfile applies a partial update in one atomic statement: a nil
	// field leaves the existing column value untouched.
	UpdateProfile(ctx context.Context, id uuid.UUID, firstName, lastName *string) (domain.User, error)
}

type LocationRepository interface {
	Upsert(ctx context.Context, userID uuid.UUID, latitude, longitude float64) (domain.Location, error)
	GetTrackedLocations(ctx context.Context, trackerID uuid.UUID) ([]domain.TrackedLocation, error)
}

type TrackingRepository interface {
	Get(ctx context.Context, trackerID, trackedUserID uuid.UUID) (bool, error)
	GetTrackedUsers(ctx context.Context, trackerID uuid.UUID) ([]domain.User, error)
	// GetIncomingRequests lists other users' pending requests to track
	// trackedUserID - the consent inbox they accept or reject from.
	GetIncomingRequests(ctx context.Context, trackedUserID uuid.UUID) ([]domain.User, error)
	// GetFollowers lists users currently, with consent, tracking
	// trackedUserID, so they have ongoing visibility into who has access.
	GetFollowers(ctx context.Context, trackedUserID uuid.UUID) ([]domain.User, error)
	// Add creates a pending request; the tracked user must Accept it before
	// the tracker gets any location visibility.
	Add(ctx context.Context, trackerID, trackedUserID uuid.UUID) error
	// Accept transitions a pending request to accepted. Returns
	// domain.ErrNotFound if there was no such pending request.
	Accept(ctx context.Context, trackerID, trackedUserID uuid.UUID) error
	// Remove deletes the edge regardless of status: used both for the
	// tracker canceling/stopping tracking, and the tracked user
	// rejecting/revoking it.
	Remove(ctx context.Context, trackerID, trackedUserID uuid.UUID) error
}
