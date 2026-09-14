jest.mock("../auth/auth0", () => ({
  refreshAccessToken: jest.fn(),
}));
jest.mock("../auth/session", () => ({
  getAccessToken: jest.fn(),
  clearSession: jest.fn(),
}));

import { refreshAccessToken } from "../auth/auth0";
import { clearSession, getAccessToken } from "../auth/session";
import { api, ApiError, setOnSessionExpired } from "./client";

function jsonResponse(status: number, body: unknown) {
  return { status, ok: status >= 200 && status < 300, json: async () => body };
}

describe("api client 401 handling", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    (getAccessToken as jest.Mock).mockResolvedValue("stale-token");
    globalThis.fetch = jest.fn();
    setOnSessionExpired(() => {});
  });

  it("refreshes once and retries the request on a 401", async () => {
    (globalThis.fetch as jest.Mock)
      .mockResolvedValueOnce(jsonResponse(401, { error: { message: "expired" } }))
      .mockResolvedValueOnce(jsonResponse(200, { id: "u1", email: "a@b.com" }));
    (refreshAccessToken as jest.Mock).mockResolvedValue("new-token");
    // Mirrors the real refreshAccessToken(), which persists the new token via
    // saveAccessToken() before the retried request re-reads it from storage.
    (getAccessToken as jest.Mock).mockResolvedValueOnce("stale-token").mockResolvedValueOnce("new-token");

    const result = await api.getMe();

    expect(result).toEqual({ id: "u1", email: "a@b.com" });
    expect(refreshAccessToken).toHaveBeenCalledTimes(1);
    const retriedRequest = (globalThis.fetch as jest.Mock).mock.calls[1][1];
    expect(retriedRequest.headers.Authorization).toBe("Bearer new-token");
  });

  it("clears the session and reports expiry when the refresh itself fails", async () => {
    (globalThis.fetch as jest.Mock).mockResolvedValue(jsonResponse(401, {}));
    (refreshAccessToken as jest.Mock).mockResolvedValue(null);
    const onExpired = jest.fn();
    setOnSessionExpired(onExpired);

    await expect(api.getMe()).rejects.toBeInstanceOf(ApiError);

    expect(clearSession).toHaveBeenCalledTimes(1);
    expect(onExpired).toHaveBeenCalledTimes(1);
  });

  it("does not retry a second time when the retried request also 401s", async () => {
    (globalThis.fetch as jest.Mock).mockResolvedValue(jsonResponse(401, {}));
    (refreshAccessToken as jest.Mock).mockResolvedValue("new-token");

    await expect(api.getMe()).rejects.toBeInstanceOf(ApiError);

    expect(globalThis.fetch).toHaveBeenCalledTimes(2);
    expect(refreshAccessToken).toHaveBeenCalledTimes(1);
  });

  it("dedups concurrent 401s from different calls into a single refresh", async () => {
    (globalThis.fetch as jest.Mock).mockImplementation(() => Promise.resolve(jsonResponse(401, {})));
    (refreshAccessToken as jest.Mock).mockResolvedValue("shared-new-token");

    // Two different endpoints hitting 401 around the same time, as would
    // happen with a location push racing a tracked-locations poll.
    const [meResult, trackedResult] = await Promise.allSettled([api.getMe(), api.getTrackedUsers()]);

    expect(meResult.status).toBe("rejected");
    expect(trackedResult.status).toBe("rejected");
    // The whole point of the fix: exactly one refresh call, not one per request.
    expect(refreshAccessToken).toHaveBeenCalledTimes(1);
  });
});
