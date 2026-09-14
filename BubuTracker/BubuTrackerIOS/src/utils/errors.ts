import { ApiError } from "../api/client";

export function apiErrorMessage(error: unknown, fallback = "Could not send the request. Please try again."): string {
  if (error instanceof ApiError) {
    switch (error.code) {
      case "already_exists":
        return "You already sent a request to this person, or you're already tracking them.";
      case "not_found":
        return "No BubuTracker account for that email. They need to open the app and sign in once first.";
      case "self_tracking":
        return "You can't track yourself.";
      case "invalid_argument":
        return "Enter a valid email address.";
      case "rate_limited":
        return "Too many requests — wait a moment and try again.";
      case "unauthenticated":
        return "Your session expired. Please sign in again.";
      default:
        if (error.status === 401) {
          return "Your session expired. Please sign in again.";
        }
        if (error.status === 404) {
          return "No BubuTracker account for that email. They need to open the app and sign in once first.";
        }
        if (error.status === 409) {
          return "You already sent a request to this person, or you're already tracking them.";
        }
        if (error.status === 429) {
          return "Too many requests — wait a moment and try again.";
        }
        return fallback;
    }
  }
  return "Couldn't reach the server. Check your connection and try again.";
}

export function actionErrorMessage(error: unknown): string {
  if (error instanceof ApiError) {
    if (error.status === 404 || error.code === "not_found") {
      return "That request is no longer available.";
    }
    if (error.status === 401 || error.code === "unauthenticated") {
      return "Your session expired. Please sign in again.";
    }
    if (error.status === 429 || error.code === "rate_limited") {
      return "Too many requests — wait a moment and try again.";
    }
  }
  if (error instanceof ApiError) {
    return "Please try again.";
  }
  return "Couldn't reach the server. Check your connection and try again.";
}
