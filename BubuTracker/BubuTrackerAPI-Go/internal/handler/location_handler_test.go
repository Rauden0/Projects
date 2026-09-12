package handler_test

import (
	"context"
	"encoding/json"
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
	user := domain.User{ID: uuid.New()}
	h := handler.NewLocationHandler(&fakeLocationService{updateErr: domain.ErrInvalidArgument})

	rec := httptest.NewRecorder()
	h.UpdateMine(rec, requestAsUser(http.MethodPost, "/api/v1/locations/me", `{"latitude":999,"longitude":0}`, user))

	assert.Equal(t, http.StatusBadRequest, rec.Code)
}

func TestLocationHandler_UpdateMine_RejectsMissingCoordinates(t *testing.T) {
	user := domain.User{ID: uuid.New()}
	h := handler.NewLocationHandler(&fakeLocationService{})

	cases := []string{`{}`, `{"latitude":50.1}`, `{"longitude":14.4}`}
	for _, body := range cases {
		rec := httptest.NewRecorder()
		h.UpdateMine(rec, requestAsUser(http.MethodPost, "/api/v1/locations/me", body, user))

		assert.Equalf(t, http.StatusBadRequest, rec.Code, "body %q should be rejected", body)
	}
}

func TestLocationHandler_UpdateMine_Succeeds(t *testing.T) {
	user := domain.User{ID: uuid.New()}
	locations := &fakeLocationService{}
	h := handler.NewLocationHandler(locations)

	rec := httptest.NewRecorder()
	h.UpdateMine(rec, requestAsUser(http.MethodPost, "/api/v1/locations/me", `{"latitude":50.1,"longitude":14.4}`, user))

	require.Equal(t, http.StatusOK, rec.Code)
	assert.Equal(t, 50.1, locations.lastLat)
	assert.Equal(t, 14.4, locations.lastLon)
}

func TestLocationHandler_Tracked_ReturnsList(t *testing.T) {
	user := domain.User{ID: uuid.New()}
	loc := domain.Location{Latitude: 1, Longitude: 2}
	locations := &fakeLocationService{
		trackedLocations: []domain.TrackedLocation{
			{User: domain.UserSummary{Email: "friend@example.com"}, Location: &loc},
		},
	}
	h := handler.NewLocationHandler(locations)

	rec := httptest.NewRecorder()
	h.Tracked(rec, requestAsUser(http.MethodGet, "/api/v1/locations/tracked", "", user))

	require.Equal(t, http.StatusOK, rec.Code)
	assert.Contains(t, rec.Body.String(), "friend@example.com")
}

func TestLocationHandler_Tracked_NullsLocationForUserWithNoReportedLocation(t *testing.T) {
	user := domain.User{ID: uuid.New()}
	locations := &fakeLocationService{
		trackedLocations: []domain.TrackedLocation{
			{User: domain.UserSummary{Email: "ghost@example.com"}, Location: nil},
		},
	}
	h := handler.NewLocationHandler(locations)

	rec := httptest.NewRecorder()
	h.Tracked(rec, requestAsUser(http.MethodGet, "/api/v1/locations/tracked", "", user))

	require.Equal(t, http.StatusOK, rec.Code)

	var got []map[string]any
	require.NoError(t, json.NewDecoder(rec.Body).Decode(&got))
	require.Len(t, got, 1)
	assert.Equal(t, "ghost@example.com", got[0]["email"])
	assert.Nil(t, got[0]["latitude"])
	assert.Nil(t, got[0]["longitude"])
	assert.Nil(t, got[0]["updatedAt"])
}
