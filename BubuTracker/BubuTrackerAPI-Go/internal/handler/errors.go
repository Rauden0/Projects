// Package handler contains HTTP handlers: request decoding, response
// encoding, and delegation to the service layer. No business logic lives
// here.
package handler

import (
	"fmt"

	"github.com/Rauden0/bubutracker-api/internal/domain"
)

// badRequest wraps domain.ErrInvalidArgument with a handler-local message,
// for validation failures caught before the service layer is even reached
// (e.g. malformed JSON).
func badRequest(msg string) error {
	return fmt.Errorf("%w: %s", domain.ErrInvalidArgument, msg)
}
