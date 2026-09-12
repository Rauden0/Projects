package main

import (
	"testing"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
)

// If this fails after adding a migration, bump the expected version — it's
// meant to catch latestMigrationVersion() silently returning the wrong
// number (e.g. an off-by-one or a parsing regression), not to move on its
// own.
const expectedLatestMigrationVersion = 2

func TestLatestMigrationVersion(t *testing.T) {
	got, err := latestMigrationVersion()

	require.NoError(t, err)
	assert.EqualValues(t, expectedLatestMigrationVersion, got)
}
