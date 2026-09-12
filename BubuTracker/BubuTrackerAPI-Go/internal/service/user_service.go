package service

import (
	"context"
	"errors"
	"fmt"
	"strings"

	"github.com/google/uuid"

	"github.com/Rauden0/bubutracker-api/internal/domain"
)

// maxNameLength bounds first/last name input. Without it the only limit is
// the HTTP layer's whole-request-body cap, so a single field could still be
// stored as an almost-1MB string in an otherwise-unbounded text column.
const maxNameLength = 100

type UserService struct {
	users UserRepository
}

func NewUserService(users UserRepository) *UserService {
	return &UserService{users: users}
}

// GetOrCreateBySubject returns the user for a validated Auth0 subject,
// provisioning a new account on first sign-in and keeping the cached email
// in sync with the identity provider thereafter. The fast path is a plain
// read; UpsertByAuth0Subject (a write) only runs when the account doesn't
// exist yet or its cached email has drifted, and is safe under concurrent
// first-sign-in requests for the same subject.
func (s *UserService) GetOrCreateBySubject(ctx context.Context, subjectID, email, firstName, lastName string) (domain.User, error) {
	if subjectID == "" {
		return domain.User{}, domain.ErrUnauthenticated
	}
	email = normalizeEmail(email)
	if email == "" {
		email = normalizeEmail(fmt.Sprintf("%s@users.auth0.local", subjectID))
	}

	existing, err := s.users.GetByAuth0SubjectID(ctx, subjectID)
	if err == nil {
		if existing.Email == email {
			return existing, nil
		}
	} else if !errors.Is(err, domain.ErrNotFound) {
		return domain.User{}, err
	}

	return s.users.UpsertByAuth0Subject(ctx, subjectID, email, firstName, lastName)
}

// UpdateProfile applies a partial update: a nil field leaves the existing
// value untouched, matching PATCH semantics. Validation happens against the
// input alone, so no read is needed before the atomic write.
func (s *UserService) UpdateProfile(ctx context.Context, userID uuid.UUID, firstName, lastName *string) (domain.User, error) {
	firstName, err := trimmedOrError(firstName, "firstName")
	if err != nil {
		return domain.User{}, err
	}
	lastName, err = trimmedOrError(lastName, "lastName")
	if err != nil {
		return domain.User{}, err
	}

	return s.users.UpdateProfile(ctx, userID, firstName, lastName)
}

// trimmedOrError trims a provided (non-nil) field and rejects it if that
// leaves it blank; a nil field passes through untouched.
func trimmedOrError(field *string, name string) (*string, error) {
	if field == nil {
		return nil, nil
	}
	trimmed := strings.TrimSpace(*field)
	if trimmed == "" {
		return nil, fmt.Errorf("%w: %s cannot be blank", domain.ErrInvalidArgument, name)
	}
	if len(trimmed) > maxNameLength {
		return nil, fmt.Errorf("%w: %s must be at most %d characters", domain.ErrInvalidArgument, name, maxNameLength)
	}
	return &trimmed, nil
}

// normalizeEmail lowercases and trims an email so storage and lookups are
// case-insensitive; Auth0 and API callers aren't guaranteed to send
// consistent casing for the same address.
func normalizeEmail(email string) string {
	return strings.ToLower(strings.TrimSpace(email))
}
