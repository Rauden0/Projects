package handler

import (
	"context"
	"encoding/json"
	"net/http"
	"time"

	"github.com/google/uuid"

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

// Latitude, Longitude, and UpdatedAt are nullable: a tracked user who hasn't
// reported a location yet still appears in the list, just without these.
type trackedLocationResponse struct {
	UserID    uuid.UUID  `json:"userId"`
	Email     string     `json:"email"`
	FirstName string     `json:"firstName"`
	LastName  string     `json:"lastName"`
	Latitude  *float64   `json:"latitude"`
	Longitude *float64   `json:"longitude"`
	UpdatedAt *time.Time `json:"updatedAt"`
}

func toTrackedLocationResponse(tl domain.TrackedLocation) trackedLocationResponse {
	resp := trackedLocationResponse{
		UserID:    tl.User.ID,
		Email:     tl.User.Email,
		FirstName: tl.User.FirstName,
		LastName:  tl.User.LastName,
	}
	if tl.Location != nil {
		resp.Latitude = &tl.Location.Latitude
		resp.Longitude = &tl.Location.Longitude
		resp.UpdatedAt = &tl.Location.UpdatedAt
	}
	return resp
}

// LocationService is the subset of service.LocationService this handler needs.
type LocationService interface {
	UpdateMyLocation(ctx context.Context, userID uuid.UUID, latitude, longitude float64) (domain.Location, error)
	GetTrackedLocations(ctx context.Context, trackerID uuid.UUID) ([]domain.TrackedLocation, error)
}

type LocationHandler struct {
	locations LocationService
}

func NewLocationHandler(locations LocationService) *LocationHandler {
	return &LocationHandler{locations: locations}
}

func (h *LocationHandler) UpdateMine(w http.ResponseWriter, r *http.Request) {
	user := currentUserFromContext(r.Context())

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
	user := currentUserFromContext(r.Context())

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
