package handler

import (
	"context"
	"encoding/json"
	"net/http"

	"github.com/go-chi/chi/v5"
	"github.com/google/uuid"

	"github.com/Rauden0/bubutracker-api/internal/domain"
	"github.com/Rauden0/bubutracker-api/internal/httpserver"
)

type addTrackingRequest struct {
	Email string `json:"email"`
}

// TrackingService is the subset of service.TrackingService this handler needs.
type TrackingService interface {
	ListTracked(ctx context.Context, trackerID uuid.UUID) ([]domain.User, error)
	ListIncomingRequests(ctx context.Context, trackedUserID uuid.UUID) ([]domain.User, error)
	ListFollowers(ctx context.Context, trackedUserID uuid.UUID) ([]domain.User, error)
	AddTracking(ctx context.Context, trackerID uuid.UUID, email string) error
	AcceptTracking(ctx context.Context, trackedUserID, trackerID uuid.UUID) error
	RemoveTracking(ctx context.Context, trackerID, trackedUserID uuid.UUID) error
}

type TrackingHandler struct {
	tracking TrackingService
}

func NewTrackingHandler(tracking TrackingService) *TrackingHandler {
	return &TrackingHandler{tracking: tracking}
}

func (h *TrackingHandler) List(w http.ResponseWriter, r *http.Request) {
	user := currentUserFromContext(r.Context())

	tracked, err := h.tracking.ListTracked(r.Context(), user.ID)
	if err != nil {
		httpserver.WriteError(w, err)
		return
	}

	writeUserList(w, tracked)
}

// Requests lists other users' pending requests to track the current user -
// the consent inbox they accept or reject from.
func (h *TrackingHandler) Requests(w http.ResponseWriter, r *http.Request) {
	user := currentUserFromContext(r.Context())

	requests, err := h.tracking.ListIncomingRequests(r.Context(), user.ID)
	if err != nil {
		httpserver.WriteError(w, err)
		return
	}

	writeUserList(w, requests)
}

// Followers lists users currently, with consent, tracking the current user,
// so they have ongoing visibility into (and can revoke) who has access.
func (h *TrackingHandler) Followers(w http.ResponseWriter, r *http.Request) {
	user := currentUserFromContext(r.Context())

	followers, err := h.tracking.ListFollowers(r.Context(), user.ID)
	if err != nil {
		httpserver.WriteError(w, err)
		return
	}

	writeUserList(w, followers)
}

func writeUserList(w http.ResponseWriter, users []domain.User) {
	resp := make([]userProfileResponse, len(users))
	for i, u := range users {
		resp[i] = toUserProfileResponse(u)
	}
	httpserver.WriteJSON(w, http.StatusOK, resp)
}

func (h *TrackingHandler) Add(w http.ResponseWriter, r *http.Request) {
	user := currentUserFromContext(r.Context())

	var req addTrackingRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		httpserver.WriteError(w, decodeError(err))
		return
	}

	if err := h.tracking.AddTracking(r.Context(), user.ID, req.Email); err != nil {
		httpserver.WriteError(w, err)
		return
	}

	httpserver.WriteJSON(w, http.StatusCreated, map[string]string{"message": "tracking request sent"})
}

// Accept lets the current user (the tracked party) approve a pending
// request from the tracker named in the URL, granting them location
// visibility.
func (h *TrackingHandler) Accept(w http.ResponseWriter, r *http.Request) {
	user := currentUserFromContext(r.Context())

	trackerID, err := uuid.Parse(chi.URLParam(r, "trackerID"))
	if err != nil {
		httpserver.WriteError(w, badRequest("trackerID must be a valid UUID"))
		return
	}

	if err := h.tracking.AcceptTracking(r.Context(), user.ID, trackerID); err != nil {
		httpserver.WriteError(w, err)
		return
	}

	w.WriteHeader(http.StatusNoContent)
}

// Remove lets the current user (the tracker) cancel their own pending
// request or stop actively tracking trackedUserID.
func (h *TrackingHandler) Remove(w http.ResponseWriter, r *http.Request) {
	user := currentUserFromContext(r.Context())

	trackedUserID, err := uuid.Parse(chi.URLParam(r, "trackedUserID"))
	if err != nil {
		httpserver.WriteError(w, badRequest("trackedUserID must be a valid UUID"))
		return
	}

	if err := h.tracking.RemoveTracking(r.Context(), user.ID, trackedUserID); err != nil {
		httpserver.WriteError(w, err)
		return
	}

	w.WriteHeader(http.StatusNoContent)
}

// RemoveFollower lets the current user (the tracked party) reject a pending
// request or revoke consent already given to the tracker named in the URL.
func (h *TrackingHandler) RemoveFollower(w http.ResponseWriter, r *http.Request) {
	user := currentUserFromContext(r.Context())

	trackerID, err := uuid.Parse(chi.URLParam(r, "trackerID"))
	if err != nil {
		httpserver.WriteError(w, badRequest("trackerID must be a valid UUID"))
		return
	}

	if err := h.tracking.RemoveTracking(r.Context(), trackerID, user.ID); err != nil {
		httpserver.WriteError(w, err)
		return
	}

	w.WriteHeader(http.StatusNoContent)
}
