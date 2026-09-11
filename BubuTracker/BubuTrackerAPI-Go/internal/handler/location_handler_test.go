package handler_test

import (
	"context"
	"net/http"
	"net/http/httptest"
	"testing"

	"github.com/google/uuid"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"

	"github.com/Rauden0/bubutracker-api/internal/domain"
	"github.com/Rauden0/bubutracker-api/internal/handler"
)

type fakeLocationService struct {
	trackedLocations []domain.TrackedLocation
	updateErr        error
	lastLat, lastLon float64
}

func (f *fakeLocationService) UpdateMyLocation(_ context.Context, _ uuid.UUID, lat, lon float64) (domain.Location, error) {
	f.lastLat, f.lastLon = lat, lon
	if f.updateErr != nil {
		return domain.Location{}, f.updateErr
	}
	return domain.Location{Latitude: lat, Longitude: lon}, nil
}

func (f *fakeLocationService) GetTrackedLocations(_ context.Context, _ uuid.UUID) ([]domain.TrackedLocation, error) {
	return f.trackedLocations, nil
}

func TestLocationHandler_UpdateMine_RejectsInvalidCoordinates(t *testing.T) {
	users := &fakeUserService{user: domain.User{ID: uuid.New()}}
	locations := &fakeLocationService{updateErr: domain.ErrInvalidArgument}
	h := handler.NewLocationHandler(users, locations)

	rec := httptest.NewRecorder()
	h.UpdateMine(rec, authedRequest(http.MethodPost, "/api/v1/locations/me", `{"latitude":999,"longitude":0}`))

	assert.Equal(t, http.StatusBadRequest, rec.Code)
}

func TestLocationHandler_UpdateMine_Succeeds(t *testing.T) {
	users := &fakeUserService{user: domain.User{ID: uuid.New()}}
	locations := &fakeLocationService{}
	h := handler.NewLocationHandler(users, locations)

	rec := httptest.NewRecorder()
	h.UpdateMine(rec, authedRequest(http.MethodPost, "/api/v1/locations/me", `{"latitude":50.1,"longitude":14.4}`))

	require.Equal(t, http.StatusOK, rec.Code)
	assert.Equal(t, 50.1, locations.lastLat)
	assert.Equal(t, 14.4, locations.lastLon)
}

func TestLocationHandler_Tracked_ReturnsList(t *testing.T) {
	users := &fakeUserService{user: domain.User{ID: uuid.New()}}
	locations := &fakeLocationService{
		trackedLocations: []domain.TrackedLocation{
			{User: domain.User{Email: "friend@example.com"}, Location: domain.Location{Latitude: 1, Longitude: 2}},
		},
	}
	h := handler.NewLocationHandler(users, locations)

	rec := httptest.NewRecorder()
	h.Tracked(rec, authedRequest(http.MethodGet, "/api/v1/locations/tracked", ""))

	require.Equal(t, http.StatusOK, rec.Code)
	assert.Contains(t, rec.Body.String(), "friend@example.com")
}
