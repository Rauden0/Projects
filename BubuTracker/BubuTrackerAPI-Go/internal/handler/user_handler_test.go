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

// requestAsUser builds a request carrying user as the resolved current
// user, exactly as handler.CurrentUserMiddleware would set it on a real
// request. Handler-level tests call handlers directly (no router, no
// middleware chain), so the middleware's job of resolving "who is the
// current user" has to be done by hand here.
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
