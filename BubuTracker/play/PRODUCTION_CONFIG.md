# Auth0 + API setup (local ngrok → later paid host)

Maps: OpenStreetMap via osmdroid — **no Google Maps API key**.

## Auth0 (Native application)

1. Create Application → Native → note **Client ID** and **Domain**.
2. Allowed Callback URLs:
   ```
   bubutracker://YOUR_TENANT.auth0.com/android/com.martinmucka.bubutracker/callback
   ```
3. Allowed Logout URLs: same scheme/host as needed by the SDK.
4. Create an API with identifier matching `auth0Audience` (default `https://api.bubutracker`).
5. Put values into `BubuTracker/secrets.properties` **and** `BubuTrackerAPI-Go/.env`
   (`AUTH0_DOMAIN` / `AUTH0_AUDIENCE` must match the app).

## Local testing with ngrok (now)

One-shot helper (starts Docker Compose + ngrok, patches API URLs in secrets):

```bash
# once: install ngrok + authtoken — https://ngrok.com/download
ngrok config add-authtoken <YOUR_TOKEN>

bash play/dev-ngrok.sh
cd BubuTracker && ./gradlew :app:installDebug
```

Requirements: Docker running, `BubuTrackerAPI-Go/.env` and `BubuTracker/secrets.properties` with real Auth0 values (already present if you filled them earlier).

Smoke:

```bash
curl -sS "$(bash play/dev-ngrok.sh --url-only)healthz"
curl -sS "$(bash play/dev-ngrok.sh --url-only)readyz"
```

Free ngrok URLs change when you restart the tunnel — re-run `dev-ngrok.sh`, then rebuild/install the app.

## Production API (when you pay monthly)

1. Deploy `BubuTrackerAPI-Go` + Postgres behind HTTPS (VPS / Render / etc.).
2. Set `apiBaseUrlRelease=https://your.api.host/` in secrets (trailing slash).
3. Rebuild: `./gradlew :app:bundleRelease` and upload the AAB to Play.

## Rebuild

```bash
cd BubuTracker
./gradlew :app:bundleRelease
```
