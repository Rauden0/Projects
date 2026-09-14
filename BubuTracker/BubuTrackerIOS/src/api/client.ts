import { API_BASE_URL } from "../config";
import { refreshAccessToken } from "../auth/auth0";
import { clearSession, getAccessToken } from "../auth/session";
import type { TrackedLocation, UpdateProfileRequest, UserProfile } from "../types";

export class ApiError extends Error {
  constructor(
    public status: number,
    message: string,
    public code?: string
  ) {
    super(message);
  }
}

let onSessionExpired: (() => void) | null = null;
export function setOnSessionExpired(handler: () => void): void {
  onSessionExpired = handler;
}

// Two calls can each hit a 401 around the same time (e.g. a location push and
// a tracked-locations poll) and both try to refresh concurrently. Without this
// dedup, the loser would refresh with an already-rotated refresh token (Auth0
// rotates refresh tokens by default) and force a spurious logout even though
// the session was fine. Sharing one in-flight refresh promise fixes that.
let inFlightRefresh: Promise<string | null> | null = null;
function refreshAccessTokenOnce(): Promise<string | null> {
  if (!inFlightRefresh) {
    inFlightRefresh = refreshAccessToken().finally(() => {
      inFlightRefresh = null;
    });
  }
  return inFlightRefresh;
}

async function request<T>(
  path: string,
  options: { method?: string; body?: unknown } = {},
  isRetry = false
): Promise<T> {
  const accessToken = await getAccessToken();
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: options.method ?? "GET",
    headers: {
      "Content-Type": "application/json",
      ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
    },
    body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
  });

  if (response.status === 401 && !isRetry) {
    const newToken = await refreshAccessTokenOnce();
    if (newToken) {
      return request<T>(path, options, true);
    }
    await clearSession();
    onSessionExpired?.();
    throw new ApiError(401, "Session expired");
  }

  if (!response.ok) {
    let code: string | undefined;
    let message = `Request to ${path} failed with ${response.status}`;
    try {
      const body = (await response.json()) as {
        error?: { code?: string; message?: string };
      };
      code = body.error?.code;
      if (body.error?.message) {
        message = body.error.message;
      }
    } catch {
    }
    throw new ApiError(response.status, message, code);
  }

  if (response.status === 204) {
    return undefined as T;
  }
  return (await response.json()) as T;
}

export const api = {
  getMe: () => request<UserProfile>("/users/me"),
  updateMe: (body: UpdateProfileRequest) =>
    request<UserProfile>("/users/me", { method: "PATCH", body }),
  deleteMe: () => request<void>("/users/me", { method: "DELETE" }),

  updateMyLocation: (latitude: number, longitude: number) =>
    request<void>("/locations/me", { method: "POST", body: { latitude, longitude } }),
  getTrackedLocations: () => request<TrackedLocation[]>("/locations/tracked"),

  getTrackedUsers: () => request<UserProfile[]>("/tracking"),
  addTracking: (email: string) => request<void>("/tracking", { method: "POST", body: { email } }),
  removeTracking: (trackedUserId: string) =>
    request<void>(`/tracking/${trackedUserId}`, { method: "DELETE" }),

  getIncomingRequests: () => request<UserProfile[]>("/tracking/requests"),
  getOutgoingRequests: () => request<UserProfile[]>("/tracking/outgoing"),
  acceptRequest: (trackerId: string) =>
    request<void>(`/tracking/requests/${trackerId}/accept`, { method: "POST" }),
  getFollowers: () => request<UserProfile[]>("/tracking/followers"),
  removeFollower: (trackerId: string) =>
    request<void>(`/tracking/followers/${trackerId}`, { method: "DELETE" }),
};
