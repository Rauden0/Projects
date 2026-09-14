import { formatLastSeenAt } from "./lastSeen";

const NOW = new Date("2026-09-13T12:00:00.000Z").getTime();

describe("formatLastSeenAt", () => {
  it("returns a placeholder when there is no location yet", () => {
    expect(formatLastSeenAt(null, NOW)).toBe("No location reported yet");
  });

  it("returns a placeholder for an unparseable timestamp", () => {
    expect(formatLastSeenAt("not-a-date", NOW)).toBe("No location reported yet");
  });

  it("says 'Just now' for a timestamp within the last 30 seconds", () => {
    const updatedAt = new Date(NOW - 10_000).toISOString();
    expect(formatLastSeenAt(updatedAt, NOW)).toBe("Just now");
  });

  it("formats a few minutes ago", () => {
    const updatedAt = new Date(NOW - 5 * 60_000).toISOString();
    expect(formatLastSeenAt(updatedAt, NOW)).toBe("5 minutes ago");
  });

  it("formats exactly one minute ago in the singular", () => {
    const updatedAt = new Date(NOW - 60_000).toISOString();
    expect(formatLastSeenAt(updatedAt, NOW)).toBe("1 minute ago");
  });

  it("switches to an hour bucket past 60 minutes", () => {
    const updatedAt = new Date(NOW - 3 * 3_600_000).toISOString();
    expect(formatLastSeenAt(updatedAt, NOW)).toBe("3 hours ago");
  });

  it("switches to a day bucket past 24 hours, unlike a bare hour count", () => {
    const updatedAt = new Date(NOW - 2 * 86_400_000).toISOString();
    expect(formatLastSeenAt(updatedAt, NOW)).toBe("2 days ago");
  });

  it("clamps a future timestamp (clock skew) to 'Just now' instead of showing future tense", () => {
    const updatedAt = new Date(NOW + 7 * 60_000).toISOString();
    expect(formatLastSeenAt(updatedAt, NOW)).toBe("Just now");
  });
});
