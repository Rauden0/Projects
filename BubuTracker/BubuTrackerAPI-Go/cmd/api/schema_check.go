package main

import (
	"errors"
	"fmt"
	"log/slog"
	"strconv"
	"strings"

	"github.com/golang-migrate/migrate/v4"
	_ "github.com/golang-migrate/migrate/v4/database/postgres"
	"github.com/golang-migrate/migrate/v4/source/iofs"

	dbmigrations "github.com/Rauden0/bubutracker-api/db"
)

// Fail-fast if cmd/migrate was skipped (avoids opaque 500s on missing schema).
func checkSchemaCurrent(databaseURL string) error {
	latest, err := latestMigrationVersion()
	if err != nil {
		return fmt.Errorf("determine latest embedded migration: %w", err)
	}

	source, err := iofs.New(dbmigrations.MigrationsFS, "migrations")
	if err != nil {
		return fmt.Errorf("load embedded migrations: %w", err)
	}

	m, err := migrate.NewWithSourceInstance("iofs", source, databaseURL)
	if err != nil {
		return fmt.Errorf("init migration checker: %w", err)
	}
	defer func() {
		if srcErr, dbErr := m.Close(); srcErr != nil || dbErr != nil {
			slog.Warn("migration checker cleanup", "source_error", srcErr, "db_error", dbErr)
		}
	}()

	current, dirty, err := m.Version()
	if err != nil {
		if errors.Is(err, migrate.ErrNilVersion) {
			return fmt.Errorf("no migrations have been applied yet (binary expects version %d); run cmd/migrate before starting the API", latest)
		}
		return fmt.Errorf("read applied schema version: %w", err)
	}
	if dirty {
		return fmt.Errorf("database schema is dirty at version %d; resolve manually before starting", current)
	}
	if uint64(current) < latest {
		return fmt.Errorf("database schema is at version %d but this binary expects %d; run cmd/migrate before starting", current, latest)
	}

	return nil
}

func latestMigrationVersion() (uint64, error) {
	entries, err := dbmigrations.MigrationsFS.ReadDir("migrations")
	if err != nil {
		return 0, err
	}

	var latest uint64
	for _, e := range entries {
		name := e.Name()
		underscore := strings.IndexByte(name, '_')
		if underscore <= 0 {
			continue
		}
		version, err := strconv.ParseUint(name[:underscore], 10, 64)
		if err != nil {
			continue
		}
		if version > latest {
			latest = version
		}
	}
	if latest == 0 {
		return 0, errors.New("no migration files found in embedded filesystem")
	}
	return latest, nil
}
