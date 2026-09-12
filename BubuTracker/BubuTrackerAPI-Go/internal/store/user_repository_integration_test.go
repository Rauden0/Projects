//go:build integration

package store_test

import (
	"context"
	"testing"

	"github.com/google/uuid"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"

	"github.com/Rauden0/bubutracker-api/internal/domain"
	"github.com/Rauden0/bubutracker-api/internal/store"
)

func TestUserRepository_GetByID_NotFound(t *testing.T) {
	_, queries := setupDB(t)
	repo := store.NewUserRepository(queries)

	_, err := repo.GetByID(context.Background(), uuid.New())

	assert.ErrorIs(t, err, domain.ErrNotFound)
}

func TestUserRepository_UpsertByAuth0Subject_CreatesOnFirstCall(t *testing.T) {
	_, queries := setupDB(t)
	repo := store.NewUserRepository(queries)
	ctx := context.Background()

	user, err := repo.UpsertByAuth0Subject(ctx, "auth0|1", "alice@example.com", "Alice", "Smith")

	require.NoError(t, err)
	assert.NotEqual(t, uuid.Nil, user.ID)
	assert.Equal(t, "alice@example.com", user.Email)
	assert.Equal(t, "Alice", user.FirstName)
	assert.Equal(t, "Smith", user.LastName)

	fetched, err := repo.GetByID(ctx, user.ID)
	require.NoError(t, err)
	assert.Equal(t, user, fetched)
}

// TestUserRepository_UpsertByAuth0Subject_ConvergesWithoutError is the
// direct regression test for the race this API used to have: two upserts
// for the same subject (simulating two concurrent first-sign-in requests,
// or a later login with a drifted email) must converge onto one row
// instead of the second call erroring with a unique-violation.
func TestUserRepository_UpsertByAuth0Subject_ConvergesWithoutError(t *testing.T) {
	_, queries := setupDB(t)
	repo := store.NewUserRepository(queries)
	ctx := context.Background()

	first, err := repo.UpsertByAuth0Subject(ctx, "auth0|1", "alice@example.com", "Alice", "Smith")
	require.NoError(t, err)

	second, err := repo.UpsertByAuth0Subject(ctx, "auth0|1", "alice-new@example.com", "ShouldNotAppear", "ShouldNotAppear")
	require.NoError(t, err, "second upsert for the same subject must not error")

	assert.Equal(t, first.ID, second.ID)
	assert.Equal(t, "alice-new@example.com", second.Email, "email should converge to the latest value")
	assert.Equal(t, "Alice", second.FirstName, "name from the first insert must survive a later conflict")
	assert.Equal(t, "Smith", second.LastName)
}

func TestUserRepository_UpsertByAuth0Subject_RejectsUppercaseEmail(t *testing.T) {
	// The service layer normalizes email casing before this is ever called;
	// this proves the DB CHECK constraint holds as a second line of defense
	// even if that normalization were ever bypassed or removed by mistake.
	_, queries := setupDB(t)
	repo := store.NewUserRepository(queries)

	_, err := repo.UpsertByAuth0Subject(context.Background(), "auth0|1", "Upper@Example.COM", "X", "Y")

	assert.Error(t, err)
}

func TestUserRepository_UpsertByAuth0Subject_DuplicateEmailDifferentSubjectFails(t *testing.T) {
	_, queries := setupDB(t)
	repo := store.NewUserRepository(queries)
	ctx := context.Background()

	_, err := repo.UpsertByAuth0Subject(ctx, "auth0|1", "shared@example.com", "A", "B")
	require.NoError(t, err)

	_, err = repo.UpsertByAuth0Subject(ctx, "auth0|2", "shared@example.com", "C", "D")

	assert.ErrorIs(t, err, domain.ErrAlreadyExists)
}

func TestUserRepository_UpdateProfile_PartialUpdateLeavesOtherFieldAlone(t *testing.T) {
	_, queries := setupDB(t)
	repo := store.NewUserRepository(queries)
	ctx := context.Background()

	user, err := repo.UpsertByAuth0Subject(ctx, "auth0|1", "carol@example.com", "Carol", "Jones")
	require.NoError(t, err)

	newFirst := "Caroline"
	updated, err := repo.UpdateProfile(ctx, user.ID, &newFirst, nil)

	require.NoError(t, err)
	assert.Equal(t, "Caroline", updated.FirstName)
	assert.Equal(t, "Jones", updated.LastName, "nil lastName must leave the existing value untouched")
}

func TestUserRepository_UpdateProfile_NotFoundForUnknownID(t *testing.T) {
	_, queries := setupDB(t)
	repo := store.NewUserRepository(queries)

	name := "Nobody"
	_, err := repo.UpdateProfile(context.Background(), uuid.New(), &name, nil)

	assert.ErrorIs(t, err, domain.ErrNotFound)
}

func TestUserRepository_GetByEmail_IsCaseSensitiveAtStorageLayer(t *testing.T) {
	// Case-insensitivity is a service-layer guarantee (normalize before
	// write and before lookup); the repository itself does an exact match,
	// so this documents that boundary rather than asserting a repo-level
	// case-insensitive lookup that doesn't exist.
	_, queries := setupDB(t)
	repo := store.NewUserRepository(queries)
	ctx := context.Background()

	_, err := repo.UpsertByAuth0Subject(ctx, "auth0|1", "dave@example.com", "Dave", "X")
	require.NoError(t, err)

	_, err = repo.GetByEmail(ctx, "dave@example.com")
	require.NoError(t, err)

	_, err = repo.GetByEmail(ctx, "Dave@Example.com")
	assert.ErrorIs(t, err, domain.ErrNotFound)
}
