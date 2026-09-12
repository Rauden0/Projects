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

// UserSummary is the partial profile projection returned alongside a
// tracked user's location. It's a distinct type from User (rather than a
// partially-populated User) so a future field added to User can't silently
// end up zero-valued here by accident.
type UserSummary struct {
	ID        uuid.UUID
	Email     string
	FirstName string
	LastName  string
}

// TrackedLocation is a tracked user's location joined with their profile, as
// returned to a tracker. Location is nil if that user hasn't reported a
// location yet — they still appear (matching GetTrackedUsers) rather than
// silently vanishing from the list.
type TrackedLocation struct {
	User     UserSummary
	Location *Location
}
