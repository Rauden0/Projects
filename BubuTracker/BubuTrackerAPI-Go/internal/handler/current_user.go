package handler

import (
	"context"
	"net/http"

	"github.com/Rauden0/bubutracker-api/internal/auth"
	"github.com/Rauden0/bubutracker-api/internal/domain"
	"github.com/Rauden0/bubutracker-api/internal/httpserver"
)

type currentUserCtxKey struct{}

// CurrentUserMiddleware resolves (and lazily provisions) the domain user for
// the authenticated Auth0 subject exactly once per request, and stores it in
// context. It must run after auth.Verifier.Middleware, since it reads the
// claims that middleware attaches.
//
// Centralizing this here replaces what used to be every handler
// independently calling GetOrCreateBySubject: one place resolves "who is the
// current user," and handlers just read the result.
func CurrentUserMiddleware(users UserService) func(http.Handler) http.Handler {
	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			claims := auth.FromContext(r.Context())

			user, err := users.GetOrCreateBySubject(r.Context(), claims.Subject, claims.Email, claims.FirstName, claims.LastName)
			if err != nil {
				httpserver.WriteError(w, err)
				return
			}

			ctx := context.WithValue(r.Context(), currentUserCtxKey{}, user)
			next.ServeHTTP(w, r.WithContext(ctx))
		})
	}
}

// currentUserFromContext returns the user CurrentUserMiddleware resolved. It
// panics if called on a request that middleware did not process, since that
// indicates a routing bug (a handler reachable without the middleware chain
// that's supposed to precede it).
func currentUserFromContext(ctx context.Context) domain.User {
	user, ok := ctx.Value(currentUserCtxKey{}).(domain.User)
	if !ok {
		panic("handler: currentUserFromContext called outside of CurrentUserMiddleware")
	}
	return user
}

// WithCurrentUser returns a copy of ctx carrying user, as CurrentUserMiddleware
// would set it on a real request. It exists so handler tests can exercise a
// handler directly, without a real auth chain in front of it.
func WithCurrentUser(ctx context.Context, user domain.User) context.Context {
	return context.WithValue(ctx, currentUserCtxKey{}, user)
}
