package handler_test

import (
	"net/http"
	"net/http/httptest"
	"testing"

	"github.com/google/uuid"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"

	"github.com/Rauden0/bubutracker-api/internal/auth"
	"github.com/Rauden0/bubutracker-api/internal/domain"
	"github.com/Rauden0/bubutracker-api/internal/handler"
)

func TestCurrentUserMiddleware_ResolvesUserAndCallsNext(t *testing.T) {
	user := domain.User{ID: uuid.New(), Email: "alice@example.com"}
	mw := handler.CurrentUserMiddleware(&fakeUserService{user: user})

	var called bool
	next := http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		called = true
		w.WriteHeader(http.StatusOK)
	})

	req := httptest.NewRequest(http.MethodGet, "/api/v1/users/me", nil)
	req = req.WithContext(auth.NewContext(req.Context(), auth.Claims{Subject: "auth0|123", Email: "alice@example.com"}))

	rec := httptest.NewRecorder()
	mw(next).ServeHTTP(rec, req)

	require.True(t, called)
	assert.Equal(t, http.StatusOK, rec.Code)
}

func TestCurrentUserMiddleware_PropagatesResolutionError(t *testing.T) {
	mw := handler.CurrentUserMiddleware(&fakeUserService{getErr: domain.ErrUnauthenticated})

	next := http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		t.Fatal("next handler should not run when user resolution fails")
	})

	req := httptest.NewRequest(http.MethodGet, "/api/v1/users/me", nil)
	req = req.WithContext(auth.NewContext(req.Context(), auth.Claims{Subject: "auth0|123"}))

	rec := httptest.NewRecorder()
	mw(next).ServeHTTP(rec, req)

	assert.Equal(t, http.StatusUnauthorized, rec.Code)
}
