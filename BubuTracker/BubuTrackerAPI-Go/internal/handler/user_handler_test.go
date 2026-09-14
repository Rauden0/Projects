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

	"github.com/Rauden0/bubutracker-api/internal/domain"
	"github.com/Rauden0/bubutracker-api/internal/handler"
)

type fakeUserService struct {
	user             domain.User
	getErr           error
	updateErr        error
	deleteErr        error
	deletedID        uuid.UUID
	deletedSubjectID string
	lastUpdate       struct {
		firstName, lastName, markerColor *string
	}
}

func (f *fakeUserService) GetOrCreateBySubject(_ context.Context, _, _ string, _ *bool, _, _ string) (domain.User, error) {
	return f.user, f.getErr
}

func (f *fakeUserService) UpdateProfile(_ context.Context, _ uuid.UUID, firstName, lastName, markerColor *string) (domain.User, error) {
	f.lastUpdate.firstName = firstName
	f.lastUpdate.lastName = lastName
	f.lastUpdate.markerColor = markerColor
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
	if markerColor != nil {
		updated.MarkerColor = *markerColor
	}
	return updated, nil
}

func (f *fakeUserService) DeleteAccount(_ context.Context, userID uuid.UUID, subjectID string) error {
	f.deletedID = userID
	f.deletedSubjectID = subjectID
	return f.deleteErr
}

func requestAsUser(method, target, body string, user domain.User) *http.Request {
	req := httptest.NewRequest(method, target, strings.NewReader(body))
	return req.WithContext(handler.WithCurrentUser(req.Context(), user))
}

func TestUserHandler_Me_ReturnsProfile(t *testing.T) {
	user := domain.User{ID: uuid.New(), Email: "alice@example.com", FirstName: "Alice"}
	h := handler.NewUserHandler(&fakeUserService{})

	rec := httptest.NewRecorder()
	h.Me(rec, requestAsUser(http.MethodGet, "/api/v1/users/me", "", user))

	require.Equal(t, http.StatusOK, rec.Code)

	var got map[string]any
	require.NoError(t, json.NewDecoder(rec.Body).Decode(&got))
	assert.Equal(t, "alice@example.com", got["email"])
	assert.Equal(t, "Alice", got["firstName"])
}

func TestUserHandler_UpdateMe_AppliesPartialUpdate(t *testing.T) {
	user := domain.User{ID: uuid.New(), Email: "alice@example.com", FirstName: "Alice", LastName: "Smith"}
	svc := &fakeUserService{user: user}
	h := handler.NewUserHandler(svc)

	rec := httptest.NewRecorder()
	h.UpdateMe(rec, requestAsUser(http.MethodPatch, "/api/v1/users/me", `{"firstName":"Alicia"}`, user))

	require.Equal(t, http.StatusOK, rec.Code)
	require.NotNil(t, svc.lastUpdate.firstName)
	assert.Equal(t, "Alicia", *svc.lastUpdate.firstName)
	assert.Nil(t, svc.lastUpdate.lastName)
}

func TestUserHandler_UpdateMe_RejectsMalformedBody(t *testing.T) {
	user := domain.User{ID: uuid.New()}
	h := handler.NewUserHandler(&fakeUserService{user: user})

	rec := httptest.NewRecorder()
	h.UpdateMe(rec, requestAsUser(http.MethodPatch, "/api/v1/users/me", `not json`, user))

	assert.Equal(t, http.StatusBadRequest, rec.Code)
}

func TestUserHandler_DeleteMe_DeletesTheAuthenticatedUser(t *testing.T) {
	user := domain.User{ID: uuid.New(), Email: "alice@example.com", Auth0SubjectID: "auth0|123"}
	svc := &fakeUserService{user: user}
	h := handler.NewUserHandler(svc)

	rec := httptest.NewRecorder()
	h.DeleteMe(rec, requestAsUser(http.MethodDelete, "/api/v1/users/me", "", user))

	assert.Equal(t, http.StatusNoContent, rec.Code)
	assert.Equal(t, user.ID, svc.deletedID, "must delete the authenticated user, never an ID from the request")
	assert.Equal(t, user.Auth0SubjectID, svc.deletedSubjectID, "must pass the subject ID so the service can invalidate its cache")
}

func TestUserHandler_DeleteMe_PropagatesServiceError(t *testing.T) {
	user := domain.User{ID: uuid.New()}
	svc := &fakeUserService{user: user, deleteErr: domain.ErrNotFound}
	h := handler.NewUserHandler(svc)

	rec := httptest.NewRecorder()
	h.DeleteMe(rec, requestAsUser(http.MethodDelete, "/api/v1/users/me", "", user))

	assert.Equal(t, http.StatusNotFound, rec.Code)
}
