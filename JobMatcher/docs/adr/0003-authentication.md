# ADR 0003: Callers, CV ownership and the MCP token

- **Status:** Accepted
- **Date:** 2026-11-27
- **Scope:** `src/jobmatcher/api/`, `cv/service.py`, `mcp_server.py`, migration 0005

## Context

The API had no authentication. `POST /cv` returned a `cv_id` that the UI kept in
`localStorage`, and that id was the only thing standing between a caller and someone's CV:
whoever knew it could chat with the CV (`compare_with_cv` reads it) and delete it. The id is a
random UUID, so it is hard to guess, but it is a bearer token that ends up in browser storage,
history payloads, logs and bug reports. `jobmatcher-mcp --http` had no authentication either
and only the default `127.0.0.1` bind kept it private.

The demo is meant to stay usable without accounts, and a deployment behind a company
identity provider should not need code changes.

## Decision

**Every CV has an owner, the principal that uploaded it, and only that principal can use or
delete it.** `cv.owner_id` (text, nullable, indexed; migration 0005) is set on `POST /cv`.
`profile_summary` and `delete_cv` go through `cv.service.owned_cv`, which treats a CV owned
by someone else exactly like a missing one: **404, not 403**, so ids reveal nothing. Rows from
before 0005 have `owner_id NULL` and belong to nobody; they cannot be used any more and the
retention service deletes them within `CV_RETENTION_DAYS`.

The principal comes from `AUTH_MODE`, behind one dependency (`api/deps.PrincipalDep`) and an
`Authenticator` protocol (`api/auth.py`):

| | `session` (default) | `oidc` |
|---|---|---|
| Credential | `jm_session` cookie: random 192-bit id + HMAC-SHA256 with `SESSION_SECRET` | `Authorization: Bearer <JWT>` |
| Issued by | the API, on the first `/cv` or `/chat` call without a valid cookie | the identity provider |
| Checks | signature (`hmac.compare_digest`); a bad or missing cookie gets a new identity | signature against the issuer's JWKS, `exp`, `iss` = `OIDC_ISSUER`, `aud` = `OIDC_AUDIENCE`, `sub` present; asymmetric algorithms only; 30 s leeway |
| Principal | `session:<id>` | `oidc:<sub>` |
| No credential | never (a cookie is issued) | 401 + `WWW-Authenticate: Bearer` |
| Chat limits keyed by | client address | principal |
| `/jobs/*` | public | public, or token required with `AUTH_PROTECT_SEARCH=true` |

The prefixes keep the two id spaces apart, so switching modes cannot hand one user's CVs to
another; it just orphans the old ones until retention removes them.

**Session cookie.** `HttpOnly`, `SameSite=Lax`, `Path=/`, `Max-Age` 180 days, `Secure` unless
`SESSION_COOKIE_SECURE=false` (browsers keep Secure cookies on `http://localhost`, so local
development does not need to turn it off). The id says nothing about the user and stores
nothing server-side. The cookie is set by `SessionCookieMiddleware` from what `SessionAuth`
put in the request state, so error responses carry it too. No new dependency: stdlib `hmac`
(itsdangerous is not installed, and a signed random id needs nothing more).

**Secret handling.** The API refuses to start in session mode without a `SESSION_SECRET` of at
least 32 characters, unless `AUTH_INSECURE_DEV=true`, which signs with a random key made at
startup (sessions, and with them access to uploaded CVs, end on every restart and differ
between replicas). The check runs in `create_app`, not in `Settings`, so the worker, retention
and MCP processes do not need the secret.

**OIDC.** PyJWT (`pyjwt[crypto]`): `JwksKeys` reads `jwks_uri` from
`OIDC_ISSUER/.well-known/openid-configuration` once, then `PyJWKClient` caches the key set
(5 min) and refetches it when a token names an unknown `kid`, which covers key rotation.
Discovery or JWKS failures are `AuthUnavailable` (503), not 401, so an IdP outage is not
reported as bad credentials. Only RS/PS/ES/EdDSA algorithms are accepted, which rules out the
HS256-with-the-public-key confusion.

**CSRF.** In session mode the browser sends the cookie on its own, so state-changing requests
need protection. `SameSite=Lax` keeps the cookie off cross-site subrequests. Same-site pages
on sibling subdomains still get it, so `reject_cross_site` also rejects (403 `csrf_rejected`)
any `POST`/`PUT`/`PATCH`/`DELETE` with `Sec-Fetch-Site: cross-site`, and any `POST` whose
`Content-Type` is not `application/json`. A form or a `no-cors` fetch can only send "simple"
requests (form encodings or `text/plain`, never `DELETE`), and a JSON body needs a CORS
preflight that this API never answers (it has no CORS middleware). FastAPI would otherwise
parse a body without a content type as JSON. No token is needed on top of that.

**Rate limits.** In session mode the per-client chat limits stay on the client address
(through `FORWARDED_ALLOW_IPS` behind the proxy): deleting a cookie gives a fresh identity,
so keying on it would let anyone reset their limit. Combining address and principal would only
add a key a client controls. In oidc mode an identity costs an account at the IdP, so the
principal is the key, and users behind one NAT do not share a limit. `llm_usage.client_id`
then holds `oidc:<sub>`, and retention clears it after `USAGE_CLIENT_RETENTION_DAYS` like the
address.

**Security headers.** `SecurityHeadersMiddleware` adds `X-Content-Type-Options: nosniff`,
`Referrer-Policy: no-referrer` and `X-Frame-Options: DENY` everywhere, and
`Content-Security-Policy: default-src 'none'; frame-ancestors 'none'` to API responses. The
page gets its own CSP: scripts only from the two pinned jsdelivr URLs (also SRI-pinned) and the
SHA-256 of the page's inline script, styles only from the hash of its inline style block,
`connect-src 'self'`, `frame-ancestors 'none'`. The hashes are computed from `index.html` at
startup, so editing the page cannot break or silently widen the policy. `/docs` and `/redoc`
keep FastAPI's Swagger UI (CDN) and get no CSP.

**UI.** Session mode is invisible: the cookie comes with the first call (`credentials:
"same-origin"`). In oidc mode a 401 opens a small field for a bearer token, kept in
`sessionStorage` (one tab, gone when it closes) and sent as `Authorization`. A login flow in
the static page is out of scope; for real users put an authenticating proxy in front (below).

**MCP over HTTP.** `jobmatcher-mcp --http` refuses to start on a non-loopback `--host` unless
`MCP_HTTP_TOKEN` (at least 32 characters) is set; with a token, every request needs
`Authorization: Bearer <token>` (constant-time comparison), on loopback too. stdio is
unchanged: the client that starts the process is the user.

## Consequences

- A CV is reachable only from the browser (or IdP account) that uploaded it. Clearing cookies,
  another browser or a restart with `AUTH_INSECURE_DEV` lose access; the UI then gets 404,
  forgets the id and asks for the CV again. That is the intended trade for anonymous use.
- `cv_id` alone is worthless to anyone else, so it no longer needs to be treated as a secret.
- Production needs `SESSION_SECRET` (shared by all API replicas; rotating it ends every
  session).
- `/`, `/healthz`, `/readyz` (and metrics, where served) stay unauthenticated.

## Alternatives considered

- **Keep the UUID as a capability, but out of `localStorage`.** Still a bearer token in every
  request body; no answer for a deployment with real users.
- **Server-side sessions table.** Revocation and server-side expiry, at the cost of a table and
  a query per request, for an identity that holds nothing. The signed id is enough.
- **itsdangerous / Starlette `SessionMiddleware`.** A new dependency and a signed JSON
  session dict, where one HMAC over a random id does the job.
- **OIDC login in the static page (PKCE).** A client id, redirect handling and token refresh in
  hand-written JavaScript; an authenticating proxy is the standard and safer answer.

## Putting a proxy in front (oidc mode)

[oauth2-proxy](https://oauth2-proxy.github.io/oauth2-proxy/) (or Pomerium, or an API gateway)
signs users in and forwards requests with the ID or access token as a bearer header. Run it
between Caddy and the API, e.g. with `--pass-authorization-header=true` (or
`--set-authorization-header=true` in nginx `auth_request` mode) and the same client configured
as `OIDC_AUDIENCE`. The API then validates the token itself, so a misrouted request that skips
the proxy still gets 401. The UI's token field is not needed then.
