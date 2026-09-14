# Play Console upload checklist

Application ID: **`com.martinmucka.bubutracker`**  
Release artifact: `BubuTracker/app/build/outputs/bundle/release/app-release.aab`

## Before you open Play Console

1. Fill real values in `BubuTracker/secrets.properties` (Auth0 domain/client/audience, HTTPS API URL). No Maps key needed — the app uses osmdroid/OpenStreetMap, not the Google Maps SDK.
2. Rebuild: `./gradlew :app:bundleRelease`
3. Host [`PRIVACY_POLICY.md`](PRIVACY_POLICY.md) at a public HTTPS URL.
4. Have a Google account ready for the [Play Console](https://play.google.com/console) ($25 one-time).

## Create the app

1. **Create app** → App name `BubuTracker` → Language → App → Free (or Paid).
2. Declarations: let Play walk you through (spam, permissions, News, etc.).
3. **App content**
   - Privacy policy URL
   - Data safety → follow [`DATA_SAFETY.md`](DATA_SAFETY.md)
   - Ads → No
   - Target audience / content ratings questionnaire
4. **Store listing** → paste [`STORE_LISTING.md`](STORE_LISTING.md); upload icon + screenshots before production.

## Signing

1. First upload: enroll in **Play App Signing** (recommended default).
2. Upload the AAB signed with `upload-keystore.jks`.
3. Upload SHA-1 (this machine’s keystore): see `UPLOAD_CERT_SHA1.txt` after build scripts run — no Maps API key restriction step needed (osmdroid needs no key).

## Internal testing (do this first)

1. **Testing → Internal testing → Create new release**
2. Upload `app-release.aab`
3. Release name: `1.0.0 (1)`
4. Add yourself as a tester (email list).
5. Save → Review → Start rollout to Internal testing.
6. Open the opt-in link on a device signed into the tester Google account → install.

## Auth0 callback (must match Play package)

```
bubutracker://YOUR_REAL_TENANT.auth0.com/android/com.martinmucka.bubutracker/callback
```

Also allow the Auth0 scheme / Allowed Origins for the native app client.

## Production API

Deploy `BubuTrackerAPI-Go` with TLS. Set `apiBaseUrlRelease` to that URL (trailing `/`). Auth0 audience on the API must match `auth0Audience` in secrets.
