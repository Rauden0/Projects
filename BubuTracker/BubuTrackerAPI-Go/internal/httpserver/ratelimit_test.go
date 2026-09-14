package httpserver_test

import (
	"net/http"
	"net/http/httptest"
	"testing"
	"time"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"

	"github.com/Rauden0/bubutracker-api/internal/httpserver"
)

func sameKey(*http.Request) string { return "k" }

func TestRateLimit_AllowsUpToLimitThenRejects(t *testing.T) {
	calls := 0
	next := http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		calls++
		w.WriteHeader(http.StatusOK)
	})

	limited := httpserver.RateLimit(3, time.Minute, sameKey)(next)

	for i := 0; i < 3; i++ {
		rec := httptest.NewRecorder()
		limited.ServeHTTP(rec, httptest.NewRequest(http.MethodPost, "/", nil))
		require.Equalf(t, http.StatusOK, rec.Code, "request %d should be allowed", i+1)
	}

	rec := httptest.NewRecorder()
	limited.ServeHTTP(rec, httptest.NewRequest(http.MethodPost, "/", nil))

	assert.Equal(t, http.StatusTooManyRequests, rec.Code)
	assert.Equal(t, 3, calls, "the 4th request must not reach the handler")
}

func TestRateLimit_ScopesByKeyIndependently(t *testing.T) {
	var lastKey string
	keyed := func(r *http.Request) string { return r.Header.Get("X-User") }

	next := http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		lastKey = r.Header.Get("X-User")
		w.WriteHeader(http.StatusOK)
	})
	limited := httpserver.RateLimit(1, time.Minute, keyed)(next)

	req1 := httptest.NewRequest(http.MethodPost, "/", nil)
	req1.Header.Set("X-User", "alice")
	rec1 := httptest.NewRecorder()
	limited.ServeHTTP(rec1, req1)
	require.Equal(t, http.StatusOK, rec1.Code)

	req2 := httptest.NewRequest(http.MethodPost, "/", nil)
	req2.Header.Set("X-User", "bob")
	rec2 := httptest.NewRecorder()
	limited.ServeHTTP(rec2, req2)
	assert.Equal(t, http.StatusOK, rec2.Code)
	assert.Equal(t, "bob", lastKey)

	req3 := httptest.NewRequest(http.MethodPost, "/", nil)
	req3.Header.Set("X-User", "alice")
	rec3 := httptest.NewRecorder()
	limited.ServeHTTP(rec3, req3)
	assert.Equal(t, http.StatusTooManyRequests, rec3.Code)
}

func TestRateLimit_ResetsAfterWindowElapses(t *testing.T) {
	next := http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		w.WriteHeader(http.StatusOK)
	})
	limited := httpserver.RateLimit(1, 20*time.Millisecond, sameKey)(next)

	rec := httptest.NewRecorder()
	limited.ServeHTTP(rec, httptest.NewRequest(http.MethodPost, "/", nil))
	require.Equal(t, http.StatusOK, rec.Code)

	rec = httptest.NewRecorder()
	limited.ServeHTTP(rec, httptest.NewRequest(http.MethodPost, "/", nil))
	require.Equal(t, http.StatusTooManyRequests, rec.Code)

	time.Sleep(30 * time.Millisecond)

	rec = httptest.NewRecorder()
	limited.ServeHTTP(rec, httptest.NewRequest(http.MethodPost, "/", nil))
	assert.Equal(t, http.StatusOK, rec.Code, "a new window should reset the count")
}
