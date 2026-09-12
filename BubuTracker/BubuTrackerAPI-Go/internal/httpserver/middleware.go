package httpserver

import (
	"errors"
	"log/slog"
	"net/http"
	"runtime/debug"
	"time"

	"github.com/go-chi/chi/v5/middleware"
)

// RequestLogger logs one structured line per request: method, path, status,
// duration, and the request ID chi's RequestID middleware attaches. It also
// echoes that ID back as a response header, so a client-reported issue can
// be tied to a specific server log line.
func RequestLogger(logger *slog.Logger) func(http.Handler) http.Handler {
	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			start := time.Now()
			ww := middleware.NewWrapResponseWriter(w, r.ProtoMajor)

			requestID := middleware.GetReqID(r.Context())
			if requestID != "" {
				w.Header().Set("X-Request-Id", requestID)
			}

			next.ServeHTTP(ww, r)

			logger.Info("http request",
				"method", r.Method,
				"path", r.URL.Path,
				"status", ww.Status(),
				"bytes", ww.BytesWritten(),
				"duration_ms", time.Since(start).Milliseconds(),
				"request_id", requestID,
			)
		})
	}
}

// Recoverer recovers panics and responds through the same JSON error
// envelope every other error path uses (see WriteError), instead of
// chi/middleware.Recoverer's bare, empty-body 500. A panicking handler
// should still look like any other server error to the client rather than
// silently breaking the API's documented error contract.
func Recoverer(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		defer func() {
			if rvr := recover(); rvr != nil {
				if rvr == http.ErrAbortHandler { //nolint:errorlint // sentinel value, not an error to unwrap
					// Not ours to recover: let net/http abort the response.
					panic(rvr)
				}
				slog.Error("panic recovered",
					"panic", rvr,
					"request_id", middleware.GetReqID(r.Context()),
					"stack", string(debug.Stack()),
				)
				WriteError(w, errors.New("internal server error"))
			}
		}()
		next.ServeHTTP(w, r)
	})
}

// MaxBodyBytes caps request body size so a large or runaway payload can't
// exhaust server memory before handler-level JSON decoding even runs.
func MaxBodyBytes(max int64) func(http.Handler) http.Handler {
	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			r.Body = http.MaxBytesReader(w, r.Body, max)
			next.ServeHTTP(w, r)
		})
	}
}
