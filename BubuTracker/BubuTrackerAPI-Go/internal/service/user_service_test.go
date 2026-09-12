package service_test

import (
	"context"
	"strings"
	"testing"

	"github.com/google/uuid"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"

	"github.com/Rauden0/bubutracker-api/internal/domain"
	"github.com/Rauden0/bubutracker-api/internal/service"
)

func TestUserService_GetOrCreateBySubject_CreatesOnFirstSignIn(t *testing.T) {
	repo := newFakeUserRepo()
	svc := service.NewUserService(repo)

	user, err := svc.GetOrCreateBySubject(context.Background(), "auth0|123", "alice@example.com", "Alice", "Smith")

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

	user, err := svc.GetOrCreateBySubject(context.Background(), "auth0|123", "new@example.com", "", "")

	require.NoError(t, err)
	assert.Equal(t, existing.ID, user.ID)
	assert.Equal(t, "new@example.com", user.Email)
}

func TestUserService_GetOrCreateBySubject_NormalizesEmailCase(t *testing.T) {
	repo := newFakeUserRepo()
	svc := service.NewUserService(repo)

	user, err := svc.GetOrCreateBySubject(context.Background(), "auth0|123", "Alice@Example.COM", "", "")

	require.NoError(t, err)
	assert.Equal(t, "alice@example.com", user.Email)
}

func TestUserService_GetOrCreateBySubject_RejectsMissingSubject(t *testing.T) {
	svc := service.NewUserService(newFakeUserRepo())

	_, err := svc.GetOrCreateBySubject(context.Background(), "", "a@example.com", "", "")

	assert.ErrorIs(t, err, domain.ErrUnauthenticated)
}

func TestUserService_UpdateProfile_PartialUpdateLeavesOtherFieldUntouched(t *testing.T) {
	repo := newFakeUserRepo()
	user := domain.User{ID: uuid.New(), Email: "a@example.com", FirstName: "Alice", LastName: "Smith"}
	repo.seed(user)
	svc := service.NewUserService(repo)

	newFirst := "Alicia"
	updated, err := svc.UpdateProfile(context.Background(), user.ID, &newFirst, nil)

	require.NoError(t, err)
	assert.Equal(t, "Alicia", updated.FirstName)
	assert.Equal(t, "Smith", updated.LastName)
}

func TestUserService_UpdateProfile_RejectsBlankName(t *testing.T) {
	// Validation happens against the input alone, before any repository
	// call, so this rejects even for a user ID that doesn't exist.
	svc := service.NewUserService(newFakeUserRepo())

	blank := "   "
	_, err := svc.UpdateProfile(context.Background(), uuid.New(), &blank, nil)

	assert.ErrorIs(t, err, domain.ErrInvalidArgument)
}

func TestUserService_UpdateProfile_RejectsOverlongName(t *testing.T) {
	svc := service.NewUserService(newFakeUserRepo())

	tooLong := strings.Repeat("a", 101)
	_, err := svc.UpdateProfile(context.Background(), uuid.New(), &tooLong, nil)

	assert.ErrorIs(t, err, domain.ErrInvalidArgument)
}
