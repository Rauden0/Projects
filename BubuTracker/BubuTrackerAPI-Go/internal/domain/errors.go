package domain

import "errors"

var (
	ErrNotFound        = errors.New("resource not found")
	ErrAlreadyExists   = errors.New("resource already exists")
	ErrInvalidArgument = errors.New("invalid argument")
	ErrSelfTracking    = errors.New("cannot track yourself")
	ErrEmailConflict   = errors.New("email already associated with a different account")
	ErrUnauthenticated = errors.New("missing or invalid authentication")
	ErrPayloadTooLarge = errors.New("request body too large")
	ErrRateLimited     = errors.New("too many requests")
)
