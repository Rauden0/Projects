package config_test

import (
	"testing"
	"time"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"

	"github.com/Rauden0/bubutracker-api/internal/config"
)

func clearAll(t *testing.T) {
	t.Helper()
	for _, key := range []string{
		"APP_ENV", "HTTP_PORT", "DATABASE_URL", "AUTH0_DOMAIN", "AUTH0_AUDIENCE",
		"CORS_ALLOWED_ORIGINS", "MAX_REQUEST_BODY_BYTES", "SHUTDOWN_TIMEOUT",
		"REQUEST_TIMEOUT", "DB_MAX_CONNS", "DB_MIN_CONNS",
		"DB_MAX_CONN_LIFETIME", "DB_MAX_CONN_IDLE_TIME",
	} {
		t.Setenv(key, "")
	}
}

func setRequired(t *testing.T) {
	t.Helper()
	t.Setenv("DATABASE_URL", "postgres://localhost/test")
	t.Setenv("AUTH0_DOMAIN", "test.auth0.com")
	t.Setenv("AUTH0_AUDIENCE", "https://api.test")
}

func TestLoad_RejectsMissingRequiredVars(t *testing.T) {
	clearAll(t)

	_, err := config.Load()

	require.Error(t, err)
	assert.Contains(t, err.Error(), "DATABASE_URL")
	assert.Contains(t, err.Error(), "AUTH0_DOMAIN")
	assert.Contains(t, err.Error(), "AUTH0_AUDIENCE")
}

func TestLoad_AppliesDefaultsWhenOptionalVarsUnset(t *testing.T) {
	clearAll(t)
	setRequired(t)

	cfg, err := config.Load()

	require.NoError(t, err)
	assert.Equal(t, "development", cfg.Env)
	assert.True(t, cfg.IsDevelopment())
	assert.Equal(t, "8080", cfg.HTTPPort)
	assert.Equal(t, []string{"*"}, cfg.CORSAllowedOrigins)
	assert.Equal(t, int64(1<<20), cfg.MaxBodyBytes)
	assert.Equal(t, 15*time.Second, cfg.ShutdownTimeout)
	assert.Equal(t, 10*time.Second, cfg.RequestTimeout)
	assert.EqualValues(t, 10, cfg.DBMaxConns)
	assert.EqualValues(t, 2, cfg.DBMinConns)
	assert.Equal(t, 30*time.Minute, cfg.DBMaxConnLifetime)
	assert.Equal(t, 5*time.Minute, cfg.DBMaxConnIdleTime)
}

func TestLoad_OverridesDefaultsFromEnv(t *testing.T) {
	clearAll(t)
	setRequired(t)
	t.Setenv("APP_ENV", "production")
	t.Setenv("HTTP_PORT", "9090")
	t.Setenv("CORS_ALLOWED_ORIGINS", "https://a.example.com, https://b.example.com")
	t.Setenv("MAX_REQUEST_BODY_BYTES", "2048")
	t.Setenv("REQUEST_TIMEOUT", "30")
	t.Setenv("DB_MAX_CONNS", "50")

	cfg, err := config.Load()

	require.NoError(t, err)
	assert.Equal(t, "production", cfg.Env)
	assert.False(t, cfg.IsDevelopment())
	assert.Equal(t, "9090", cfg.HTTPPort)
	assert.Equal(t, []string{"https://a.example.com", "https://b.example.com"}, cfg.CORSAllowedOrigins)
	assert.Equal(t, int64(2048), cfg.MaxBodyBytes)
	assert.Equal(t, 30*time.Second, cfg.RequestTimeout)
	assert.EqualValues(t, 50, cfg.DBMaxConns)
}

func TestLoad_FallsBackToDefaultOnMalformedNumericVar(t *testing.T) {
	clearAll(t)
	setRequired(t)
	t.Setenv("DB_MAX_CONNS", "not-a-number")

	cfg, err := config.Load()

	require.NoError(t, err)
	assert.EqualValues(t, 10, cfg.DBMaxConns) // default, malformed value ignored
}
