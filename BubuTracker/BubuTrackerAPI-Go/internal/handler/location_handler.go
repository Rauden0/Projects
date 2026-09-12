package handler

import (
	"context"
	"encoding/json"
	"net/http"
	"time"

	"github.com/google/uuid"

	"github.com/Rauden0/bubutracker-api/internal/auth"
	"github.com/Rauden0/bubutracker-api/internal/domain"
	"github.com/Rauden0/bubutracker-api/internal/httpserver"
)

// Latitude and Longitude are pointers so a missing field can be told apart
// from an explicit 0 (a valid coordinate, on the equator/prime meridian)
// and rejected instead of silently overwriting the user's location.
type updateLocationRequest struct {
	Latitude  *float64 `json:"latitude"`
	Longitude *float64 `json:"longitude"`
}

type locationResponse struct {
	UserID    uuid.UUID `json:"userId"`
	Latitude  float64   `json:"latitude"`
	Longitude float64   `json:"longitude"`
	UpdatedAt time.Time `json:"updatedAt"`
}

type trackedLocationResponse struct {
	UserID    uuid.UUID `json:"userId"`
	Email     string    `json:"email"`
	FirstName string    `json:"firstName"`
	LastName  string    `json:"lastName"`
	Latitude  float64   `json:"latitude"`
	Longitude float64   `json:"longitude"`
	UpdatedAt time.Time `json:"updatedAt"`
}

func toTrackedLocationResponse(tl domain.TrackedLocation) trackedLocationResponse {
	return trackedLocationResponse{
		UserID:    tl.User.ID,
		Email:     tl.User.Email,
		FirstName: tl.User.FirstName,
		LastName:  tl.User.LastName,
		Latitude:  tl.Location.Latitude,
		Longitude: tl.Location.Longitude,
		UpdatedAt: tl.Location.UpdatedAt,
	}
}

// LocationService is the subset of service.LocationService this handler needs.
type LocationService interface {
	UpdateMyLocation(ctx context.Context, userID uuid.UUID, latitude, longitude float64) (domain.Location, error)
	GetTrackedLocations(ctx context.Context, trackerID uuid.UUID) ([]domain.TrackedLocation, error)
}

type LocationHandler struct {
	users     UserService
	locations LocationService
}

func NewLocationHandler(users UserService, locations LocationService) *LocationHandler {
	return &LocationHandler{users: users, locations: locations}
}

func (h *LocationHandler) UpdateMine(w http.ResponseWriter, r *http.Request) {
	claims := auth.FromContext(r.Context())

	user, err := h.users.GetOrCreateBySubject(r.Context(), claims.Subject, claims.Email, claims.FirstName, claims.LastName)
	if err != nil {
		httpserver.WriteError(w, err)
		return
	}

	var req updateLocationRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		httpserver.WriteError(w, decodeError(err))
		return
	}
	if req.Latitude == nil || req.Longitude == nil {
		httpserver.WriteError(w, badRequest("latitude and longitude are required"))
		return
	}

	loc, err := h.locations.UpdateMyLocation(r.Context(), user.ID, *req.Latitude, *req.Longitude)
	if err != nil {
		httpserver.WriteError(w, err)
		return
	}

	httpserver.WriteJSON(w, http.StatusOK, locationResponse{
		UserID:    loc.UserID,
		Latitude:  loc.Latitude,
		Longitude: loc.Longitude,
		UpdatedAt: loc.UpdatedAt,
	})
}

func (h *LocationHandler) Tracked(w http.ResponseWriter, r *http.Request) {
	claims := auth.FromContext(r.Context())

	user, err := h.users.GetOrCreateBySubject(r.Context(), claims.Subject, claims.Email, claims.FirstName, claims.LastName)
	if err != nil {
		httpserver.WriteError(w, err)
		return
	}

	locations, err := h.locations.GetTrackedLocations(r.Context(), user.ID)
	if err != nil {
		httpserver.WriteError(w, err)
		return
	}

	resp := make([]trackedLocationResponse, len(locations))
	for i, l := range locations {
		resp[i] = toTrackedLocationResponse(l)
	}

	httpserver.WriteJSON(w, http.StatusOK, resp)
}
