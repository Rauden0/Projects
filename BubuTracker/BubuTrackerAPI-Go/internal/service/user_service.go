package service

import (
	"context"
	"errors"
	"fmt"
	"strings"

	"github.com/google/uuid"

	"github.com/Rauden0/bubutracker-api/internal/domain"
)

type UserService struct {
	users UserRepository
}

func NewUserService(users UserRepository) *UserService {
	return &UserService{users: users}
}

// GetOrCreateBySubject returns the user for a validated Auth0 subject,
// provisioning a new account on first sign-in and keeping the cached email
// in sync with the identity provider on every call thereafter.
func (s *UserService) GetOrCreateBySubject(ctx context.Context, subjectID, email, firstName, lastName string) (domain.User, error) {
	if subjectID == "" {
		return domain.User{}, domain.ErrUnauthenticated
	}
	if email == "" {
		email = fmt.Sprintf("%s@users.auth0.local", subjectID)
	}

	existing, err := s.users.GetByAuth0SubjectID(ctx, subjectID)
	if err == nil {
		if email != "" && existing.Email != email {
			existing.Email = email
			return s.users.Update(ctx, existing)
		}
		return existing, nil
	}
	if !errors.Is(err, domain.ErrNotFound) {
		return domain.User{}, err
	}

	return s.users.Create(ctx, domain.User{
		ID:             uuid.New(),
		Auth0SubjectID: subjectID,
		Email:          email,
		FirstName:      firstName,
		LastName:       lastName,
	})
}

// UpdateProfile applies a partial update: a nil field leaves the existing
// value untouched, matching PATCH semantics.
func (s *UserService) UpdateProfile(ctx context.Context, userID uuid.UUID, firstName, lastName *string) (domain.User, error) {
	user, err := s.users.GetByID(ctx, userID)
	if err != nil {
		return domain.User{}, err
	}

	if firstName != nil {
		trimmed := strings.TrimSpace(*firstName)
		if trimmed == "" {
			return domain.User{}, fmt.Errorf("%w: firstName cannot be blank", domain.ErrInvalidArgument)
		}
		user.FirstName = trimmed
	}
	if lastName != nil {
		trimmed := strings.TrimSpace(*lastName)
		if trimmed == "" {
			return domain.User{}, fmt.Errorf("%w: lastName cannot be blank", domain.ErrInvalidArgument)
		}
		user.LastName = trimmed
	}

	return s.users.Update(ctx, user)
}
