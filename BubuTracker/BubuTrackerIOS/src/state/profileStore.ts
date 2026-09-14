// Process-scoped single source of truth for the signed-in user's own profile,
// mirroring Android's UserProfileStore.kt so App.tsx, MapScreen, and
// ProfileScreen don't each independently hit GET /users/me when one of them
// just fetched it moments ago.
//
// Callers write the result of every successful getMe/updateMe via setProfile,
// and clear it on logout/account deletion via clearProfile. getFreshProfile
// is a soft cache: it returns null once SOFT_TTL_MS has elapsed so a caller
// falls back to a real fetch instead of serving indefinitely stale data.

import type { UserProfile } from "../types";

const SOFT_TTL_MS = 5 * 60_000;

let cached: UserProfile | null = null;
let cachedAtMillis = 0;

// nowMillis is a parameter (defaulting to real time) rather than read
// internally so a test can pin it instead of sleeping past the TTL.
export function getFreshProfile(nowMillis: number = Date.now()): UserProfile | null {
  if (!cached) {
    return null;
  }
  return nowMillis - cachedAtMillis < SOFT_TTL_MS ? cached : null;
}

export function setProfile(profile: UserProfile): void {
  cached = profile;
  cachedAtMillis = Date.now();
}

export function clearProfile(): void {
  cached = null;
  cachedAtMillis = 0;
}
