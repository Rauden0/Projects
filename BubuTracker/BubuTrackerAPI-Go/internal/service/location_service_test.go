package service_test

import (
	"context"
	"testing"

	"github.com/google/uuid"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"

	"github.com/Rauden0/bubutracker-api/internal/domain"
	"github.com/Rauden0/bubutracker-api/internal/service"
)

func TestLocationService_UpdateMyLocation_RejectsOutOfRangeCoordinates(t *testing.T) {
	svc := service.NewLocationService(newFakeLocationRepo())
	userID := uuid.New()

	cases := []struct {
		name      string
		lat, long float64
	}{
		{"latitude too high", 91, 0},
		{"latitude too low", -91, 0},
		{"longitude too high", 0, 181},
		{"longitude too low", 0, -181},
	}

	for _, tc := range cases {
		t.Run(tc.name, func(t *testing.T) {
			_, err := svc.UpdateMyLocation(context.Background(), userID, tc.lat, tc.long)
			assert.ErrorIs(t, err, domain.ErrInvalidArgument)
		})
	}
}

func TestLocationService_UpdateMyLocation_AcceptsValidCoordinates(t *testing.T) {
	svc := service.NewLocationService(newFakeLocationRepo())

	loc, err := svc.UpdateMyLocation(context.Background(), uuid.New(), 50.0755, 14.4378)

	require.NoError(t, err)
	assert.Equal(t, 50.0755, loc.Latitude)
	assert.Equal(t, 14.4378, loc.Longitude)
}
