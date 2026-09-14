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
	// Proves email lowercase CHECK holds even if service normalization is bypassed.
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

	assert.ErrorIs(t, err, domain.ErrEmailConflict)
}

func TestUserRepository_UpdateProfile_PartialUpdateLeavesOtherFieldAlone(t *testing.T) {
	_, queries := setupDB(t)
	repo := store.NewUserRepository(queries)
	ctx := context.Background()

	user, err := repo.UpsertByAuth0Subject(ctx, "auth0|1", "carol@example.com", "Carol", "Jones")
	require.NoError(t, err)

	newFirst := "Caroline"
	updated, err := repo.UpdateProfile(ctx, user.ID, &newFirst, nil, nil)

	require.NoError(t, err)
	assert.Equal(t, "Caroline", updated.FirstName)
	assert.Equal(t, "Jones", updated.LastName, "nil lastName must leave the existing value untouched")
}

func TestUserRepository_UpdateProfile_NotFoundForUnknownID(t *testing.T) {
	_, queries := setupDB(t)
	repo := store.NewUserRepository(queries)

	name := "Nobody"
	_, err := repo.UpdateProfile(context.Background(), uuid.New(), &name, nil, nil)

	assert.ErrorIs(t, err, domain.ErrNotFound)
}

func TestUserRepository_Delete_CascadesToLocationAndTrackingEdges(t *testing.T) {
	// Proves migration 000005 actually took effect: before it, this would fail
	// with a foreign-key violation (user_tracking was ON DELETE RESTRICT)
	// instead of cleanly removing every row that referenced this user.
	_, queries := setupDB(t)
	repo := store.NewUserRepository(queries)
	trackingRepo := store.NewTrackingRepository(queries)
	locationRepo := store.NewLocationRepository(queries)
	ctx := context.Background()

	alice, err := repo.UpsertByAuth0Subject(ctx, "auth0|alice", "alice@example.com", "Alice", "A")
	require.NoError(t, err)
	bob, err := repo.UpsertByAuth0Subject(ctx, "auth0|bob", "bob@example.com", "Bob", "B")
	require.NoError(t, err)

	_, err = locationRepo.Upsert(ctx, alice.ID, 1.0, 2.0)
	require.NoError(t, err)
	require.NoError(t, trackingRepo.Add(ctx, alice.ID, bob.ID)) // alice tracks bob (pending)
	require.NoError(t, trackingRepo.Add(ctx, bob.ID, alice.ID)) // bob tracks alice (pending)

	err = repo.Delete(ctx, alice.ID)
	require.NoError(t, err, "delete must succeed despite existing location + tracking edges in both directions")

	_, err = repo.GetByID(ctx, alice.ID)
	assert.ErrorIs(t, err, domain.ErrNotFound)

	tracked, err := trackingRepo.GetOutgoingRequests(ctx, bob.ID)
	require.NoError(t, err)
	assert.Empty(t, tracked, "bob's pending request to alice must be gone, not orphaned")
}

func TestUserRepository_Delete_NotFoundForUnknownID(t *testing.T) {
	_, queries := setupDB(t)
	repo := store.NewUserRepository(queries)

	err := repo.Delete(context.Background(), uuid.New())

	assert.ErrorIs(t, err, domain.ErrNotFound)
}

func TestUserRepository_GetByEmail_IsCaseSensitiveAtStorageLayer(t *testing.T) {
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
