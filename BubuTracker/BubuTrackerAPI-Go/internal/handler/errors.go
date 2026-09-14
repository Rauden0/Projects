package handler

import (
	"errors"
	"fmt"
	"net/http"

	"github.com/Rauden0/bubutracker-api/internal/domain"
)

func badRequest(msg string) error {
	return fmt.Errorf("%w: %s", domain.ErrInvalidArgument, msg)
}

func decodeError(err error) error {
	var tooLarge *http.MaxBytesError
	if errors.As(err, &tooLarge) {
		return domain.ErrPayloadTooLarge
	}
	return badRequest("malformed request body")
}
