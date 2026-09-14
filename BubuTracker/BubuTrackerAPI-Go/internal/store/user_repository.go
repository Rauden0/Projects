package store

import (
	"context"

	"github.com/google/uuid"

	"github.com/Rauden0/bubutracker-api/internal/domain"
	"github.com/Rauden0/bubutracker-api/internal/store/sqlc"
)

type UserRepository struct {
	q *sqlc.Queries
}

func NewUserRepository(q *sqlc.Queries) *UserRepository {
	return &UserRepository{q: q}
}

func (r *UserRepository) GetByID(ctx context.Context, id uuid.UUID) (domain.User, error) {
	u, err := r.q.GetUserByID(ctx, id)
	if err != nil {
		return domain.User{}, mapError(err)
	}
	return toDomainUser(u), nil
}

func (r *UserRepository) GetByAuth0SubjectID(ctx context.Context, subjectID string) (domain.User, error) {
	u, err := r.q.GetUserByAuth0SubjectID(ctx, subjectID)
	if err != nil {
		return domain.User{}, mapError(err)
	}
	return toDomainUser(u), nil
}

func (r *UserRepository) GetByEmail(ctx context.Context, email string) (domain.User, error) {
	u, err := r.q.GetUserByEmail(ctx, email)
	if err != nil {
		return domain.User{}, mapError(err)
	}
	return toDomainUser(u), nil
}

func (r *UserRepository) UpsertByAuth0Subject(ctx context.Context, subjectID, email, firstName, lastName string) (domain.User, error) {
	upserted, err := r.q.UpsertUserByAuth0Subject(ctx, sqlc.UpsertUserByAuth0SubjectParams{
		Auth0SubjectID: subjectID,
		Email:          email,
		FirstName:      firstName,
		LastName:       lastName,
	})
	if err != nil {
		return domain.User{}, mapUpsertUserError(err)
	}
	return toDomainUser(upserted), nil
}

func (r *UserRepository) UpdateProfile(ctx context.Context, id uuid.UUID, firstName, lastName, markerColor *string) (domain.User, error) {
	updated, err := r.q.UpdateUserProfile(ctx, sqlc.UpdateUserProfileParams{
		ID:          id,
		FirstName:   firstName,
		LastName:    lastName,
		MarkerColor: markerColor,
	})
	if err != nil {
		return domain.User{}, mapError(err)
	}
	return toDomainUser(updated), nil
}

func (r *UserRepository) Delete(ctx context.Context, id uuid.UUID) error {
	rows, err := r.q.DeleteUser(ctx, id)
	if err != nil {
		return mapError(err)
	}
	if rows == 0 {
		return domain.ErrNotFound
	}
	return nil
}
