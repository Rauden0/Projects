package handler_test

import (
	"bytes"
	"context"
	"io"
	"log/slog"
	"net/http"
	"net/http/httptest"
	"testing"
	"time"

	"github.com/google/uuid"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"

	"github.com/Rauden0/bubutracker-api/internal/auth"
	"github.com/Rauden0/bubutracker-api/internal/domain"
	"github.com/Rauden0/bubutracker-api/internal/handler"
)

// alwaysUpPinger satisfies handler.Pinger without a real database, for
// router tests that don't care about readiness-check behavior.
type alwaysUpPinger struct{}

func (alwaysUpPinger) Ping(context.Context) error { return nil }

func testLogger() *slog.Logger {
	return slog.New(slog.NewTextHandler(io.Discard, nil))
}

// fakeAuthMiddleware stands in for a real Auth0 verifier: it either injects
// fixed claims (as auth.Verifier.Middleware would for a valid token) or
// rejects with 401 (as it would for a missing/invalid one).
func fakeAuthMiddleware(authenticated bool) func(http.Handler) http.Handler {
	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			if !authenticated {
				http.Error(w, `{"error":"missing bearer token"}`, http.StatusUnauthorized)
				return
			}
			ctx := auth.NewContext(r.Context(), auth.Claims{Subject: "auth0|123", Email: "alice@example.com"})
			next.ServeHTTP(w, r.WithContext(ctx))
		})
	}
}

func newTestRouter(t *testing.T, authenticated bool) http.Handler {
	t.Helper()
	return handler.NewRouter(handler.Deps{
		Logger:         testLogger(),
		AuthMiddleware: fakeAuthMiddleware(authenticated),
		Health:         handler.NewHealthHandler(alwaysUpPinger{}),
		Users:          &fakeUserService{user: domain.User{ID: uuid.New(), Email: "alice@example.com"}},
		Locations:      &fakeLocationService{},
		Tracking:       &fakeTrackingService{},
		CORSOrigins:    []string{"*"},
		RequestTimeout: 5 * time.Second,
		MaxBodyBytes:   1 << 20,
	})
}

func TestRouter_HealthzIsReachableWithoutAuth(t *testing.T) {
	srv := httptest.NewServer(newTestRouter(t, false))
	defer srv.Close()

	resp, err := http.Get(srv.URL + "/healthz")
	require.NoError(t, err)
	defer resp.Body.Close()

	assert.Equal(t, http.StatusOK, resp.StatusCode)
}

func TestRouter_ApiV1RejectsUnauthenticatedRequests(t *testing.T) {
	srv := httptest.NewServer(newTestRouter(t, false))
	defer srv.Close()

	resp, err := http.Get(srv.URL + "/api/v1/users/me")
	require.NoError(t, err)
	defer resp.Body.Close()

	assert.Equal(t, http.StatusUnauthorized, resp.StatusCode)
}

func TestRouter_ApiV1AllowsAuthenticatedRequests(t *testing.T) {
	srv := httptest.NewServer(newTestRouter(t, true))
	defer srv.Close()

	resp, err := http.Get(srv.URL + "/api/v1/users/me")
	require.NoError(t, err)
	defer resp.Body.Close()

	assert.Equal(t, http.StatusOK, resp.StatusCode)
}

func TestRouter_RejectsOversizedBody(t *testing.T) {
	srv := httptest.NewServer(newTestRouter(t, true))
	defer srv.Close()

	// A syntactically-open JSON string of padding, not arbitrary zero bytes:
	// the decoder must keep pulling bytes (rather than failing on the first
	// invalid token) so it actually hits the MaxBodyBytes limit instead of a
	// plain syntax error.
	oversized := append([]byte(`{"padding":"`), bytes.Repeat([]byte("a"), 2<<20)...) // 2 MiB against the 1 MiB test limit
	req, err := http.NewRequest(http.MethodPost, srv.URL+"/api/v1/locations/me", bytes.NewReader(oversized))
	require.NoError(t, err)

	resp, err := http.DefaultClient.Do(req)
	require.NoError(t, err)
	defer resp.Body.Close()

	assert.Equal(t, http.StatusRequestEntityTooLarge, resp.StatusCode)
}
