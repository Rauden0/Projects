# Production / internal-track E2E checklist

Run after installing the release build (Play internal track **or** `play/out/universal.apk` sideload).

## Preflight

- [ ] `secrets.properties` has real Auth0 domain, client id, audience, HTTPS API URL (no Maps key needed — osmdroid)
- [ ] Rebuilt: `./gradlew :app:bundleRelease`
- [ ] Go API reachable over HTTPS; Auth0 audience matches

## Device checks

1. Install release build (Play opt-in or `adb install -r play/out/universal.apk`).
2. Cold start → Login screen.
3. Auth0 login succeeds; lands on map.
4. Grant location → own marker appears / location posts succeed (no API errors).
5. Track a second test account by email → request appears for them.
6. Accept request on account B → A sees B on map (or vice versa).
7. Revoke / stop tracking → marker disappears on next poll.
8. Open profile → edit name → save succeeds.
9. Log out → back to login; tokens cleared (re-open app stays logged out).
10. Profile → "Delete my account" → confirm → back to login; signing back in with the same Auth0 identity provisions a fresh, empty account (old tracking relationships and location are gone).

## Local artifact verification (done in CI / this deploy)

```bash
bash play/verify-release.sh
# or manually:
java -jar play/out/bundletool.jar validate --bundle BubuTracker/app/build/outputs/bundle/release/app-release.aab
```

| Check | Expected |
|---|---|
| Package | `com.martinmucka.bubutracker` |
| Auth0 path | `/android/com.martinmucka.bubutracker/callback` |
| Signature | apksigner VERIFY SUCCESS |
| Version | `1.0.0` / versionCode `1` |
