package handler_test

import (
	"bytes"
	"context"
	"encoding/json"
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

// panickyLocationService simulates an unexpected bug reaching production: a
// handler dependency panics instead of returning an error.
type panickyLocationService struct{ fakeLocationService }

func (panickyLocationService) UpdateMyLocation(context.Context, uuid.UUID, float64, float64) (domain.Location, error) {
	panic("simulated unexpected failure")
}

func TestRouter_PanicRecoveryReturnsJSONErrorEnvelope(t *testing.T) {
	router := handler.NewRouter(handler.Deps{
		Logger:         testLogger(),
		AuthMiddleware: fakeAuthMiddleware(true),
		Health:         handler.NewHealthHandler(alwaysUpPinger{}),
		Users:          &fakeUserService{user: domain.User{ID: uuid.New(), Email: "alice@example.com"}},
		Locations:      &panickyLocationService{},
		Tracking:       &fakeTrackingService{},
		CORSOrigins:    []string{"*"},
		RequestTimeout: 5 * time.Second,
		MaxBodyBytes:   1 << 20,
	})
	srv := httptest.NewServer(router)
	defer srv.Close()

	resp, err := http.Post(srv.URL+"/api/v1/locations/me", "application/json", bytes.NewReader([]byte(`{"latitude":1,"longitude":1}`)))
	require.NoError(t, err)
	defer resp.Body.Close()

	require.Equal(t, http.StatusInternalServerError, resp.StatusCode)

	var body struct {
		Error struct {
			Code    string `json:"code"`
			Message string `json:"message"`
		} `json:"error"`
	}
	require.NoError(t, json.NewDecoder(resp.Body).Decode(&body))
	assert.NotEmpty(t, body.Error.Code)
	assert.NotEmpty(t, body.Error.Message)
}

// TestRouter_TrackingAddIsRateLimited is the regression test for the email
// enumeration finding: POST /tracking distinguishes "unknown email" (404)
// from "known email" (201/409), so without a rate limit an authenticated
// user could probe arbitrary emails to discover who has an account.
func TestRouter_TrackingAddIsRateLimited(t *testing.T) {
	srv := httptest.NewServer(newTestRouter(t, true))
	defer srv.Close()

	postTracking := func() *http.Response {
		resp, err := http.Post(srv.URL+"/api/v1/tracking", "application/json", bytes.NewReader([]byte(`{"email":"ghost@example.com"}`)))
		require.NoError(t, err)
		return resp
	}

	for i := 0; i < 10; i++ {
		resp := postTracking()
		resp.Body.Close()
		require.NotEqualf(t, http.StatusTooManyRequests, resp.StatusCode, "request %d should still be within the limit", i+1)
	}

	resp := postTracking()
	defer resp.Body.Close()
	assert.Equal(t, http.StatusTooManyRequests, resp.StatusCode, "the 11th request in a minute should be rate limited")
}

// TestRouter_TrackingSubRoutesAreNotShadowedByTheWildcardRoute guards
// against a routing regression: DELETE /tracking/{trackedUserID} is a
// wildcard on the same path prefix as the literal GET /tracking/requests
// and GET /tracking/followers. If chi's static-route-wins-over-wildcard
// behavior ever broke (or a route got reordered into the wrong place),
// these would 400 on "requests"/"followers" as an invalid UUID instead of
// reaching the intended handler.
func TestRouter_TrackingSubRoutesAreNotShadowedByTheWildcardRoute(t *testing.T) {
	srv := httptest.NewServer(newTestRouter(t, true))
	defer srv.Close()

	for _, path := range []string{"/api/v1/tracking/requests", "/api/v1/tracking/followers"} {
		resp, err := http.Get(srv.URL + path)
		require.NoError(t, err)
		resp.Body.Close()
		assert.Equal(t, http.StatusOK, resp.StatusCode, "GET %s", path)
	}
}

func TestRouter_TrackingAccept_RejectsUnauthenticated(t *testing.T) {
	srv := httptest.NewServer(newTestRouter(t, false))
	defer srv.Close()

	resp, err := http.Post(srv.URL+"/api/v1/tracking/requests/"+uuid.New().String()+"/accept", "application/json", nil)
	require.NoError(t, err)
	defer resp.Body.Close()

	assert.Equal(t, http.StatusUnauthorized, resp.StatusCode)
}
