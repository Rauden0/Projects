package store

import (
	"context"

	"github.com/google/uuid"

	"github.com/Rauden0/bubutracker-api/internal/domain"
	"github.com/Rauden0/bubutracker-api/internal/store/sqlc"
)

// UserRepository implements service.UserRepository backed by Postgres.
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

func (r *UserRepository) Create(ctx context.Context, u domain.User) (domain.User, error) {
	created, err := r.q.CreateUser(ctx, sqlc.CreateUserParams{
		Auth0SubjectID: u.Auth0SubjectID,
		Email:          u.Email,
		FirstName:      u.FirstName,
		LastName:       u.LastName,
	})
	if err != nil {
		return domain.User{}, mapError(err)
	}
	return toDomainUser(created), nil
}

func (r *UserRepository) Update(ctx context.Context, u domain.User) (domain.User, error) {
	updated, err := r.q.UpdateUser(ctx, sqlc.UpdateUserParams{
		ID:        u.ID,
		Email:     u.Email,
		FirstName: u.FirstName,
		LastName:  u.LastName,
	})
	if err != nil {
		return domain.User{}, mapError(err)
	}
	return toDomainUser(updated), nil
}
