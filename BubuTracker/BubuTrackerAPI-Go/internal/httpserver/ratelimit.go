package httpserver

import (
	"net/http"
	"sync"
	"time"

	"github.com/Rauden0/bubutracker-api/internal/domain"
)

type KeyFunc func(r *http.Request) string

// sweepEvery caps how often we walk the whole bucket map looking for expired
// entries, so a busy server isn't paying for a full-map scan on every request.
const sweepEvery = 500

func RateLimit(limit int, window time.Duration, key KeyFunc) func(http.Handler) http.Handler {
	type bucket struct {
		count      int
		windowEnds time.Time
	}

	var mu sync.Mutex
	buckets := make(map[string]*bucket)
	requestsSinceSweep := 0

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

			// Every key that has ever hit this route otherwise stays in the map
			// forever; periodically drop entries whose window has long since
			// closed so long-running processes don't leak memory.
			requestsSinceSweep++
			if requestsSinceSweep >= sweepEvery {
				requestsSinceSweep = 0
				for k, b := range buckets {
					if now.After(b.windowEnds) {
						delete(buckets, k)
					}
				}
			}
			mu.Unlock()

			if exceeded {
				WriteError(w, domain.ErrRateLimited)
				return
			}
			next.ServeHTTP(w, r)
		})
	}
}
