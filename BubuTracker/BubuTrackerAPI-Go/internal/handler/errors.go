// Package handler contains HTTP handlers: request decoding, response
// encoding, and delegation to the service layer. No business logic lives
// here.
package handler

import (
	"errors"
	"fmt"
	"net/http"

	"github.com/Rauden0/bubutracker-api/internal/domain"
)

// badRequest wraps domain.ErrInvalidArgument with a handler-local message,
// for validation failures caught before the service layer is even reached
// (e.g. malformed JSON).
func badRequest(msg string) error {
	return fmt.Errorf("%w: %s", domain.ErrInvalidArgument, msg)
}

// decodeError turns a json.Decoder error into the right domain error: a body
// rejected by httpserver.MaxBodyBytes reports 413 instead of being lumped in
// with ordinary malformed JSON.
func decodeError(err error) error {
	var tooLarge *http.MaxBytesError
	if errors.As(err, &tooLarge) {
		return domain.ErrPayloadTooLarge
	}
	return badRequest("malformed request body")
}
