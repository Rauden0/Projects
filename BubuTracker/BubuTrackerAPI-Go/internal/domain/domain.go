// Package domain holds the core business types shared across service and
// handler layers. It has no dependency on HTTP, SQL, or any framework.
package domain

import (
	"time"

	"github.com/google/uuid"
)

// User is an account, provisioned lazily from an authenticated Auth0 subject.
type User struct {
	ID             uuid.UUID
	Auth0SubjectID string
	Email          string
	FirstName      string
	LastName       string
	CreatedAt      time.Time
}

// Location is a user's most recently reported position.
type Location struct {
	UserID    uuid.UUID
	Latitude  float64
	Longitude float64
	UpdatedAt time.Time
}

// TrackedLocation is a tracked user's location joined with their profile,
// as returned to a tracker.
type TrackedLocation struct {
	User     User
	Location Location
}
