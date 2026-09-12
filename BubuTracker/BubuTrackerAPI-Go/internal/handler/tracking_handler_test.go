package handler_test

import (
	"context"
	"net/http"
	"net/http/httptest"
	"testing"

	"github.com/go-chi/chi/v5"
	"github.com/google/uuid"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"

	"github.com/Rauden0/bubutracker-api/internal/domain"
	"github.com/Rauden0/bubutracker-api/internal/handler"
)

type fakeTrackingService struct {
	tracked      []domain.User
	addErr       error
	removeErr    error
	lastAddEmail string
}

func (f *fakeTrackingService) ListTracked(_ context.Context, _ uuid.UUID) ([]domain.User, error) {
	return f.tracked, nil
}

func (f *fakeTrackingService) AddTracking(_ context.Context, _ uuid.UUID, email string) error {
	f.lastAddEmail = email
	return f.addErr
}

func (f *fakeTrackingService) RemoveTracking(_ context.Context, _, _ uuid.UUID) error {
	return f.removeErr
}

func withURLParam(r *http.Request, key, value string) *http.Request {
	rctx := chi.NewRouteContext()
	rctx.URLParams.Add(key, value)
	return r.WithContext(context.WithValue(r.Context(), chi.RouteCtxKey, rctx))
}

func TestTrackingHandler_List_ReturnsTrackedUsers(t *testing.T) {
	user := domain.User{ID: uuid.New()}
	tracking := &fakeTrackingService{tracked: []domain.User{{Email: "friend@example.com"}}}
	h := handler.NewTrackingHandler(tracking)

	rec := httptest.NewRecorder()
	h.List(rec, requestAsUser(http.MethodGet, "/api/v1/tracking", "", user))

	require.Equal(t, http.StatusOK, rec.Code)
	assert.Contains(t, rec.Body.String(), "friend@example.com")
}

func TestTrackingHandler_Add_ReturnsConflictWhenAlreadyExists(t *testing.T) {
	user := domain.User{ID: uuid.New()}
	tracking := &fakeTrackingService{addErr: domain.ErrAlreadyExists}
	h := handler.NewTrackingHandler(tracking)

	rec := httptest.NewRecorder()
	h.Add(rec, requestAsUser(http.MethodPost, "/api/v1/tracking", `{"email":"friend@example.com"}`, user))

	assert.Equal(t, http.StatusConflict, rec.Code)
	assert.Equal(t, "friend@example.com", tracking.lastAddEmail)
}

func TestTrackingHandler_Add_ReturnsNotFoundForUnknownEmail(t *testing.T) {
	user := domain.User{ID: uuid.New()}
	tracking := &fakeTrackingService{addErr: domain.ErrNotFound}
	h := handler.NewTrackingHandler(tracking)

	rec := httptest.NewRecorder()
	h.Add(rec, requestAsUser(http.MethodPost, "/api/v1/tracking", `{"email":"ghost@example.com"}`, user))

	assert.Equal(t, http.StatusNotFound, rec.Code)
}

func TestTrackingHandler_Remove_RejectsInvalidUUID(t *testing.T) {
	user := domain.User{ID: uuid.New()}
	h := handler.NewTrackingHandler(&fakeTrackingService{})

	req := withURLParam(requestAsUser(http.MethodDelete, "/api/v1/tracking/not-a-uuid", "", user), "trackedUserID", "not-a-uuid")
	rec := httptest.NewRecorder()
	h.Remove(rec, req)

	assert.Equal(t, http.StatusBadRequest, rec.Code)
}

func TestTrackingHandler_Remove_Succeeds(t *testing.T) {
	user := domain.User{ID: uuid.New()}
	h := handler.NewTrackingHandler(&fakeTrackingService{})

	targetID := uuid.New()
	req := withURLParam(requestAsUser(http.MethodDelete, "/api/v1/tracking/"+targetID.String(), "", user), "trackedUserID", targetID.String())
	rec := httptest.NewRecorder()
	h.Remove(rec, req)

	assert.Equal(t, http.StatusNoContent, rec.Code)
}
