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

type userProfileResponse struct {
	ID          uuid.UUID `json:"id"`
	Email       string    `json:"email"`
	FirstName   string    `json:"firstName"`
	LastName    string    `json:"lastName"`
	MarkerColor string    `json:"markerColor"`
	CreatedAt   time.Time `json:"createdAt"`
}

func toUserProfileResponse(u domain.User) userProfileResponse {
	return userProfileResponse{
		ID:          u.ID,
		Email:       u.Email,
		FirstName:   u.FirstName,
		LastName:    u.LastName,
		MarkerColor: u.MarkerColor,
		CreatedAt:   u.CreatedAt,
	}
}

type updateUserRequest struct {
	FirstName   *string `json:"firstName"`
	LastName    *string `json:"lastName"`
	MarkerColor *string `json:"markerColor"`
}

type UserService interface {
	GetOrCreateBySubject(ctx context.Context, subjectID, email string, emailVerified *bool, firstName, lastName string) (domain.User, error)
	UpdateProfile(ctx context.Context, userID uuid.UUID, firstName, lastName, markerColor *string) (domain.User, error)
	DeleteAccount(ctx context.Context, userID uuid.UUID, subjectID string) error
}

type UserHandler struct {
	users UserService
}

func NewUserHandler(users UserService) *UserHandler {
	return &UserHandler{users: users}
}

func (h *UserHandler) Me(w http.ResponseWriter, r *http.Request) {
	user := currentUserFromContext(r.Context())
	httpserver.WriteJSON(w, http.StatusOK, toUserProfileResponse(user))
}

func (h *UserHandler) UpdateMe(w http.ResponseWriter, r *http.Request) {
	current := currentUserFromContext(r.Context())

	var req updateUserRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		httpserver.WriteError(w, decodeError(err))
		return
	}

	updated, err := h.users.UpdateProfile(r.Context(), current.ID, req.FirstName, req.LastName, req.MarkerColor)
	if err != nil {
		httpserver.WriteError(w, err)
		return
	}

	httpserver.WriteJSON(w, http.StatusOK, toUserProfileResponse(updated))
}

func (h *UserHandler) DeleteMe(w http.ResponseWriter, r *http.Request) {
	current := currentUserFromContext(r.Context())

	if err := h.users.DeleteAccount(r.Context(), current.ID, current.Auth0SubjectID); err != nil {
		httpserver.WriteError(w, err)
		return
	}

	w.WriteHeader(http.StatusNoContent)
}
