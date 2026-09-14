package auth

import (
	"context"
	"fmt"
	"net/http"
	"strings"
	"time"

	"github.com/coreos/go-oidc/v3/oidc"

	"github.com/Rauden0/bubutracker-api/internal/domain"
	"github.com/Rauden0/bubutracker-api/internal/httpserver"
)

const (
	discoveryAttempts = 3
	discoveryTimeout  = 10 * time.Second
	discoveryBackoff  = time.Second
)

type ctxKey int

const claimsCtxKey ctxKey = iota

// Access tokens for a custom API audience omit plain OIDC profile claims;
// an Auth0 Action must set these namespaced custom claims on the access token.
//
// EmailVerified is a *bool, not bool: the Action may not set this claim yet,
// and a missing claim (nil) must be treated differently from an explicit
// false — see UserService.GetOrCreateBySubject.
type Claims struct {
	Subject       string `json:"sub"`
	Email         string `json:"https://bubutracker.app/email"`
	EmailVerified *bool  `json:"https://bubutracker.app/email_verified"`
	FirstName     string `json:"https://bubutracker.app/given_name"`
	LastName      string `json:"https://bubutracker.app/family_name"`
}

type Verifier struct {
	idTokenVerifier *oidc.IDTokenVerifier
}

func NewVerifier(ctx context.Context, auth0Domain, audience string) (*Verifier, error) {
	issuer := fmt.Sprintf("https://%s/", auth0Domain)

	provider, err := discoverProvider(ctx, issuer)
	if err != nil {
		return nil, fmt.Errorf("discover auth0 oidc provider at %s after %d attempts: %w", issuer, discoveryAttempts, err)
	}

	verifier := provider.Verifier(&oidc.Config{
		ClientID:          audience,
		SkipClientIDCheck: audience == "",
	})

	return &Verifier{idTokenVerifier: verifier}, nil
}

func discoverProvider(ctx context.Context, issuer string) (*oidc.Provider, error) {
	var lastErr error
	for attempt := 1; attempt <= discoveryAttempts; attempt++ {
		attemptCtx, cancel := context.WithTimeout(ctx, discoveryTimeout)
		provider, err := oidc.NewProvider(attemptCtx, issuer)
		cancel()
		if err == nil {
			return provider, nil
		}
		lastErr = err

		if attempt < discoveryAttempts {
			select {
			case <-time.After(discoveryBackoff * time.Duration(attempt)):
			case <-ctx.Done():
				return nil, ctx.Err()
			}
		}
	}
	return nil, lastErr
}

func (v *Verifier) Middleware(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		token := bearerToken(r)
		if token == "" {
			httpserver.WriteError(w, fmt.Errorf("%w: missing bearer token", domain.ErrUnauthenticated))
			return
		}

		idToken, err := v.idTokenVerifier.Verify(r.Context(), token)
		if err != nil {
			httpserver.WriteError(w, fmt.Errorf("%w: invalid or expired token", domain.ErrUnauthenticated))
			return
		}

		var claims Claims
		if err := idToken.Claims(&claims); err != nil {
			httpserver.WriteError(w, fmt.Errorf("%w: malformed token claims", domain.ErrUnauthenticated))
			return
		}
		claims.Subject = idToken.Subject

		ctx := context.WithValue(r.Context(), claimsCtxKey, claims)
		next.ServeHTTP(w, r.WithContext(ctx))
	})
}

func bearerToken(r *http.Request) string {
	header := r.Header.Get("Authorization")
	const prefix = "Bearer "
	if len(header) <= len(prefix) || !strings.EqualFold(header[:len(prefix)], prefix) {
		return ""
	}
	return header[len(prefix):]
}

func FromContext(ctx context.Context) Claims {
	claims, ok := ctx.Value(claimsCtxKey).(Claims)
	if !ok {
		panic("auth: FromContext called outside of an authenticated request")
	}
	return claims
}

func NewContext(ctx context.Context, claims Claims) context.Context {
	return context.WithValue(ctx, claimsCtxKey, claims)
}
