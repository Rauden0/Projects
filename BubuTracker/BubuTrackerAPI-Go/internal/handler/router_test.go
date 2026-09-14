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

type alwaysUpPinger struct{}

func (alwaysUpPinger) Ping(context.Context) error { return nil }

func testLogger() *slog.Logger {
	return slog.New(slog.NewTextHandler(io.Discard, nil))
}

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

	// Open JSON string so the decoder keeps reading until MaxBodyBytes, not a syntax error.
	oversized := append([]byte(`{"padding":"`), bytes.Repeat([]byte("a"), 2<<20)...) // 2 MiB against the 1 MiB test limit
	req, err := http.NewRequest(http.MethodPost, srv.URL+"/api/v1/locations/me", bytes.NewReader(oversized))
	require.NoError(t, err)

	resp, err := http.DefaultClient.Do(req)
	require.NoError(t, err)
	defer resp.Body.Close()

	assert.Equal(t, http.StatusRequestEntityTooLarge, resp.StatusCode)
}

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

func TestRouter_TrackingSubRoutesAreNotShadowedByTheWildcardRoute(t *testing.T) {
	srv := httptest.NewServer(newTestRouter(t, true))
	defer srv.Close()

	for _, path := range []string{"/api/v1/tracking/requests", "/api/v1/tracking/followers", "/api/v1/tracking/outgoing"} {
		resp, err := http.Get(srv.URL + path)
		require.NoError(t, err)
		resp.Body.Close()
		assert.Equal(t, http.StatusOK, resp.StatusCode, "GET %s", path)
	}
}

func TestRouter_DeleteMe_RejectsUnauthenticated(t *testing.T) {
	srv := httptest.NewServer(newTestRouter(t, false))
	defer srv.Close()

	req, err := http.NewRequest(http.MethodDelete, srv.URL+"/api/v1/users/me", nil)
	require.NoError(t, err)
	resp, err := http.DefaultClient.Do(req)
	require.NoError(t, err)
	defer resp.Body.Close()

	assert.Equal(t, http.StatusUnauthorized, resp.StatusCode)
}

func TestRouter_DeleteMe_SucceedsWhenAuthenticated(t *testing.T) {
	srv := httptest.NewServer(newTestRouter(t, true))
	defer srv.Close()

	req, err := http.NewRequest(http.MethodDelete, srv.URL+"/api/v1/users/me", nil)
	require.NoError(t, err)
	resp, err := http.DefaultClient.Do(req)
	require.NoError(t, err)
	defer resp.Body.Close()

	assert.Equal(t, http.StatusNoContent, resp.StatusCode)
}

func TestRouter_TrackingAccept_RejectsUnauthenticated(t *testing.T) {
	srv := httptest.NewServer(newTestRouter(t, false))
	defer srv.Close()

	resp, err := http.Post(srv.URL+"/api/v1/tracking/requests/"+uuid.New().String()+"/accept", "application/json", nil)
	require.NoError(t, err)
	defer resp.Body.Close()

	assert.Equal(t, http.StatusUnauthorized, resp.StatusCode)
}
