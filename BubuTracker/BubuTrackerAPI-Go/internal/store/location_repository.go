package store

import (
	"context"

	"github.com/google/uuid"

	"github.com/Rauden0/bubutracker-api/internal/domain"
	"github.com/Rauden0/bubutracker-api/internal/store/sqlc"
)

// LocationRepository implements service.LocationRepository backed by Postgres.
type LocationRepository struct {
	q *sqlc.Queries
}

func NewLocationRepository(q *sqlc.Queries) *LocationRepository {
	return &LocationRepository{q: q}
}

func (r *LocationRepository) Upsert(ctx context.Context, userID uuid.UUID, latitude, longitude float64) (domain.Location, error) {
	l, err := r.q.UpsertLocation(ctx, sqlc.UpsertLocationParams{
		UserID:    userID,
		Latitude:  latitude,
		Longitude: longitude,
	})
	if err != nil {
		return domain.Location{}, mapError(err)
	}
	return toDomainLocation(l), nil
}

func (r *LocationRepository) GetTrackedLocations(ctx context.Context, trackerID uuid.UUID) ([]domain.TrackedLocation, error) {
	rows, err := r.q.GetTrackedLocations(ctx, trackerID)
	if err != nil {
		return nil, mapError(err)
	}

	result := make([]domain.TrackedLocation, len(rows))
	for i, row := range rows {
		result[i] = domain.TrackedLocation{
			User: domain.User{
				ID:        row.UserID,
				Email:     row.Email,
				FirstName: row.FirstName,
				LastName:  row.LastName,
			},
			Location: domain.Location{
				UserID:    row.UserID,
				Latitude:  row.Latitude,
				Longitude: row.Longitude,
				UpdatedAt: row.UpdatedAt.Time,
			},
		}
	}
	return result, nil
}
