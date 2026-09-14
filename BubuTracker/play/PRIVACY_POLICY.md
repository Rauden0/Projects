# BubuTracker — Privacy Policy

**Last updated:** 13 September 2026  
**Application ID:** `com.martinmucka.bubutracker`  
**Contact:** replace-with-your-email@example.com

This privacy policy describes how BubuTracker (“the App”) handles information when you use the Android application and its backend API.

## 1. Who we are

BubuTracker is a location-sharing app that lets signed-in users optionally share their device location with people they approve.

## 2. Information we collect

| Category | Examples | Why |
|---|---|---|
| Account | Email address, Auth0 user id, optional first/last name | Create and identify your account |
| Authentication | Access and refresh tokens (stored encrypted on device) | Keep you signed in |
| Location | Approximate/precise GPS coordinates while the App is in use and location sharing is active | Show your position on the map to people you have allowed |
| Tracking relationships | Email of people you request to track; accept/reject/revoke decisions | Manage who can see whose location |
| Diagnostics | Basic crash/performance data if you enable Play vitals / crash reporting | Improve stability |

We do **not** sell personal data. We do **not** use your location for advertising.

## 3. How location sharing works

- Location is uploaded to our API only while you are signed in and the App is open (foreground or, briefly, switching between its own screens) — leaving the App stops uploads.
- Other users can see your location **only** after you accept their tracking request (or equivalent consent in-product).
- You can revoke a follower’s access in the App; they will stop receiving your updates.

## 4. Third parties

- **Auth0** — sign-in and token issuance. See [Auth0 Privacy](https://auth0.com/privacy).
- **OpenStreetMap** — map tile imagery (anonymous tile requests only; no account data is sent). See [OSM Privacy Policy](https://osmfoundation.org/wiki/Privacy_Policy).
- **Google Play services (Fused Location)** — reads device location on Android. See [Google Privacy](https://policies.google.com/privacy).
- **Google Play** — distribution, and optionally crash/ANR vitals.

## 5. Data retention

Account and tracking relationship data are kept while your account exists. We keep only your latest reported location, not a history. You can permanently delete your account and all associated data at any time from the App’s profile screen (“Delete my account”), or by contacting the email above.

## 6. Security

Tokens on the device are encrypted (Android Keystore / Tink). API traffic in production uses HTTPS. Access to location data is limited to authenticated users with an accepted tracking relationship.

## 7. Children’s privacy

The App is not directed at children under 13 (or the minimum age in your country). We do not knowingly collect data from children.

## 8. Your choices

- Deny or revoke location permission in Android settings.
- Reject or revoke tracking requests in the App.
- Sign out to stop sending location from this device.
- Delete your account and all associated data at any time from the App’s profile screen, or by contacting the email above.

## 9. Changes

We may update this policy. The “Last updated” date will change when we do. Continued use after changes means you accept the updated policy.

## 10. Contact

Questions or deletion requests: **replace-with-your-email@example.com**

---

Host this file at a public HTTPS URL (GitHub Pages, your domain, etc.) and paste that URL into Play Console → App content → Privacy policy.
