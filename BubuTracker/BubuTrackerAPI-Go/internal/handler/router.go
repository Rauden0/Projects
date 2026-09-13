package handler

import (
	"log/slog"
	"net/http"
	"time"

	"github.com/go-chi/chi/v5"
	"github.com/go-chi/chi/v5/middleware"
	"github.com/go-chi/cors"

	"github.com/Rauden0/bubutracker-api/internal/httpserver"
)

// Deps are the constructed dependencies the router wires into handlers.
type Deps struct {
	Logger *slog.Logger
	// AuthMiddleware gates the /api/v1 route group. In production this is
	// (*auth.Verifier).Middleware; tests can substitute a fake so the route
	// tree and its auth gating can be exercised without a real Auth0 tenant.
	AuthMiddleware func(http.Handler) http.Handler
	Health         *HealthHandler
	Users          UserService
	Locations      LocationService
	Tracking       TrackingService
	CORSOrigins    []string
	RequestTimeout time.Duration
	MaxBodyBytes   int64
}

// NewRouter builds the full HTTP route tree: public health checks, and an
// Auth0-authenticated /api/v1 surface for users, locations, and tracking.
func NewRouter(d Deps) http.Handler {
	r := chi.NewRouter()

	r.Use(middleware.RequestID)
	r.Use(middleware.RealIP)
	r.Use(httpserver.RequestLogger(d.Logger))
	r.Use(httpserver.Recoverer)
	r.Use(middleware.Timeout(d.RequestTimeout))
	r.Use(httpserver.MaxBodyBytes(d.MaxBodyBytes))
	r.Use(cors.Handler(cors.Options{
		AllowedOrigins: d.CORSOrigins,
		AllowedMethods: []string{http.MethodGet, http.MethodPost, http.MethodPatch, http.MethodPut, http.MethodDelete},
		AllowedHeaders: []string{"Authorization", "Content-Type"},
		// No cookies/browser credentials are used (auth is a Bearer token),
		// so this stays false. AllowCredentials:true combined with a "*"
		// origin (the default CORSOrigins) is a known CORS footgun: browsers
		// reject it outright, silently breaking any web client.
		AllowCredentials: false,
	}))

	r.Get("/healthz", d.Health.Live)
	r.Get("/readyz", d.Health.Ready)

	userHandler := NewUserHandler(d.Users)
	locationHandler := NewLocationHandler(d.Locations)
	trackingHandler := NewTrackingHandler(d.Tracking)

	r.Route("/api/v1", func(r chi.Router) {
		r.Use(d.AuthMiddleware)
		r.Use(CurrentUserMiddleware(d.Users))

		r.Get("/users/me", userHandler.Me)
		r.Patch("/users/me", userHandler.UpdateMe)

		r.Post("/locations/me", locationHandler.UpdateMine)
		r.Get("/locations/tracked", locationHandler.Tracked)

		r.Get("/tracking", trackingHandler.List)
		// Rate limited by user, not just left to the DB constraints: without
		// this, POST /tracking's distinct 404 (unknown email) vs 201/409
		// (known email) responses let an authenticated user enumerate every
		// registered email address in the system at whatever pace they like.
		r.With(httpserver.RateLimit(10, time.Minute, currentUserRateLimitKey)).
			Post("/tracking", trackingHandler.Add)
		r.Delete("/tracking/{trackedUserID}", trackingHandler.Remove)

		// The consent side: who wants to track me (Requests), who already
		// does (Followers), and accepting/rejecting/revoking that access.
		r.Get("/tracking/requests", trackingHandler.Requests)
		r.Post("/tracking/requests/{trackerID}/accept", trackingHandler.Accept)
		r.Get("/tracking/followers", trackingHandler.Followers)
		r.Delete("/tracking/followers/{trackerID}", trackingHandler.RemoveFollower)
	})

	return r
}
