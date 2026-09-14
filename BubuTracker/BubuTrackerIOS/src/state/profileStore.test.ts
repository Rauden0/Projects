import { clearProfile, getFreshProfile, setProfile } from "./profileStore";
import type { UserProfile } from "../types";

const profile: UserProfile = {
  id: "u1",
  email: "alice@example.com",
  firstName: "Alice",
  lastName: "Smith",
  markerColor: "#FF9800",
};

describe("profileStore", () => {
  beforeEach(() => {
    // Module-level state persists across tests in this file; isolate each
    // test from the others' writes rather than assuming execution order.
    clearProfile();
  });

  it("returns null when nothing has been cached yet", () => {
    expect(getFreshProfile()).toBeNull();
  });

  it("returns the cached profile shortly after set", () => {
    setProfile(profile);

    expect(getFreshProfile()).toEqual(profile);
  });

  it("returns null once the soft TTL has elapsed", () => {
    const now = Date.now();
    setProfile(profile);

    expect(getFreshProfile(now + 4 * 60_000)).toEqual(profile);
    expect(getFreshProfile(now + 6 * 60_000)).toBeNull();
  });

  it("drops the cached profile immediately on clear", () => {
    setProfile(profile);
    clearProfile();

    expect(getFreshProfile()).toBeNull();
  });

  it("overwrites a previous entry and resets its freshness", () => {
    const stale = Date.now();
    setProfile(profile);

    const updated = { ...profile, firstName: "Alicia" };
    setProfile(updated);

    expect(getFreshProfile(stale + 4 * 60_000)).toEqual(updated);
  });
});
