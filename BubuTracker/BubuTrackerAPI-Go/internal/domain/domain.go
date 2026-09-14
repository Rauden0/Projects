package domain

import (
	"time"

	"github.com/google/uuid"
)

type User struct {
	ID             uuid.UUID
	Auth0SubjectID string
	Email          string
	FirstName      string
	LastName       string
	MarkerColor    string
	CreatedAt      time.Time
}

type Location struct {
	UserID    uuid.UUID
	Latitude  float64
	Longitude float64
	UpdatedAt time.Time
}

type UserSummary struct {
	ID          uuid.UUID
	Email       string
	FirstName   string
	LastName    string
	MarkerColor string
}

type TrackedLocation struct {
	User     UserSummary
	Location *Location
}
