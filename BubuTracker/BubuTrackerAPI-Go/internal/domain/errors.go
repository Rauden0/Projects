package domain

import "errors"

// Sentinel errors returned by the service layer. Handlers translate these
// into HTTP status codes; nothing above the service layer should need to
// know about SQL error codes or driver-specific types.
var (
	ErrNotFound        = errors.New("resource not found")
	ErrAlreadyExists   = errors.New("resource already exists")
	ErrInvalidArgument = errors.New("invalid argument")
	ErrSelfTracking    = errors.New("cannot track yourself")
	ErrUnauthenticated = errors.New("missing or invalid authentication")
)
