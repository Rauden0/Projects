package service

import (
	"context"
	"fmt"

	"github.com/google/uuid"

	"github.com/Rauden0/bubutracker-api/internal/domain"
)

type LocationService struct {
	locations LocationRepository
}

func NewLocationService(locations LocationRepository) *LocationService {
	return &LocationService{locations: locations}
}

func (s *LocationService) UpdateMyLocation(ctx context.Context, userID uuid.UUID, latitude, longitude float64) (domain.Location, error) {
	if latitude < -90 || latitude > 90 {
		return domain.Location{}, fmt.Errorf("%w: latitude must be between -90 and 90", domain.ErrInvalidArgument)
	}
	if longitude < -180 || longitude > 180 {
		return domain.Location{}, fmt.Errorf("%w: longitude must be between -180 and 180", domain.ErrInvalidArgument)
	}

	return s.locations.Upsert(ctx, userID, latitude, longitude)
}

func (s *LocationService) GetTrackedLocations(ctx context.Context, trackerID uuid.UUID) ([]domain.TrackedLocation, error) {
	return s.locations.GetTrackedLocations(ctx, trackerID)
}
