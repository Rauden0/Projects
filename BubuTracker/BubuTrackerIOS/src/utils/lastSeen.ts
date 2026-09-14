// Locale-aware "last seen" phrasing, kept in its own module so it's testable
// and shared verbatim between MapScreen and anywhere else that needs it.
//
// Mirrors the Android app's `DateUtils.getRelativeTimeSpanString` behavior:
// locale-aware wording, and a step up to hour/day buckets once the value is
// large enough that a bare minute count stops being useful. `nowMs` is a
// parameter (rather than read internally) so tests can pin it.

const relativeTimeFormatter = new Intl.RelativeTimeFormat(undefined, { numeric: "auto" });

export function formatLastSeenAt(updatedAt: string | null, nowMs: number): string {
  if (!updatedAt) {
    return "No location reported yet";
  }
  const eventMs = Date.parse(updatedAt);
  if (Number.isNaN(eventMs)) {
    return "No location reported yet";
  }

  // Clamp to now so client/server clock skew never renders a future tense
  // ("in 7 minutes") for a location that was actually just reported.
  const clampedMs = Math.min(eventMs, nowMs);
  const diffSeconds = (clampedMs - nowMs) / 1000;

  const minutes = Math.round(diffSeconds / 60);
  if (minutes === 0) {
    return "Just now";
  }
  if (Math.abs(minutes) < 60) {
    return relativeTimeFormatter.format(minutes, "minute");
  }

  const hours = Math.round(diffSeconds / 3600);
  if (Math.abs(hours) < 24) {
    return relativeTimeFormatter.format(hours, "hour");
  }

  const days = Math.round(diffSeconds / 86400);
  return relativeTimeFormatter.format(days, "day");
}
