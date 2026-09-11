package handler_test

import (
	"context"
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"strings"
	"testing"

	"github.com/google/uuid"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"

	"github.com/Rauden0/bubutracker-api/internal/auth"
	"github.com/Rauden0/bubutracker-api/internal/domain"
	"github.com/Rauden0/bubutracker-api/internal/handler"
)

// fakeUserService implements handler.UserService for handler-level tests,
// isolating them from the real service/database layers.
type fakeUserService struct {
	user       domain.User
	getErr     error
	updateErr  error
	lastUpdate struct {
		firstName, lastName *string
	}
}

func (f *fakeUserService) GetOrCreateBySubject(_ context.Context, _, _, _, _ string) (domain.User, error) {
	return f.user, f.getErr
}

func (f *fakeUserService) UpdateProfile(_ context.Context, _ uuid.UUID, firstName, lastName *string) (domain.User, error) {
	f.lastUpdate.firstName = firstName
	f.lastUpdate.lastName = lastName
	if f.updateErr != nil {
		return domain.User{}, f.updateErr
	}
	updated := f.user
	if firstName != nil {
		updated.FirstName = *firstName
	}
	if lastName != nil {
		updated.LastName = *lastName
	}
	return updated, nil
}

func authedRequest(method, target, body string) *http.Request {
	req := httptest.NewRequest(method, target, strings.NewReader(body))
	claims := auth.Claims{Subject: "auth0|123", Email: "alice@example.com"}
	return req.WithContext(auth.NewContext(req.Context(), claims))
}

func TestUserHandler_Me_ReturnsProfile(t *testing.T) {
	svc := &fakeUserService{user: domain.User{ID: uuid.New(), Email: "alice@example.com", FirstName: "Alice"}}
	h := handler.NewUserHandler(svc)

	rec := httptest.NewRecorder()
	h.Me(rec, authedRequest(http.MethodGet, "/api/v1/users/me", ""))

	require.Equal(t, http.StatusOK, rec.Code)

	var got map[string]any
	require.NoError(t, json.NewDecoder(rec.Body).Decode(&got))
	assert.Equal(t, "alice@example.com", got["email"])
	assert.Equal(t, "Alice", got["firstName"])
}

func TestUserHandler_UpdateMe_AppliesPartialUpdate(t *testing.T) {
	svc := &fakeUserService{user: domain.User{ID: uuid.New(), Email: "alice@example.com", FirstName: "Alice", LastName: "Smith"}}
	h := handler.NewUserHandler(svc)

	rec := httptest.NewRecorder()
	h.UpdateMe(rec, authedRequest(http.MethodPatch, "/api/v1/users/me", `{"firstName":"Alicia"}`))

	require.Equal(t, http.StatusOK, rec.Code)
	require.NotNil(t, svc.lastUpdate.firstName)
	assert.Equal(t, "Alicia", *svc.lastUpdate.firstName)
	assert.Nil(t, svc.lastUpdate.lastName)
}

func TestUserHandler_UpdateMe_RejectsMalformedBody(t *testing.T) {
	svc := &fakeUserService{user: domain.User{ID: uuid.New()}}
	h := handler.NewUserHandler(svc)

	rec := httptest.NewRecorder()
	h.UpdateMe(rec, authedRequest(http.MethodPatch, "/api/v1/users/me", `not json`))

	assert.Equal(t, http.StatusBadRequest, rec.Code)
}

func TestUserHandler_Me_PropagatesServiceError(t *testing.T) {
	svc := &fakeUserService{getErr: domain.ErrUnauthenticated}
	h := handler.NewUserHandler(svc)

	rec := httptest.NewRecorder()
	h.Me(rec, authedRequest(http.MethodGet, "/api/v1/users/me", ""))

	assert.Equal(t, http.StatusUnauthorized, rec.Code)
}
