package main

import (
	"testing"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
)

// Bump when adding a migration; guards latestMigrationVersion() parsing regressions.
const expectedLatestMigrationVersion = 5

func TestLatestMigrationVersion(t *testing.T) {
	got, err := latestMigrationVersion()

	require.NoError(t, err)
	assert.EqualValues(t, expectedLatestMigrationVersion, got)
}
