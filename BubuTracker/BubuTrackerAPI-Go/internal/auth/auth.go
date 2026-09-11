// Package auth validates Auth0-issued access tokens (RS256 JWTs) using the
// tenant's published OIDC discovery document and JWKS, and exposes the
// authenticated subject's claims to downstream handlers via context.
package auth

import (
	"context"
	"fmt"
	"net/http"
	"strings"

	"github.com/coreos/go-oidc/v3/oidc"
)

type ctxKey int

const claimsCtxKey ctxKey = iota

// Claims is the subset of the Auth0 access token we care about.
type Claims struct {
	Subject   string `json:"sub"`
	Email     string `json:"email"`
	FirstName string `json:"given_name"`
	LastName  string `json:"family_name"`
}

// Verifier validates bearer tokens against a single Auth0 tenant/audience.
type Verifier struct {
	idTokenVerifier *oidc.IDTokenVerifier
}

// NewVerifier fetches the OIDC discovery document for the given Auth0 domain
// and builds a verifier scoped to the given API audience. It fails fast if
// the tenant's discovery document can't be reached, so a misconfiguration is
// caught at startup rather than on the first request.
func NewVerifier(ctx context.Context, auth0Domain, audience string) (*Verifier, error) {
	issuer := fmt.Sprintf("https://%s/", auth0Domain)

	provider, err := oidc.NewProvider(ctx, issuer)
	if err != nil {
		return nil, fmt.Errorf("discover auth0 oidc provider at %s: %w", issuer, err)
	}

	verifier := provider.Verifier(&oidc.Config{
		ClientID:          audience,
		SkipClientIDCheck: audience == "",
	})

	return &Verifier{idTokenVerifier: verifier}, nil
}

// Middleware authenticates the request's bearer token and injects its claims
// into the request context, rejecting the request with 401 otherwise.
func (v *Verifier) Middleware(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		token := bearerToken(r)
		if token == "" {
			http.Error(w, `{"error":"missing bearer token"}`, http.StatusUnauthorized)
			return
		}

		idToken, err := v.idTokenVerifier.Verify(r.Context(), token)
		if err != nil {
			http.Error(w, `{"error":"invalid or expired token"}`, http.StatusUnauthorized)
			return
		}

		var claims Claims
		if err := idToken.Claims(&claims); err != nil {
			http.Error(w, `{"error":"malformed token claims"}`, http.StatusUnauthorized)
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

// FromContext returns the claims attached by Middleware. It panics if called
// on a request that Middleware did not process, since that indicates a
// routing bug (an unauthenticated route reaching an authenticated handler).
func FromContext(ctx context.Context) Claims {
	claims, ok := ctx.Value(claimsCtxKey).(Claims)
	if !ok {
		panic("auth: FromContext called outside of an authenticated request")
	}
	return claims
}

// NewContext returns a copy of ctx carrying claims, as Middleware would set
// it on a real request. It exists so handler tests can simulate an
// authenticated request without standing up a real Auth0 verifier.
func NewContext(ctx context.Context, claims Claims) context.Context {
	return context.WithValue(ctx, claimsCtxKey, claims)
}
