package httpserver

import (
	"net/http"
	"sync"
	"time"

	"github.com/Rauden0/bubutracker-api/internal/domain"
)

// KeyFunc extracts the identity a rate limit is scoped to from a request —
// e.g. the authenticated user's ID, so the limit follows the account rather
// than an IP that's easy to rotate.
type KeyFunc func(r *http.Request) string

// RateLimit caps each key to limit requests per window, using a simple
// fixed-window counter in memory. That's enough for a single-instance
// deployment; a multi-instance one would need a shared store (Redis) instead
// so instances don't each enforce their own independent limit.
//
// Bucket entries for keys that are only ever seen once are never evicted,
// so memory grows with the number of distinct keys seen — acceptable here
// since keys are user IDs from a small, known user base, not raw client IPs.
func RateLimit(limit int, window time.Duration, key KeyFunc) func(http.Handler) http.Handler {
	type bucket struct {
		count      int
		windowEnds time.Time
	}

	var mu sync.Mutex
	buckets := make(map[string]*bucket)

	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			k := key(r)
			now := time.Now()

			mu.Lock()
			b, ok := buckets[k]
			if !ok || now.After(b.windowEnds) {
				b = &bucket{windowEnds: now.Add(window)}
				buckets[k] = b
			}
			b.count++
			exceeded := b.count > limit
			mu.Unlock()

			if exceeded {
				WriteError(w, domain.ErrRateLimited)
				return
			}
			next.ServeHTTP(w, r)
		})
	}
}
