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
	AddTracking(ctx context.Context, trackerID uuid.UUID, email string) error
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

	resp := make([]userProfileResponse, len(tracked))
	for i, u := range tracked {
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

	httpserver.WriteJSON(w, http.StatusCreated, map[string]string{"message": "user added to tracking list"})
}

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
