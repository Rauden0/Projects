# Deploy status (13 Sep 2026)

## Done in-repo

| Item | Status |
|---|---|
| `applicationId` → `com.martinmucka.bubutracker` | Done |
| Secrets via `secrets.properties` (Auth0, API URLs) | Auth0 set; API URLs via `play/dev-ngrok.sh` |
| Maps | OSM/osmdroid — no Google Maps key |
| Upload keystore + `keystore.properties` | Generated (gitignored) |
| Signed `app-release.aab` | `BubuTracker/app/build/outputs/bundle/release/` and `play/out/` |
| Universal APK verified (`apksigner`, package id, v2/v3) | Done |
| Play listing / Data safety / privacy materials | `play/` |
| Upload helper | `play/upload-internal.sh` (needs service account JSON) |
| Unit tests | Passed |

## Local API testing (ngrok) — prepared

| Item | Status |
|---|---|
| Auth0 in `.env` + `secrets.properties` (matched) | Done |
| Helper `play/dev-ngrok.sh` | Done |
| Install ngrok + authtoken | **You** — https://ngrok.com/download |
| `bash play/dev-ngrok.sh` then installDebug | **You** |
| Paid always-on host | Later (when Play testers need uptime) |

## Blocked on you (cannot automate)

1. **ngrok** — install binary, `ngrok config add-authtoken …`, run `bash play/dev-ngrok.sh`, then `./gradlew :app:installDebug`.
2. **Google Play Console** — sign in, pay the $25 fee if needed, create the app, paste privacy URL + Data safety from `play/`, upload the AAB to **Internal testing**.
3. **Host privacy** — publish `play/privacy/index.html` (or the markdown) on HTTPS.
4. **Device E2E** — follow `play/E2E_CHECKLIST.md` after install.

Back up `BubuTracker/upload-keystore.jks` + `keystore.properties` offline.
