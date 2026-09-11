package store

import (
	"errors"

	"github.com/jackc/pgx/v5"
	"github.com/jackc/pgx/v5/pgconn"

	"github.com/Rauden0/bubutracker-api/internal/domain"
	"github.com/Rauden0/bubutracker-api/internal/store/sqlc"
)

// postgres error codes we branch on. See:
// https://www.postgresql.org/docs/current/errcodes-appendix.html
const pgUniqueViolation = "23505"

// mapError normalizes pgx/postgres errors into the sentinel domain errors
// the service layer expects, so callers never need to know about SQL.
func mapError(err error) error {
	if err == nil {
		return nil
	}
	if errors.Is(err, pgx.ErrNoRows) {
		return domain.ErrNotFound
	}
	var pgErr *pgconn.PgError
	if errors.As(err, &pgErr) && pgErr.Code == pgUniqueViolation {
		return domain.ErrAlreadyExists
	}
	return err
}

func toDomainUser(u sqlc.User) domain.User {
	return domain.User{
		ID:             u.ID,
		Auth0SubjectID: u.Auth0SubjectID,
		Email:          u.Email,
		FirstName:      u.FirstName,
		LastName:       u.LastName,
		CreatedAt:      u.CreatedAt.Time,
	}
}

func toDomainUsers(users []sqlc.User) []domain.User {
	result := make([]domain.User, len(users))
	for i, u := range users {
		result[i] = toDomainUser(u)
	}
	return result
}

func toDomainLocation(l sqlc.Location) domain.Location {
	return domain.Location{
		UserID:    l.UserID,
		Latitude:  l.Latitude,
		Longitude: l.Longitude,
		UpdatedAt: l.UpdatedAt.Time,
	}
}
