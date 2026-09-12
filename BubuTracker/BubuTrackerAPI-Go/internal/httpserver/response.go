// Package httpserver holds transport-level concerns shared by every
// handler: JSON encoding, error translation, middleware, and the HTTP
// server lifecycle. It knows nothing about business logic.
package httpserver

import (
	"encoding/json"
	"errors"
	"log/slog"
	"net/http"

	"github.com/Rauden0/bubutracker-api/internal/domain"
)

// WriteJSON encodes v as the response body with the given status code.
func WriteJSON(w http.ResponseWriter, status int, v any) {
	w.Header().Set("Content-Type", "application/json; charset=utf-8")
	w.WriteHeader(status)
	if v == nil {
		return
	}
	if err := json.NewEncoder(w).Encode(v); err != nil {
		slog.Error("failed to encode json response", "error", err)
	}
}

type errorBody struct {
	Error struct {
		Code    string `json:"code"`
		Message string `json:"message"`
	} `json:"error"`
}

// WriteError translates a domain/service error (or a handler-local one) into
// the appropriate HTTP status and a consistent JSON error envelope.
func WriteError(w http.ResponseWriter, err error) {
	status, code := statusFor(err)

	body := errorBody{}
	body.Error.Code = code
	body.Error.Message = err.Error()

	if status >= http.StatusInternalServerError {
		slog.Error("request failed", "error", err)
		body.Error.Message = "internal server error"
	}

	WriteJSON(w, status, body)
}

func statusFor(err error) (int, string) {
	switch {
	case errors.Is(err, domain.ErrNotFound):
		return http.StatusNotFound, "not_found"
	case errors.Is(err, domain.ErrAlreadyExists):
		return http.StatusConflict, "already_exists"
	case errors.Is(err, domain.ErrInvalidArgument):
		return http.StatusBadRequest, "invalid_argument"
	case errors.Is(err, domain.ErrSelfTracking):
		return http.StatusBadRequest, "self_tracking"
	case errors.Is(err, domain.ErrUnauthenticated):
		return http.StatusUnauthorized, "unauthenticated"
	case errors.Is(err, domain.ErrPayloadTooLarge):
		return http.StatusRequestEntityTooLarge, "payload_too_large"
	default:
		return http.StatusInternalServerError, "internal_error"
	}
}
