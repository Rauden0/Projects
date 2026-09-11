package service_test

import (
	"context"

	"github.com/google/uuid"

	"github.com/Rauden0/bubutracker-api/internal/domain"
)

// In-memory fakes for the repository ports, used to unit test the service
// layer without a database.

type fakeUserRepo struct {
	byID      map[uuid.UUID]domain.User
	bySubject map[string]uuid.UUID
	byEmail   map[string]uuid.UUID
}

func newFakeUserRepo() *fakeUserRepo {
	return &fakeUserRepo{
		byID:      map[uuid.UUID]domain.User{},
		bySubject: map[string]uuid.UUID{},
		byEmail:   map[string]uuid.UUID{},
	}
}

func (f *fakeUserRepo) seed(u domain.User) {
	f.byID[u.ID] = u
	f.bySubject[u.Auth0SubjectID] = u.ID
	f.byEmail[u.Email] = u.ID
}

func (f *fakeUserRepo) GetByID(_ context.Context, id uuid.UUID) (domain.User, error) {
	u, ok := f.byID[id]
	if !ok {
		return domain.User{}, domain.ErrNotFound
	}
	return u, nil
}

func (f *fakeUserRepo) GetByAuth0SubjectID(_ context.Context, subjectID string) (domain.User, error) {
	id, ok := f.bySubject[subjectID]
	if !ok {
		return domain.User{}, domain.ErrNotFound
	}
	return f.byID[id], nil
}

func (f *fakeUserRepo) GetByEmail(_ context.Context, email string) (domain.User, error) {
	id, ok := f.byEmail[email]
	if !ok {
		return domain.User{}, domain.ErrNotFound
	}
	return f.byID[id], nil
}

func (f *fakeUserRepo) Create(_ context.Context, u domain.User) (domain.User, error) {
	if _, exists := f.byEmail[u.Email]; exists {
		return domain.User{}, domain.ErrAlreadyExists
	}
	f.seed(u)
	return u, nil
}

func (f *fakeUserRepo) Update(_ context.Context, u domain.User) (domain.User, error) {
	if _, ok := f.byID[u.ID]; !ok {
		return domain.User{}, domain.ErrNotFound
	}
	f.seed(u)
	return u, nil
}

type fakeLocationRepo struct {
	byUser map[uuid.UUID]domain.Location
}

func newFakeLocationRepo() *fakeLocationRepo {
	return &fakeLocationRepo{byUser: map[uuid.UUID]domain.Location{}}
}

func (f *fakeLocationRepo) Upsert(_ context.Context, userID uuid.UUID, latitude, longitude float64) (domain.Location, error) {
	loc := domain.Location{UserID: userID, Latitude: latitude, Longitude: longitude}
	f.byUser[userID] = loc
	return loc, nil
}

func (f *fakeLocationRepo) GetTrackedLocations(_ context.Context, _ uuid.UUID) ([]domain.TrackedLocation, error) {
	var result []domain.TrackedLocation
	for _, loc := range f.byUser {
		result = append(result, domain.TrackedLocation{Location: loc})
	}
	return result, nil
}

type trackingKey struct {
	tracker, tracked uuid.UUID
}

type fakeTrackingRepo struct {
	edges map[trackingKey]bool
	users map[uuid.UUID]domain.User
}

func newFakeTrackingRepo() *fakeTrackingRepo {
	return &fakeTrackingRepo{edges: map[trackingKey]bool{}, users: map[uuid.UUID]domain.User{}}
}

func (f *fakeTrackingRepo) Get(_ context.Context, trackerID, trackedUserID uuid.UUID) (bool, error) {
	return f.edges[trackingKey{trackerID, trackedUserID}], nil
}

func (f *fakeTrackingRepo) GetTrackedUsers(_ context.Context, trackerID uuid.UUID) ([]domain.User, error) {
	var result []domain.User
	for k := range f.edges {
		if k.tracker == trackerID {
			result = append(result, f.users[k.tracked])
		}
	}
	return result, nil
}

func (f *fakeTrackingRepo) Add(_ context.Context, trackerID, trackedUserID uuid.UUID) error {
	f.edges[trackingKey{trackerID, trackedUserID}] = true
	return nil
}

func (f *fakeTrackingRepo) Remove(_ context.Context, trackerID, trackedUserID uuid.UUID) error {
	delete(f.edges, trackingKey{trackerID, trackedUserID})
	return nil
}
