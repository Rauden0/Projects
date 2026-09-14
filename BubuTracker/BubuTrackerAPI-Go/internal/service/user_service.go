package service

import (
	"context"
	"errors"
	"fmt"
	"strings"
	"time"

	"github.com/google/uuid"

	"github.com/Rauden0/bubutracker-api/internal/domain"
)

const maxNameLength = 100
const defaultUserCacheTTL = 2 * time.Minute

// Mirrors chk_marker_color (db/migrations/000004).
var allowedMarkerColors = map[string]bool{
	"#FF9800": true,
	"#E91E63": true,
	"#8E24AA": true,
	"#3949AB": true,
	"#00897B": true,
	"#43A047": true,
	"#F4511E": true,
	"#6D4C41": true,
}

type UserService struct {
	users UserRepository
	cache *userCache
}

type UserServiceOption func(*UserService)

// WithUserCacheTTL overrides the subject->user cache's TTL. Production
// callers don't need this (the default is sensible); it exists so tests can
// use a short real TTL + a short real sleep, matching this codebase's
// existing rate-limiter test style instead of injecting a fake clock.
func WithUserCacheTTL(ttl time.Duration) UserServiceOption {
	return func(s *UserService) { s.cache.ttl = ttl }
}

func NewUserService(users UserRepository, opts ...UserServiceOption) *UserService {
	s := &UserService{users: users, cache: newUserCache(defaultUserCacheTTL)}
	for _, opt := range opts {
		opt(s)
	}
	return s
}

// emailVerified is nil when the Auth0 Action doesn't set the
// email_verified custom claim yet - treated as trusted (matches this
// method's behavior before that claim existed) so rollout can't regress
// email syncing. An explicit false means the claim is untrusted: an
// attacker-controlled connection could otherwise claim someone else's email
// address without ever proving they own it, and POST /tracking would then
// silently route to the attacker's account instead of the real owner's.
func (s *UserService) GetOrCreateBySubject(ctx context.Context, subjectID, email string, emailVerified *bool, firstName, lastName string) (domain.User, error) {
	if subjectID == "" {
		return domain.User{}, domain.ErrUnauthenticated
	}
	email = normalizeEmail(email)
	trustEmail := emailVerified == nil || *emailVerified
	if email == "" || !trustEmail {
		email = normalizeEmail(fmt.Sprintf("%s@users.auth0.local", subjectID))
	}

	now := time.Now()
	// Only short-circuits when the cached email still matches the claims'
	// email - an email change falls through to the real DB path below, same
	// as the existing found-but-different-email branch already does.
	if cached, ok := s.cache.get(subjectID, now); ok && cached.Email == email {
		return cached, nil
	}

	existing, err := s.users.GetByAuth0SubjectID(ctx, subjectID)
	found := err == nil
	if found {
		if existing.Email == email {
			s.cache.set(subjectID, existing, now)
			return existing, nil
		}
	} else if !errors.Is(err, domain.ErrNotFound) {
		return domain.User{}, err
	}

	upserted, err := s.users.UpsertByAuth0Subject(ctx, subjectID, email, firstName, lastName)
	if err != nil {
		if found && errors.Is(err, domain.ErrEmailConflict) {
			// Keep serving the known account rather than locking out on email refresh conflict.
			s.cache.set(subjectID, existing, now)
			return existing, nil
		}
		return domain.User{}, err
	}
	s.cache.set(subjectID, upserted, now)
	return upserted, nil
}

func (s *UserService) UpdateProfile(ctx context.Context, userID uuid.UUID, firstName, lastName, markerColor *string) (domain.User, error) {
	firstName, err := trimmedOrError(firstName, "firstName")
	if err != nil {
		return domain.User{}, err
	}
	lastName, err = trimmedOrError(lastName, "lastName")
	if err != nil {
		return domain.User{}, err
	}
	if markerColor != nil && !allowedMarkerColors[*markerColor] {
		return domain.User{}, fmt.Errorf("%w: markerColor must be one of the preset colors", domain.ErrInvalidArgument)
	}

	updated, err := s.users.UpdateProfile(ctx, userID, firstName, lastName, markerColor)
	if err != nil {
		return domain.User{}, err
	}
	// Refresh rather than just invalidate: the very next request for this
	// subject (e.g. the map screen the user returns to right after editing
	// their profile) gets the new data from cache instead of a cache miss.
	s.cache.set(updated.Auth0SubjectID, updated, time.Now())
	return updated, nil
}

// DeleteAccount removes the user row; migration 000005 cascades this to their
// location and every user_tracking edge (as tracker and as tracked user), so
// callers never need to clean those up separately. subjectID is the caller's
// own Auth0 subject (already resolved earlier in the same request by
// CurrentUserMiddleware) - purely to invalidate their cache entry; without
// this, every request for the next cache TTL would keep resolving to a
// deleted user ID and fail instead of cleanly re-provisioning a fresh account.
func (s *UserService) DeleteAccount(ctx context.Context, userID uuid.UUID, subjectID string) error {
	if err := s.users.Delete(ctx, userID); err != nil {
		return err
	}
	s.cache.invalidate(subjectID)
	return nil
}

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

func normalizeEmail(email string) string {
	return strings.ToLower(strings.TrimSpace(email))
}
