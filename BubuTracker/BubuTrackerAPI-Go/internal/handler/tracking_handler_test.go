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
	tracked           []domain.User
	incomingRequests  []domain.User
	followers         []domain.User
	addErr            error
	acceptErr         error
	removeErr         error
	lastAddEmail      string
	lastAcceptTracker uuid.UUID
	lastRemoveTracker uuid.UUID
	lastRemoveTracked uuid.UUID
}

func (f *fakeTrackingService) ListTracked(_ context.Context, _ uuid.UUID) ([]domain.User, error) {
	return f.tracked, nil
}

func (f *fakeTrackingService) ListIncomingRequests(_ context.Context, _ uuid.UUID) ([]domain.User, error) {
	return f.incomingRequests, nil
}

func (f *fakeTrackingService) ListFollowers(_ context.Context, _ uuid.UUID) ([]domain.User, error) {
	return f.followers, nil
}

func (f *fakeTrackingService) AddTracking(_ context.Context, _ uuid.UUID, email string) error {
	f.lastAddEmail = email
	return f.addErr
}

func (f *fakeTrackingService) AcceptTracking(_ context.Context, _, trackerID uuid.UUID) error {
	f.lastAcceptTracker = trackerID
	return f.acceptErr
}

func (f *fakeTrackingService) RemoveTracking(_ context.Context, trackerID, trackedUserID uuid.UUID) error {
	f.lastRemoveTracker = trackerID
	f.lastRemoveTracked = trackedUserID
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

func TestTrackingHandler_Requests_ReturnsIncomingRequests(t *testing.T) {
	user := domain.User{ID: uuid.New()}
	tracking := &fakeTrackingService{incomingRequests: []domain.User{{Email: "wannabe@example.com"}}}
	h := handler.NewTrackingHandler(tracking)

	rec := httptest.NewRecorder()
	h.Requests(rec, requestAsUser(http.MethodGet, "/api/v1/tracking/requests", "", user))

	require.Equal(t, http.StatusOK, rec.Code)
	assert.Contains(t, rec.Body.String(), "wannabe@example.com")
}

func TestTrackingHandler_Followers_ReturnsFollowers(t *testing.T) {
	user := domain.User{ID: uuid.New()}
	tracking := &fakeTrackingService{followers: []domain.User{{Email: "follower@example.com"}}}
	h := handler.NewTrackingHandler(tracking)

	rec := httptest.NewRecorder()
	h.Followers(rec, requestAsUser(http.MethodGet, "/api/v1/tracking/followers", "", user))

	require.Equal(t, http.StatusOK, rec.Code)
	assert.Contains(t, rec.Body.String(), "follower@example.com")
}

func TestTrackingHandler_Accept_RejectsInvalidUUID(t *testing.T) {
	user := domain.User{ID: uuid.New()}
	h := handler.NewTrackingHandler(&fakeTrackingService{})

	req := withURLParam(requestAsUser(http.MethodPost, "/api/v1/tracking/requests/not-a-uuid/accept", "", user), "trackerID", "not-a-uuid")
	rec := httptest.NewRecorder()
	h.Accept(rec, req)

	assert.Equal(t, http.StatusBadRequest, rec.Code)
}

func TestTrackingHandler_Accept_ReturnsNotFoundWhenNoPendingRequest(t *testing.T) {
	user := domain.User{ID: uuid.New()}
	tracking := &fakeTrackingService{acceptErr: domain.ErrNotFound}
	h := handler.NewTrackingHandler(tracking)

	trackerID := uuid.New()
	req := withURLParam(requestAsUser(http.MethodPost, "/api/v1/tracking/requests/"+trackerID.String()+"/accept", "", user), "trackerID", trackerID.String())
	rec := httptest.NewRecorder()
	h.Accept(rec, req)

	assert.Equal(t, http.StatusNotFound, rec.Code)
}

func TestTrackingHandler_Accept_Succeeds(t *testing.T) {
	user := domain.User{ID: uuid.New()}
	tracking := &fakeTrackingService{}
	h := handler.NewTrackingHandler(tracking)

	trackerID := uuid.New()
	req := withURLParam(requestAsUser(http.MethodPost, "/api/v1/tracking/requests/"+trackerID.String()+"/accept", "", user), "trackerID", trackerID.String())
	rec := httptest.NewRecorder()
	h.Accept(rec, req)

	assert.Equal(t, http.StatusNoContent, rec.Code)
	assert.Equal(t, trackerID, tracking.lastAcceptTracker)
}

func TestTrackingHandler_RemoveFollower_RejectsInvalidUUID(t *testing.T) {
	user := domain.User{ID: uuid.New()}
	h := handler.NewTrackingHandler(&fakeTrackingService{})

	req := withURLParam(requestAsUser(http.MethodDelete, "/api/v1/tracking/followers/not-a-uuid", "", user), "trackerID", "not-a-uuid")
	rec := httptest.NewRecorder()
	h.RemoveFollower(rec, req)

	assert.Equal(t, http.StatusBadRequest, rec.Code)
}

func TestTrackingHandler_RemoveFollower_RemovesEdgeFromTheTrackedSide(t *testing.T) {
	user := domain.User{ID: uuid.New()}
	tracking := &fakeTrackingService{}
	h := handler.NewTrackingHandler(tracking)

	trackerID := uuid.New()
	req := withURLParam(requestAsUser(http.MethodDelete, "/api/v1/tracking/followers/"+trackerID.String(), "", user), "trackerID", trackerID.String())
	rec := httptest.NewRecorder()
	h.RemoveFollower(rec, req)

	assert.Equal(t, http.StatusNoContent, rec.Code)
	// Args must be swapped relative to Remove: trackerID from the URL, and
	// the current (tracked) user as trackedUserID - not the other way
	// around, or this would let a user revoke someone ELSE's follower.
	assert.Equal(t, trackerID, tracking.lastRemoveTracker)
	assert.Equal(t, user.ID, tracking.lastRemoveTracked)
}
