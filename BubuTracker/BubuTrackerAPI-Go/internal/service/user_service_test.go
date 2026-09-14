package service_test

import (
	"context"
	"strings"
	"testing"
	"time"

	"github.com/google/uuid"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"

	"github.com/Rauden0/bubutracker-api/internal/domain"
	"github.com/Rauden0/bubutracker-api/internal/service"
)

func TestUserService_GetOrCreateBySubject_CreatesOnFirstSignIn(t *testing.T) {
	repo := newFakeUserRepo()
	svc := service.NewUserService(repo)

	user, err := svc.GetOrCreateBySubject(context.Background(), "auth0|123", "alice@example.com", nil, "Alice", "Smith")

	require.NoError(t, err)
	assert.Equal(t, "auth0|123", user.Auth0SubjectID)
	assert.Equal(t, "alice@example.com", user.Email)
	assert.Equal(t, "Alice", user.FirstName)
}

func TestUserService_GetOrCreateBySubject_ReturnsExistingAndSyncsEmail(t *testing.T) {
	repo := newFakeUserRepo()
	existing := domain.User{ID: uuid.New(), Auth0SubjectID: "auth0|123", Email: "old@example.com"}
	repo.seed(existing)
	svc := service.NewUserService(repo)

	user, err := svc.GetOrCreateBySubject(context.Background(), "auth0|123", "new@example.com", nil, "", "")

	require.NoError(t, err)
	assert.Equal(t, existing.ID, user.ID)
	assert.Equal(t, "new@example.com", user.Email)
}

func TestUserService_GetOrCreateBySubject_FallsBackToKnownAccountOnEmailConflict(t *testing.T) {
	repo := newFakeUserRepo()
	existing := domain.User{ID: uuid.New(), Auth0SubjectID: "auth0|123", Email: "old@example.com"}
	other := domain.User{ID: uuid.New(), Auth0SubjectID: "auth0|456", Email: "taken@example.com"}
	repo.seed(existing)
	repo.seed(other)
	svc := service.NewUserService(repo)

	user, err := svc.GetOrCreateBySubject(context.Background(), "auth0|123", "taken@example.com", nil, "", "")

	require.NoError(t, err)
	assert.Equal(t, existing.ID, user.ID)
	assert.Equal(t, "old@example.com", user.Email, "cached email must be left untouched, not the colliding one")
}

func TestUserService_GetOrCreateBySubject_RejectsNewSignUpWithTakenEmail(t *testing.T) {
	repo := newFakeUserRepo()
	other := domain.User{ID: uuid.New(), Auth0SubjectID: "auth0|456", Email: "taken@example.com"}
	repo.seed(other)
	svc := service.NewUserService(repo)

	_, err := svc.GetOrCreateBySubject(context.Background(), "auth0|new", "taken@example.com", nil, "", "")

	assert.ErrorIs(t, err, domain.ErrEmailConflict)
}

func TestUserService_GetOrCreateBySubject_NormalizesEmailCase(t *testing.T) {
	repo := newFakeUserRepo()
	svc := service.NewUserService(repo)

	user, err := svc.GetOrCreateBySubject(context.Background(), "auth0|123", "Alice@Example.COM", nil, "", "")

	require.NoError(t, err)
	assert.Equal(t, "alice@example.com", user.Email)
}

func TestUserService_GetOrCreateBySubject_RejectsMissingSubject(t *testing.T) {
	svc := service.NewUserService(newFakeUserRepo())

	_, err := svc.GetOrCreateBySubject(context.Background(), "", "a@example.com", nil, "", "")

	assert.ErrorIs(t, err, domain.ErrUnauthenticated)
}

func TestUserService_GetOrCreateBySubject_FallsBackToSyntheticEmailWhenExplicitlyUnverified(t *testing.T) {
	repo := newFakeUserRepo()
	svc := service.NewUserService(repo)
	unverified := false

	user, err := svc.GetOrCreateBySubject(context.Background(), "auth0|new", "victim@example.com", &unverified, "", "")

	require.NoError(t, err)
	assert.NotEqual(t, "victim@example.com", user.Email, "an unproven email claim must never become the account's email")
	assert.Contains(t, user.Email, "@users.auth0.local")
}

func TestUserService_GetOrCreateBySubject_DoesNotSyncAnUnverifiedEmailOverAnExistingOne(t *testing.T) {
	repo := newFakeUserRepo()
	existing := domain.User{ID: uuid.New(), Auth0SubjectID: "auth0|123", Email: "old@example.com"}
	repo.seed(existing)
	svc := service.NewUserService(repo)
	unverified := false

	user, err := svc.GetOrCreateBySubject(context.Background(), "auth0|123", "someone-elses@example.com", &unverified, "", "")

	require.NoError(t, err)
	assert.Equal(t, existing.ID, user.ID)
	assert.NotEqual(t, "someone-elses@example.com", user.Email)
}

func TestUserService_GetOrCreateBySubject_TrustsExplicitlyVerifiedEmail(t *testing.T) {
	repo := newFakeUserRepo()
	svc := service.NewUserService(repo)
	verified := true

	user, err := svc.GetOrCreateBySubject(context.Background(), "auth0|123", "alice@example.com", &verified, "", "")

	require.NoError(t, err)
	assert.Equal(t, "alice@example.com", user.Email)
}

func TestUserService_GetOrCreateBySubject_ServesFromCacheWithoutRereadingTheRepository(t *testing.T) {
	repo := newFakeUserRepo()
	svc := service.NewUserService(repo)

	first, err := svc.GetOrCreateBySubject(context.Background(), "auth0|123", "alice@example.com", nil, "Alice", "Smith")
	require.NoError(t, err)

	// Mutate the repo directly, bypassing the service - a real cache hit must
	// keep returning the value from the first call, not this changed one.
	mutated := first
	mutated.FirstName = "ChangedBehindTheCache"
	repo.seed(mutated)

	second, err := svc.GetOrCreateBySubject(context.Background(), "auth0|123", "alice@example.com", nil, "Alice", "Smith")
	require.NoError(t, err)
	assert.Equal(t, "Alice", second.FirstName, "a fresh cache entry must be served without re-reading the repository")
}

func TestUserService_GetOrCreateBySubject_RereadsTheRepositoryAfterCacheTTLExpires(t *testing.T) {
	repo := newFakeUserRepo()
	svc := service.NewUserService(repo, service.WithUserCacheTTL(20*time.Millisecond))

	first, err := svc.GetOrCreateBySubject(context.Background(), "auth0|123", "alice@example.com", nil, "Alice", "Smith")
	require.NoError(t, err)

	mutated := first
	mutated.FirstName = "ChangedAfterExpiry"
	repo.seed(mutated)

	time.Sleep(30 * time.Millisecond)

	second, err := svc.GetOrCreateBySubject(context.Background(), "auth0|123", "alice@example.com", nil, "Alice", "Smith")
	require.NoError(t, err)
	assert.Equal(t, "ChangedAfterExpiry", second.FirstName, "an expired entry must fall through to a real repository read")
}

func TestUserService_GetOrCreateBySubject_ChangedEmailBypassesTheCache(t *testing.T) {
	repo := newFakeUserRepo()
	svc := service.NewUserService(repo)

	first, err := svc.GetOrCreateBySubject(context.Background(), "auth0|123", "old@example.com", nil, "Alice", "Smith")
	require.NoError(t, err)

	second, err := svc.GetOrCreateBySubject(context.Background(), "auth0|123", "new@example.com", nil, "", "")
	require.NoError(t, err)

	assert.Equal(t, first.ID, second.ID)
	assert.Equal(t, "new@example.com", second.Email, "a real email change must not be masked by a stale cache entry")
}

func TestUserService_UpdateProfile_RefreshesTheSubjectCache(t *testing.T) {
	repo := newFakeUserRepo()
	user := domain.User{ID: uuid.New(), Auth0SubjectID: "auth0|123", Email: "a@example.com", FirstName: "Alice"}
	repo.seed(user)
	svc := service.NewUserService(repo)
	// Populate the cache the same way a real request would, via CurrentUserMiddleware.
	_, err := svc.GetOrCreateBySubject(context.Background(), user.Auth0SubjectID, user.Email, nil, "", "")
	require.NoError(t, err)

	newFirst := "Alicia"
	_, err = svc.UpdateProfile(context.Background(), user.ID, &newFirst, nil, nil)
	require.NoError(t, err)

	// Mutate the repo behind the cache so this can only pass if UpdateProfile
	// actually refreshed the cache entry rather than just leaving the old one.
	stale := user
	stale.FirstName = "ShouldNotBeSeen"
	repo.seed(stale)

	refetched, err := svc.GetOrCreateBySubject(context.Background(), user.Auth0SubjectID, user.Email, nil, "", "")
	require.NoError(t, err)
	assert.Equal(t, "Alicia", refetched.FirstName)
}

func TestUserService_DeleteAccount_InvalidatesTheSubjectCache(t *testing.T) {
	repo := newFakeUserRepo()
	user := domain.User{ID: uuid.New(), Auth0SubjectID: "auth0|123", Email: "a@example.com"}
	repo.seed(user)
	svc := service.NewUserService(repo)
	_, err := svc.GetOrCreateBySubject(context.Background(), user.Auth0SubjectID, user.Email, nil, "", "")
	require.NoError(t, err)

	require.NoError(t, svc.DeleteAccount(context.Background(), user.ID, user.Auth0SubjectID))

	// If the cache entry survived the delete, this would keep returning the
	// deleted user's ID instead of re-provisioning a fresh account.
	recreated, err := svc.GetOrCreateBySubject(context.Background(), user.Auth0SubjectID, user.Email, nil, "", "")
	require.NoError(t, err)
	assert.NotEqual(t, user.ID, recreated.ID, "a cache entry must not outlive the account it points at")
}

func TestUserService_UpdateProfile_PartialUpdateLeavesOtherFieldUntouched(t *testing.T) {
	repo := newFakeUserRepo()
	user := domain.User{ID: uuid.New(), Email: "a@example.com", FirstName: "Alice", LastName: "Smith"}
	repo.seed(user)
	svc := service.NewUserService(repo)

	newFirst := "Alicia"
	updated, err := svc.UpdateProfile(context.Background(), user.ID, &newFirst, nil, nil)

	require.NoError(t, err)
	assert.Equal(t, "Alicia", updated.FirstName)
	assert.Equal(t, "Smith", updated.LastName)
}

func TestUserService_UpdateProfile_RejectsBlankName(t *testing.T) {
	svc := service.NewUserService(newFakeUserRepo())

	blank := "   "
	_, err := svc.UpdateProfile(context.Background(), uuid.New(), &blank, nil, nil)

	assert.ErrorIs(t, err, domain.ErrInvalidArgument)
}

func TestUserService_UpdateProfile_RejectsOverlongName(t *testing.T) {
	svc := service.NewUserService(newFakeUserRepo())

	tooLong := strings.Repeat("a", 101)
	_, err := svc.UpdateProfile(context.Background(), uuid.New(), &tooLong, nil, nil)

	assert.ErrorIs(t, err, domain.ErrInvalidArgument)
}

func TestUserService_UpdateProfile_AcceptsAllowedMarkerColor(t *testing.T) {
	repo := newFakeUserRepo()
	user := domain.User{ID: uuid.New(), Email: "a@example.com", MarkerColor: "#FF9800"}
	repo.seed(user)
	svc := service.NewUserService(repo)

	teal := "#00897B"
	updated, err := svc.UpdateProfile(context.Background(), user.ID, nil, nil, &teal)

	require.NoError(t, err)
	assert.Equal(t, "#00897B", updated.MarkerColor)
}

func TestUserService_UpdateProfile_RejectsUnknownMarkerColor(t *testing.T) {
	svc := service.NewUserService(newFakeUserRepo())

	bogus := "#123456"
	_, err := svc.UpdateProfile(context.Background(), uuid.New(), nil, nil, &bogus)

	assert.ErrorIs(t, err, domain.ErrInvalidArgument)
}

func TestUserService_DeleteAccount_RemovesTheUser(t *testing.T) {
	repo := newFakeUserRepo()
	user := domain.User{ID: uuid.New(), Auth0SubjectID: "auth0|123", Email: "a@example.com"}
	repo.seed(user)
	svc := service.NewUserService(repo)

	err := svc.DeleteAccount(context.Background(), user.ID, user.Auth0SubjectID)

	require.NoError(t, err)
	assert.True(t, repo.deleted[user.ID])
	_, err = repo.GetByID(context.Background(), user.ID)
	assert.ErrorIs(t, err, domain.ErrNotFound)
}

func TestUserService_DeleteAccount_PropagatesRepositoryError(t *testing.T) {
	svc := service.NewUserService(newFakeUserRepo())

	err := svc.DeleteAccount(context.Background(), uuid.New(), "auth0|ghost")

	assert.ErrorIs(t, err, domain.ErrNotFound)
}
