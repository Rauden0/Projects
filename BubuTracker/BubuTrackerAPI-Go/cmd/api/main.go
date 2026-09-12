// Command api runs the BubuTracker HTTP API.
package main

import (
	"context"
	"errors"
	"fmt"
	"log/slog"
	"net/http"
	"os"
	"os/signal"
	"syscall"
	"time"

	"github.com/Rauden0/bubutracker-api/internal/auth"
	"github.com/Rauden0/bubutracker-api/internal/config"
	"github.com/Rauden0/bubutracker-api/internal/handler"
	"github.com/Rauden0/bubutracker-api/internal/service"
	"github.com/Rauden0/bubutracker-api/internal/store"
	"github.com/Rauden0/bubutracker-api/internal/store/sqlc"
)

func main() {
	if err := run(); err != nil {
		slog.Error("fatal startup error", "error", err)
		os.Exit(1)
	}
}

func run() error {
	cfg, err := config.Load()
	if err != nil {
		return fmt.Errorf("load config: %w", err)
	}

	logger := newLogger(cfg)
	slog.SetDefault(logger)

	if err := checkSchemaCurrent(cfg.DatabaseURL); err != nil {
		return fmt.Errorf("schema check: %w", err)
	}

	ctx, stop := signal.NotifyContext(context.Background(), os.Interrupt, syscall.SIGTERM)
	defer stop()

	pool, err := store.NewPool(ctx, store.PoolConfig{
		DatabaseURL:     cfg.DatabaseURL,
		MaxConns:        cfg.DBMaxConns,
		MinConns:        cfg.DBMinConns,
		MaxConnLifetime: cfg.DBMaxConnLifetime,
		MaxConnIdleTime: cfg.DBMaxConnIdleTime,
	})
	if err != nil {
		return fmt.Errorf("connect to database: %w", err)
	}
	defer pool.Close()

	verifier, err := auth.NewVerifier(ctx, cfg.Auth0Domain, cfg.Auth0Audience)
	if err != nil {
		return fmt.Errorf("init auth0 verifier: %w", err)
	}

	queries := sqlc.New(pool)
	userRepo := store.NewUserRepository(queries)
	locationRepo := store.NewLocationRepository(queries)
	trackingRepo := store.NewTrackingRepository(queries)

	userService := service.NewUserService(userRepo)
	locationService := service.NewLocationService(locationRepo)
	trackingService := service.NewTrackingService(userRepo, trackingRepo)

	router := handler.NewRouter(handler.Deps{
		Logger:         logger,
		AuthMiddleware: verifier.Middleware,
		Health:         handler.NewHealthHandler(pool),
		Users:          userService,
		Locations:      locationService,
		Tracking:       trackingService,
		CORSOrigins:    cfg.CORSAllowedOrigins,
		RequestTimeout: cfg.RequestTimeout,
		MaxBodyBytes:   cfg.MaxBodyBytes,
	})

	srv := &http.Server{
		Addr:              ":" + cfg.HTTPPort,
		Handler:           router,
		ReadHeaderTimeout: 5 * time.Second,
		ReadTimeout:       cfg.RequestTimeout,
		WriteTimeout:      cfg.RequestTimeout + 5*time.Second,
		IdleTimeout:       60 * time.Second,
	}

	serveErr := make(chan error, 1)
	go func() {
		logger.Info("http server listening", "port", cfg.HTTPPort, "env", cfg.Env)
		if err := srv.ListenAndServe(); err != nil && !errors.Is(err, http.ErrServerClosed) {
			serveErr <- err
			return
		}
		serveErr <- nil
	}()

	select {
	case err := <-serveErr:
		if err != nil {
			return fmt.Errorf("http server: %w", err)
		}
	case <-ctx.Done():
		logger.Info("shutdown signal received, draining connections")
		shutdownCtx, cancel := context.WithTimeout(context.Background(), cfg.ShutdownTimeout)
		defer cancel()
		if err := srv.Shutdown(shutdownCtx); err != nil {
			return fmt.Errorf("graceful shutdown: %w", err)
		}
	}

	logger.Info("shutdown complete")
	return nil
}

func newLogger(cfg config.Config) *slog.Logger {
	opts := &slog.HandlerOptions{Level: slog.LevelInfo}
	if cfg.IsDevelopment() {
		return slog.New(slog.NewTextHandler(os.Stdout, opts))
	}
	return slog.New(slog.NewJSONHandler(os.Stdout, opts))
}
