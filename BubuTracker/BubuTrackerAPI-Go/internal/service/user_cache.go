package service

import (
	"sync"
	"time"

	"github.com/Rauden0/bubutracker-api/internal/domain"
)

// userCache is an in-process, per-replica cache of the resolved domain.User
// for an Auth0 subject, keyed by subject ID. It exists because
// CurrentUserMiddleware calls GetOrCreateBySubject on every authenticated
// request - without this, a location POST and a tracked-locations poll from
// the same user each cost a SELECT (and sometimes an upsert) on the hot path.
//
// Per-replica only: fine at today's single-instance scale (same caveat as
// the in-memory rate limiter). If the API ever runs more than one replica, a
// PATCH/DELETE landing on replica A won't invalidate replica B's copy - would
// need a shared store (Redis) or a short-enough TTL to make that acceptable.
type userCache struct {
	mu      sync.Mutex
	entries map[string]userCacheEntry
	ttl     time.Duration
}

type userCacheEntry struct {
	user      domain.User
	expiresAt time.Time
}

func newUserCache(ttl time.Duration) *userCache {
	return &userCache{entries: make(map[string]userCacheEntry), ttl: ttl}
}

func (c *userCache) get(subjectID string, now time.Time) (domain.User, bool) {
	c.mu.Lock()
	defer c.mu.Unlock()
	entry, ok := c.entries[subjectID]
	if !ok || now.After(entry.expiresAt) {
		return domain.User{}, false
	}
	return entry.user, true
}

func (c *userCache) set(subjectID string, user domain.User, now time.Time) {
	c.mu.Lock()
	defer c.mu.Unlock()
	c.entries[subjectID] = userCacheEntry{user: user, expiresAt: now.Add(c.ttl)}
}

func (c *userCache) invalidate(subjectID string) {
	c.mu.Lock()
	defer c.mu.Unlock()
	delete(c.entries, subjectID)
}
