# Play Console — Data safety answers (BubuTracker)

Use these when filling **App content → Data safety**. Adjust if your production setup differs.

## Does your app collect or share user data?

**Yes** — collects. Shares only with other users the account holder has approved (in-app tracking consent), not with advertisers.

## Data collected

| Data type | Collected | Shared | Required / Optional | Purpose |
|---|---|---|---|---|
| Email address | Yes | Yes — only with users whose tracking request you accept | Required | Account management, App functionality |
| Name (first/last) | Yes | Yes — only with users whose tracking request you accept | Optional | App functionality / profile |
| User IDs (Auth0 subject) | Yes | No | Required | Account management |
| Approximate location | Yes | Yes — only to users with accepted tracking | Optional (permission) | App functionality |
| Precise location | Yes | Yes — only to users with accepted tracking | Optional (permission) | App functionality |
| Crash logs | Yes (via Play if enabled) | Shared with Google Play | Optional | Analytics / diagnostics |
| Other: tracking relationship emails | Yes | No | Required for the feature | App functionality |

Auth0 processes the email address to authenticate you but does not receive it as a distinct "shared" disclosure beyond normal login processing — the disclosure above is specifically about the peer-to-peer sharing with other app users once a tracking request is accepted, which is the sharing Play's form is asking about.

## Location

- **Collected:** yes (precise and approximate when permission granted).
- **Shared:** yes, with other BubuTracker users the person has allowed — **not** sold, **not** for ads.
- **Ephemeral?** **No** — the API stores the latest known location in Postgres (`locations` table); it is not held only in memory.
- **Background:** **No** background location permission is requested on either platform (`ACCESS_BACKGROUND_LOCATION` was dropped from the Android manifest; iOS only declares `NSLocationWhenInUseUsageDescription`). Location is uploaded only while the app is open, via a foreground service on Android with a persistent "sharing location" notification.

## Security practices

- Data encrypted in transit: **Yes**, once deployed behind a real HTTPS reverse proxy — the API binary itself still listens on plain HTTP (`cmd/api/main.go`), so this answer is only true if you've actually put TLS in front of it in production. Don't check this box until that's done.
- Users can request deletion: **Yes** — `DELETE /api/v1/users/me` deletes the account (cascades to location and tracking data), exposed in-app as "Delete my account" on the profile screen on both platforms.

## Privacy policy

Required. Host `play/PRIVACY_POLICY.md` and link it.

## Ads

**No** ads SDK in the project — declare **No, my app does not contain ads**.
