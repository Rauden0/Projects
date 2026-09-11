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

type userProfileResponse struct {
	ID        uuid.UUID `json:"id"`
	Email     string    `json:"email"`
	FirstName string    `json:"firstName"`
	LastName  string    `json:"lastName"`
	CreatedAt time.Time `json:"createdAt"`
}

func toUserProfileResponse(u domain.User) userProfileResponse {
	return userProfileResponse{
		ID:        u.ID,
		Email:     u.Email,
		FirstName: u.FirstName,
		LastName:  u.LastName,
		CreatedAt: u.CreatedAt,
	}
}

type updateUserRequest struct {
	FirstName *string `json:"firstName"`
	LastName  *string `json:"lastName"`
}

// UserService is the subset of service.UserService each handler needs.
type UserService interface {
	GetOrCreateBySubject(ctx context.Context, subjectID, email, firstName, lastName string) (domain.User, error)
	UpdateProfile(ctx context.Context, userID uuid.UUID, firstName, lastName *string) (domain.User, error)
}

type UserHandler struct {
	users UserService
}

func NewUserHandler(users UserService) *UserHandler {
	return &UserHandler{users: users}
}

func (h *UserHandler) Me(w http.ResponseWriter, r *http.Request) {
	claims := auth.FromContext(r.Context())

	user, err := h.users.GetOrCreateBySubject(r.Context(), claims.Subject, claims.Email, claims.FirstName, claims.LastName)
	if err != nil {
		httpserver.WriteError(w, err)
		return
	}

	httpserver.WriteJSON(w, http.StatusOK, toUserProfileResponse(user))
}

func (h *UserHandler) UpdateMe(w http.ResponseWriter, r *http.Request) {
	claims := auth.FromContext(r.Context())

	current, err := h.users.GetOrCreateBySubject(r.Context(), claims.Subject, claims.Email, claims.FirstName, claims.LastName)
	if err != nil {
		httpserver.WriteError(w, err)
		return
	}

	var req updateUserRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		httpserver.WriteError(w, badRequest("malformed request body"))
		return
	}

	updated, err := h.users.UpdateProfile(r.Context(), current.ID, req.FirstName, req.LastName)
	if err != nil {
		httpserver.WriteError(w, err)
		return
	}

	httpserver.WriteJSON(w, http.StatusOK, toUserProfileResponse(updated))
}
