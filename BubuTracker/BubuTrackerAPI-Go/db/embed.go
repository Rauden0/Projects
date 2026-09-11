// Package db embeds the SQL migration files so the migrate binary (and, if
// ever needed, the API binary) can apply them without relying on a file
// path being present at runtime.
package db

import "embed"

//go:embed migrations/*.sql
var MigrationsFS embed.FS
