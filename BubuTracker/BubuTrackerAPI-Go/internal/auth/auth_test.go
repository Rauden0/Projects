package auth_test

import (
	"crypto/rand"
	"crypto/rsa"
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"strings"
	"testing"
	"time"

	josejwt "github.com/go-jose/go-jose/v4/jwt"

	"github.com/coreos/go-oidc/v3/oidc"
	"github.com/go-jose/go-jose/v4"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"

	"github.com/Rauden0/bubutracker-api/internal/auth"
)

const testAudience = "https://api.test"

type fakeOIDCProvider struct {
	server     *httptest.Server
	signingKey *rsa.PrivateKey
	keyID      string
}

func newFakeOIDCProvider(t *testing.T) *fakeOIDCProvider {
	t.Helper()

	key, err := rsa.GenerateKey(rand.Reader, 2048)
	require.NoError(t, err)

	p := &fakeOIDCProvider{signingKey: key, keyID: "test-key"}

	mux := http.NewServeMux()
	mux.HandleFunc("/.well-known/openid-configuration", p.serveDiscovery)
	mux.HandleFunc("/.well-known/jwks.json", p.serveJWKS)
	p.server = httptest.NewTLSServer(mux)
	t.Cleanup(p.server.Close)

	return p
}

func (p *fakeOIDCProvider) serveDiscovery(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "application/json")
	_ = json.NewEncoder(w).Encode(map[string]any{
		"issuer":                                p.issuer(),
		"authorization_endpoint":                p.issuer() + "authorize",
		"token_endpoint":                        p.issuer() + "token",
		"jwks_uri":                              p.issuer() + ".well-known/jwks.json",
		"id_token_signing_alg_values_supported": []string{"RS256"},
	})
}

func (p *fakeOIDCProvider) serveJWKS(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "application/json")
	_ = json.NewEncoder(w).Encode(jose.JSONWebKeySet{
		Keys: []jose.JSONWebKey{
			{
				Key:       &p.signingKey.PublicKey,
				KeyID:     p.keyID,
				Algorithm: string(jose.RS256),
				Use:       "sig",
			},
		},
	})
}

// Domain:port with trailing slash — must match discovery document issuer exactly.
func (p *fakeOIDCProvider) issuer() string {
	return p.server.URL + "/"
}

func (p *fakeOIDCProvider) domain() string {
	return strings.TrimPrefix(p.server.URL, "https://")
}

func (p *fakeOIDCProvider) httpClient() *http.Client {
	return p.server.Client()
}

type tokenOpt func(*josejwt.Claims, map[string]any)

func withAudience(aud string) tokenOpt {
	return func(c *josejwt.Claims, _ map[string]any) { c.Audience = josejwt.Audience{aud} }
}

func withExpiry(t time.Time) tokenOpt {
	return func(c *josejwt.Claims, _ map[string]any) { c.Expiry = josejwt.NewNumericDate(t) }
}

func withEmailVerified(verified bool) tokenOpt {
	return func(_ *josejwt.Claims, extra map[string]any) {
		extra["https://bubutracker.app/email_verified"] = verified
	}
}

func (p *fakeOIDCProvider) issueToken(t *testing.T, signingKey *rsa.PrivateKey, subject string, opts ...tokenOpt) string {
	t.Helper()

	claims := josejwt.Claims{
		Issuer:   p.issuer(),
		Subject:  subject,
		Audience: josejwt.Audience{testAudience},
		Expiry:   josejwt.NewNumericDate(time.Now().Add(time.Hour)),
		IssuedAt: josejwt.NewNumericDate(time.Now()),
	}
	// Namespaced Auth0 Action custom claims (not plain OIDC profile claim names).
	extra := map[string]any{
		"https://bubutracker.app/email":       "alice@example.com",
		"https://bubutracker.app/given_name":  "Alice",
		"https://bubutracker.app/family_name": "Smith",
	}
	for _, opt := range opts {
		opt(&claims, extra)
	}

	signer, err := jose.NewSigner(
		jose.SigningKey{Algorithm: jose.RS256, Key: signingKey},
		(&jose.SignerOptions{}).WithType("JWT").WithHeader("kid", p.keyID),
	)
	require.NoError(t, err)

	token, err := josejwt.Signed(signer).Claims(claims).Claims(extra).Serialize()
	require.NoError(t, err)
	return token
}

func newVerifier(t *testing.T, p *fakeOIDCProvider) *auth.Verifier {
	t.Helper()
	ctx := oidc.ClientContext(t.Context(), p.httpClient())
	verifier, err := auth.NewVerifier(ctx, p.domain(), testAudience)
	require.NoError(t, err)
	return verifier
}

func recordingHandler(t *testing.T) (http.Handler, *bool, *auth.Claims) {
	t.Helper()
	called := false
	var seen auth.Claims
	h := http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		called = true
		seen = auth.FromContext(r.Context())
		w.WriteHeader(http.StatusOK)
	})
	return h, &called, &seen
}

func TestVerifierMiddleware_AcceptsValidToken(t *testing.T) {
	provider := newFakeOIDCProvider(t)
	verifier := newVerifier(t, provider)
	next, called, seenClaims := recordingHandler(t)

	token := provider.issueToken(t, provider.signingKey, "auth0|abc123")
	req := httptest.NewRequest(http.MethodGet, "/api/v1/users/me", nil)
	req.Header.Set("Authorization", "Bearer "+token)
	rec := httptest.NewRecorder()

	verifier.Middleware(next).ServeHTTP(rec, req)

	require.True(t, *called)
	assert.Equal(t, http.StatusOK, rec.Code)
	assert.Equal(t, "auth0|abc123", seenClaims.Subject)
	assert.Equal(t, "alice@example.com", seenClaims.Email)
	assert.Equal(t, "Alice", seenClaims.FirstName)
	assert.Equal(t, "Smith", seenClaims.LastName)
	assert.Nil(t, seenClaims.EmailVerified, "must be nil, not false, when the Action doesn't set this claim")
}

func TestVerifierMiddleware_ParsesEmailVerifiedClaimWhenPresent(t *testing.T) {
	provider := newFakeOIDCProvider(t)
	verifier := newVerifier(t, provider)
	next, _, seenClaims := recordingHandler(t)

	token := provider.issueToken(t, provider.signingKey, "auth0|abc123", withEmailVerified(false))
	req := httptest.NewRequest(http.MethodGet, "/api/v1/users/me", nil)
	req.Header.Set("Authorization", "Bearer "+token)
	rec := httptest.NewRecorder()

	verifier.Middleware(next).ServeHTTP(rec, req)

	require.NotNil(t, seenClaims.EmailVerified)
	assert.False(t, *seenClaims.EmailVerified)
}

func TestVerifierMiddleware_RejectsMissingToken(t *testing.T) {
	provider := newFakeOIDCProvider(t)
	verifier := newVerifier(t, provider)
	next, called, _ := recordingHandler(t)

	req := httptest.NewRequest(http.MethodGet, "/api/v1/users/me", nil)
	rec := httptest.NewRecorder()

	verifier.Middleware(next).ServeHTTP(rec, req)

	assert.False(t, *called)
	assert.Equal(t, http.StatusUnauthorized, rec.Code)
}

func TestVerifierMiddleware_RejectsExpiredToken(t *testing.T) {
	provider := newFakeOIDCProvider(t)
	verifier := newVerifier(t, provider)
	next, called, _ := recordingHandler(t)

	token := provider.issueToken(t, provider.signingKey, "auth0|abc123", withExpiry(time.Now().Add(-time.Hour)))
	req := httptest.NewRequest(http.MethodGet, "/api/v1/users/me", nil)
	req.Header.Set("Authorization", "Bearer "+token)
	rec := httptest.NewRecorder()

	verifier.Middleware(next).ServeHTTP(rec, req)

	assert.False(t, *called)
	assert.Equal(t, http.StatusUnauthorized, rec.Code)
}

func TestVerifierMiddleware_RejectsWrongAudience(t *testing.T) {
	provider := newFakeOIDCProvider(t)
	verifier := newVerifier(t, provider)
	next, called, _ := recordingHandler(t)

	token := provider.issueToken(t, provider.signingKey, "auth0|abc123", withAudience("https://someone-else.example.com"))
	req := httptest.NewRequest(http.MethodGet, "/api/v1/users/me", nil)
	req.Header.Set("Authorization", "Bearer "+token)
	rec := httptest.NewRecorder()

	verifier.Middleware(next).ServeHTTP(rec, req)

	assert.False(t, *called)
	assert.Equal(t, http.StatusUnauthorized, rec.Code)
}

func TestVerifierMiddleware_RejectsTokenSignedByUnknownKey(t *testing.T) {
	provider := newFakeOIDCProvider(t)
	verifier := newVerifier(t, provider)
	next, called, _ := recordingHandler(t)

	imposterKey, err := rsa.GenerateKey(rand.Reader, 2048)
	require.NoError(t, err)
	token := provider.issueToken(t, imposterKey, "auth0|abc123")

	req := httptest.NewRequest(http.MethodGet, "/api/v1/users/me", nil)
	req.Header.Set("Authorization", "Bearer "+token)
	rec := httptest.NewRecorder()

	verifier.Middleware(next).ServeHTTP(rec, req)

	assert.False(t, *called)
	assert.Equal(t, http.StatusUnauthorized, rec.Code)
}

func TestVerifierMiddleware_RejectsMalformedBearerHeader(t *testing.T) {
	provider := newFakeOIDCProvider(t)
	verifier := newVerifier(t, provider)
	next, called, _ := recordingHandler(t)

	req := httptest.NewRequest(http.MethodGet, "/api/v1/users/me", nil)
	req.Header.Set("Authorization", "not-a-bearer-token")
	rec := httptest.NewRecorder()

	verifier.Middleware(next).ServeHTTP(rec, req)

	assert.False(t, *called)
	assert.Equal(t, http.StatusUnauthorized, rec.Code)
}
