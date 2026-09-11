package handler

import (
	"log/slog"
	"net/http"
	"time"

	"github.com/go-chi/chi/v5"
	"github.com/go-chi/chi/v5/middleware"
	"github.com/go-chi/cors"

	"github.com/Rauden0/bubutracker-api/internal/auth"
	"github.com/Rauden0/bubutracker-api/internal/httpserver"
)

// Deps are the constructed dependencies the router wires into handlers.
type Deps struct {
	Logger         *slog.Logger
	Verifier       *auth.Verifier
	Health         *HealthHandler
	Users          UserService
	Locations      LocationService
	Tracking       TrackingService
	CORSOrigins    []string
	RequestTimeout time.Duration
}

// NewRouter builds the full HTTP route tree: public health checks, and an
// Auth0-authenticated /api/v1 surface for users, locations, and tracking.
func NewRouter(d Deps) http.Handler {
	r := chi.NewRouter()

	r.Use(middleware.RequestID)
	r.Use(middleware.RealIP)
	r.Use(httpserver.RequestLogger(d.Logger))
	r.Use(middleware.Recoverer)
	r.Use(middleware.Timeout(d.RequestTimeout))
	r.Use(cors.Handler(cors.Options{
		AllowedOrigins:   d.CORSOrigins,
		AllowedMethods:   []string{http.MethodGet, http.MethodPost, http.MethodPatch, http.MethodPut, http.MethodDelete},
		AllowedHeaders:   []string{"Authorization", "Content-Type"},
		AllowCredentials: true,
	}))

	r.Get("/healthz", d.Health.Live)
	r.Get("/readyz", d.Health.Ready)

	userHandler := NewUserHandler(d.Users)
	locationHandler := NewLocationHandler(d.Users, d.Locations)
	trackingHandler := NewTrackingHandler(d.Users, d.Tracking)

	r.Route("/api/v1", func(r chi.Router) {
		r.Use(d.Verifier.Middleware)

		r.Get("/users/me", userHandler.Me)
		r.Patch("/users/me", userHandler.UpdateMe)

		r.Post("/locations/me", locationHandler.UpdateMine)
		r.Get("/locations/tracked", locationHandler.Tracked)

		r.Get("/tracking", trackingHandler.List)
		r.Post("/tracking", trackingHandler.Add)
		r.Delete("/tracking/{trackedUserID}", trackingHandler.Remove)
	})

	return r
}
