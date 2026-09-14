package config

import (
	"fmt"
	"os"
	"strconv"
	"strings"
	"time"
)

type Config struct {
	Env         string // "development" or "production"
	HTTPPort    string
	DatabaseURL string

	Auth0Domain   string
	Auth0Audience string

	CORSAllowedOrigins []string
	MaxBodyBytes       int64

	ShutdownTimeout time.Duration
	RequestTimeout  time.Duration

	DBMaxConns        int32
	DBMinConns        int32
	DBMaxConnLifetime time.Duration
	DBMaxConnIdleTime time.Duration
}

func Load() (Config, error) {
	cfg := Config{
		Env:             getEnv("APP_ENV", "development"),
		HTTPPort:        getEnv("HTTP_PORT", "8080"),
		DatabaseURL:     os.Getenv("DATABASE_URL"),
		Auth0Domain:     os.Getenv("AUTH0_DOMAIN"),
		Auth0Audience:   os.Getenv("AUTH0_AUDIENCE"),
		MaxBodyBytes:    getInt64("MAX_REQUEST_BODY_BYTES", 1<<20), // 1 MiB
		ShutdownTimeout: getDuration("SHUTDOWN_TIMEOUT", 15*time.Second),
		RequestTimeout:  getDuration("REQUEST_TIMEOUT", 10*time.Second),

		DBMaxConns:        int32(getInt64("DB_MAX_CONNS", 10)),
		DBMinConns:        int32(getInt64("DB_MIN_CONNS", 2)),
		DBMaxConnLifetime: getDuration("DB_MAX_CONN_LIFETIME", 30*time.Minute),
		DBMaxConnIdleTime: getDuration("DB_MAX_CONN_IDLE_TIME", 5*time.Minute),
	}

	origins := getEnv("CORS_ALLOWED_ORIGINS", "*")
	for _, o := range strings.Split(origins, ",") {
		if trimmed := strings.TrimSpace(o); trimmed != "" {
			cfg.CORSAllowedOrigins = append(cfg.CORSAllowedOrigins, trimmed)
		}
	}

	var missing []string
	if cfg.DatabaseURL == "" {
		missing = append(missing, "DATABASE_URL")
	}
	if cfg.Auth0Domain == "" {
		missing = append(missing, "AUTH0_DOMAIN")
	}
	if cfg.Auth0Audience == "" {
		missing = append(missing, "AUTH0_AUDIENCE")
	}
	if len(missing) > 0 {
		return Config{}, fmt.Errorf("missing required environment variables: %s", strings.Join(missing, ", "))
	}

	return cfg, nil
}

func (c Config) IsDevelopment() bool {
	return c.Env == "development"
}

func getEnv(key, fallback string) string {
	if v, ok := os.LookupEnv(key); ok && v != "" {
		return v
	}
	return fallback
}

func getDuration(key string, fallback time.Duration) time.Duration {
	v, ok := os.LookupEnv(key)
	if !ok || v == "" {
		return fallback
	}
	seconds, err := strconv.Atoi(v)
	if err != nil {
		return fallback
	}
	return time.Duration(seconds) * time.Second
}

func getInt64(key string, fallback int64) int64 {
	v, ok := os.LookupEnv(key)
	if !ok || v == "" {
		return fallback
	}
	n, err := strconv.ParseInt(v, 10, 64)
	if err != nil {
		return fallback
	}
	return n
}
