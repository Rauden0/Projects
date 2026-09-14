# Play Console — what this folder prepares

Everything needed to publish **BubuTracker** (`com.martinmucka.bubutracker`) except your Google login and the $25 developer fee.

| File | Purpose |
|---|---|
| [UPLOAD_CHECKLIST.md](UPLOAD_CHECKLIST.md) | Step-by-step Play Console flow |
| [DATA_SAFETY.md](DATA_SAFETY.md) | Answers for the Data safety form |
| [STORE_LISTING.md](STORE_LISTING.md) | Title, descriptions, graphics checklist |
| [PRIVACY_POLICY.md](PRIVACY_POLICY.md) / [privacy/index.html](privacy/index.html) | Host publicly, paste URL in Console |
| [PRODUCTION_CONFIG.md](PRODUCTION_CONFIG.md) | Auth0, API, ngrok local test |
| [dev-ngrok.sh](dev-ngrok.sh) | Start API+Postgres+ngrok and patch secrets URLs |
| [UPLOAD_CERT_SHA1.txt](UPLOAD_CERT_SHA1.txt) | Upload keystore SHA-1 (reference; no Maps key restriction needed — osmdroid) |
| [verify-release.sh](verify-release.sh) | Local AAB → device install checks |
| [upload-internal.sh](upload-internal.sh) | Optional API upload via service account |
| `../BubuTracker/app/build/outputs/bundle/release/app-release.aab` | Signed bundle to upload |

## Upload now (manual — recommended first time)

1. Open https://play.google.com/console and create / sign into a developer account.
2. Create app → complete App content using the docs above.
3. **Testing → Internal testing → Create release** → upload the `.aab`.
4. Add your Google account as a tester → start rollout → install from the opt-in link.

## Upload via API (optional)

1. Play Console → Setup → API access → link Cloud project → create service account with **Release to testing tracks** permission.
2. Download JSON key to `play/service-account.json` (gitignored).
3. `bash play/upload-internal.sh`

## Blockers only you can finish

- Pay / accept Play developer agreement
- Real Auth0 + HTTPS API values in `BubuTracker/secrets.properties`, then rebuild (no Maps key needed — osmdroid)
- Host privacy HTML and paste URL
- Screenshots / feature graphic before production (internal testing can proceed without full store listing graphics in many cases, but production cannot)
