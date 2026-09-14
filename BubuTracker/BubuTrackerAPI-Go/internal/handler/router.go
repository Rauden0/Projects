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

type Deps struct {
	Logger         *slog.Logger
	AuthMiddleware func(http.Handler) http.Handler
	Health         *HealthHandler
	Users          UserService
	Locations      LocationService
	Tracking       TrackingService
	CORSOrigins    []string
	RequestTimeout time.Duration
	MaxBodyBytes   int64
}

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
		// AllowCredentials:true with "*" origin is rejected by browsers.
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
		r.Delete("/users/me", userHandler.DeleteMe)

		r.Post("/locations/me", locationHandler.UpdateMine)
		r.Get("/locations/tracked", locationHandler.Tracked)

		r.Get("/tracking", trackingHandler.List)
		// AddTracking silently no-ops for an unknown email (see tracking_service.go)
		// so this always returns 201, never a 404 that would enable email
		// enumeration; the rate limit remains as a second line of defense.
		r.With(httpserver.RateLimit(10, time.Minute, currentUserRateLimitKey)).
			Post("/tracking", trackingHandler.Add)
		r.Delete("/tracking/{trackedUserID}", trackingHandler.Remove)

		r.Get("/tracking/outgoing", trackingHandler.Outgoing)
		r.Get("/tracking/requests", trackingHandler.Requests)
		r.Post("/tracking/requests/{trackerID}/accept", trackingHandler.Accept)
		r.Get("/tracking/followers", trackingHandler.Followers)
		r.Delete("/tracking/followers/{trackerID}", trackingHandler.RemoveFollower)
	})

	return r
}
