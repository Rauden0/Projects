package main

import (
	"errors"
	"flag"
	"log"
	"os"

	"github.com/golang-migrate/migrate/v4"
	_ "github.com/golang-migrate/migrate/v4/database/postgres"
	"github.com/golang-migrate/migrate/v4/source/iofs"

	dbmigrations "github.com/Rauden0/bubutracker-api/db"
)

func main() {
	direction := flag.String("direction", "up", `migration direction: "up" or "down"`)
	steps := flag.Int("steps", 0, "number of steps to migrate (0 = all)")
	flag.Parse()

	databaseURL := os.Getenv("DATABASE_URL")
	if databaseURL == "" {
		log.Fatal("DATABASE_URL environment variable is required")
	}

	source, err := iofs.New(dbmigrations.MigrationsFS, "migrations")
	if err != nil {
		log.Fatalf("load embedded migrations: %v", err)
	}

	m, err := migrate.NewWithSourceInstance("iofs", source, databaseURL)
	if err != nil {
		log.Fatalf("init migrator: %v", err)
	}

	switch *direction {
	case "up":
		if *steps == 0 {
			err = m.Up()
		} else {
			err = m.Steps(*steps)
		}
	case "down":
		if *steps == 0 {
			err = m.Down()
		} else {
			err = m.Steps(-*steps)
		}
	default:
		log.Fatalf("unknown -direction %q: must be \"up\" or \"down\"", *direction)
	}

	if err != nil && !errors.Is(err, migrate.ErrNoChange) {
		log.Fatalf("migration failed: %v", err)
	}

	log.Println("migrations applied successfully")
}
