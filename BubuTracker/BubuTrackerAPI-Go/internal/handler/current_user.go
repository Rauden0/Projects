package handler

import (
	"context"
	"net/http"

	"github.com/Rauden0/bubutracker-api/internal/auth"
	"github.com/Rauden0/bubutracker-api/internal/domain"
	"github.com/Rauden0/bubutracker-api/internal/httpserver"
)

type currentUserCtxKey struct{}

func CurrentUserMiddleware(users UserService) func(http.Handler) http.Handler {
	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			claims := auth.FromContext(r.Context())

			user, err := users.GetOrCreateBySubject(r.Context(), claims.Subject, claims.Email, claims.EmailVerified, claims.FirstName, claims.LastName)
			if err != nil {
				httpserver.WriteError(w, err)
				return
			}

			ctx := context.WithValue(r.Context(), currentUserCtxKey{}, user)
			next.ServeHTTP(w, r.WithContext(ctx))
		})
	}
}

func currentUserFromContext(ctx context.Context) domain.User {
	user, ok := ctx.Value(currentUserCtxKey{}).(domain.User)
	if !ok {
		panic("handler: currentUserFromContext called outside of CurrentUserMiddleware")
	}
	return user
}

func WithCurrentUser(ctx context.Context, user domain.User) context.Context {
	return context.WithValue(ctx, currentUserCtxKey{}, user)
}

func currentUserRateLimitKey(r *http.Request) string {
	return currentUserFromContext(r.Context()).ID.String()
}
