// Reverse geocoding via Nominatim, cached content-addressed by rounded
// coordinates - mirrors the Android app's geocodeCache in MapsActivity.kt so
// both clients get the same behavior: fewer calls to a rate-limit-sensitive
// third party, and no stale "Unknown location" hiding a transient failure
// (only successful lookups are cached).

const CACHE_TTL_MS = 6 * 60 * 60 * 1000;
const ROUNDING_FACTOR = 10_000; // ~11 m grid cells (4 decimal places)
const MAX_CACHE_ENTRIES = 200;

type CacheEntry = { placeName: string; cachedAtMillis: number };

const cache = new Map<string, CacheEntry>();

function cacheKey(latitude: number, longitude: number): string {
  const roundedLat = Math.round(latitude * ROUNDING_FACTOR) / ROUNDING_FACTOR;
  const roundedLon = Math.round(longitude * ROUNDING_FACTOR) / ROUNDING_FACTOR;
  return `${roundedLat},${roundedLon}`;
}

function shortenPlaceName(displayName: string): string {
  return displayName
    .split(",")
    .map((part) => part.trim())
    .filter(Boolean)
    .slice(0, 2)
    .join(", ");
}

// Insertion order in a Map is iteration order, so the first key is the
// oldest - a cheap FIFO stand-in for a real LRU, fine at this size.
function evictOldestIfFull(): void {
  if (cache.size < MAX_CACHE_ENTRIES) {
    return;
  }
  const oldestKey = cache.keys().next().value;
  if (oldestKey !== undefined) {
    cache.delete(oldestKey);
  }
}

export async function reverseGeocode(latitude: number, longitude: number): Promise<string | null> {
  const key = cacheKey(latitude, longitude);
  const cached = cache.get(key);
  if (cached && Date.now() - cached.cachedAtMillis < CACHE_TTL_MS) {
    return cached.placeName;
  }

  try {
    const response = await fetch(
      `https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${latitude}&lon=${longitude}&zoom=16&addressdetails=0`,
      { headers: { "User-Agent": "BubuTrackerIOS" } }
    );
    const body = await response.json();
    const displayName = typeof body.display_name === "string" ? body.display_name : null;
    if (!displayName) {
      return null;
    }
    const placeName = shortenPlaceName(displayName);
    evictOldestIfFull();
    cache.set(key, { placeName, cachedAtMillis: Date.now() });
    return placeName;
  } catch {
    return null;
  }
}
