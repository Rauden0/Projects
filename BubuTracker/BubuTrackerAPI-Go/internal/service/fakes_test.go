package service_test

import (
	"context"

	"github.com/google/uuid"

	"github.com/Rauden0/bubutracker-api/internal/domain"
)

type fakeUserRepo struct {
	byID      map[uuid.UUID]domain.User
	bySubject map[string]uuid.UUID
	byEmail   map[string]uuid.UUID
	deleted   map[uuid.UUID]bool
}

func newFakeUserRepo() *fakeUserRepo {
	return &fakeUserRepo{
		byID:      map[uuid.UUID]domain.User{},
		bySubject: map[string]uuid.UUID{},
		byEmail:   map[string]uuid.UUID{},
		deleted:   map[uuid.UUID]bool{},
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

// Mirrors ON CONFLICT (auth0_subject_id) + users.email UNIQUE → ErrEmailConflict.
func (f *fakeUserRepo) UpsertByAuth0Subject(_ context.Context, subjectID, email, firstName, lastName string) (domain.User, error) {
	if id, ok := f.bySubject[subjectID]; ok {
		if ownerID, taken := f.byEmail[email]; taken && ownerID != id {
			return domain.User{}, domain.ErrEmailConflict
		}
		u := f.byID[id]
		u.Email = email
		f.seed(u)
		return u, nil
	}

	if _, taken := f.byEmail[email]; taken {
		return domain.User{}, domain.ErrEmailConflict
	}

	u := domain.User{
		ID:             uuid.New(),
		Auth0SubjectID: subjectID,
		Email:          email,
		FirstName:      firstName,
		LastName:       lastName,
	}
	f.seed(u)
	return u, nil
}

func (f *fakeUserRepo) UpdateProfile(_ context.Context, id uuid.UUID, firstName, lastName, markerColor *string) (domain.User, error) {
	u, ok := f.byID[id]
	if !ok {
		return domain.User{}, domain.ErrNotFound
	}
	if firstName != nil {
		u.FirstName = *firstName
	}
	if lastName != nil {
		u.LastName = *lastName
	}
	if markerColor != nil {
		u.MarkerColor = *markerColor
	}
	f.seed(u)
	return u, nil
}

// Mirrors migration 000005: cascades to every tracking edge involving this user.
func (f *fakeUserRepo) Delete(_ context.Context, id uuid.UUID) error {
	u, ok := f.byID[id]
	if !ok {
		return domain.ErrNotFound
	}
	delete(f.byID, id)
	delete(f.bySubject, u.Auth0SubjectID)
	delete(f.byEmail, u.Email)
	f.deleted[id] = true
	return nil
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
		loc := loc
		result = append(result, domain.TrackedLocation{Location: &loc})
	}
	return result, nil
}

type trackingKey struct {
	tracker, tracked uuid.UUID
}

type fakeTrackingRepo struct {
	edges map[trackingKey]string
	users map[uuid.UUID]domain.User
}

func newFakeTrackingRepo() *fakeTrackingRepo {
	return &fakeTrackingRepo{edges: map[trackingKey]string{}, users: map[uuid.UUID]domain.User{}}
}

func (f *fakeTrackingRepo) Get(_ context.Context, trackerID, trackedUserID uuid.UUID) (bool, error) {
	_, ok := f.edges[trackingKey{trackerID, trackedUserID}]
	return ok, nil
}

func (f *fakeTrackingRepo) GetTrackedUsers(_ context.Context, trackerID uuid.UUID) ([]domain.User, error) {
	var result []domain.User
	for k, status := range f.edges {
		if k.tracker == trackerID && status == "accepted" {
			result = append(result, f.users[k.tracked])
		}
	}
	return result, nil
}

func (f *fakeTrackingRepo) GetIncomingRequests(_ context.Context, trackedUserID uuid.UUID) ([]domain.User, error) {
	var result []domain.User
	for k, status := range f.edges {
		if k.tracked == trackedUserID && status == "pending" {
			result = append(result, f.users[k.tracker])
		}
	}
	return result, nil
}

func (f *fakeTrackingRepo) GetOutgoingRequests(_ context.Context, trackerID uuid.UUID) ([]domain.User, error) {
	var result []domain.User
	for k, status := range f.edges {
		if k.tracker == trackerID && status == "pending" {
			result = append(result, f.users[k.tracked])
		}
	}
	return result, nil
}

func (f *fakeTrackingRepo) GetFollowers(_ context.Context, trackedUserID uuid.UUID) ([]domain.User, error) {
	var result []domain.User
	for k, status := range f.edges {
		if k.tracked == trackedUserID && status == "accepted" {
			result = append(result, f.users[k.tracker])
		}
	}
	return result, nil
}

func (f *fakeTrackingRepo) Add(_ context.Context, trackerID, trackedUserID uuid.UUID) error {
	f.edges[trackingKey{trackerID, trackedUserID}] = "pending"
	return nil
}

func (f *fakeTrackingRepo) Accept(_ context.Context, trackerID, trackedUserID uuid.UUID) error {
	key := trackingKey{trackerID, trackedUserID}
	if f.edges[key] != "pending" {
		return domain.ErrNotFound
	}
	f.edges[key] = "accepted"
	return nil
}

func (f *fakeTrackingRepo) Remove(_ context.Context, trackerID, trackedUserID uuid.UUID) error {
	delete(f.edges, trackingKey{trackerID, trackedUserID})
	return nil
}
