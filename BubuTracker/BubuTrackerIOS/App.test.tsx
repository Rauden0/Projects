import { render, screen, waitFor } from "@testing-library/react-native";
import React from "react";
import App from "./App";

jest.mock("./src/api/client", () => ({
  setOnSessionExpired: jest.fn(),
  api: { getMe: jest.fn() },
}));
jest.mock("./src/auth/session", () => ({
  isLoggedIn: jest.fn(),
}));
jest.mock("./src/auth/auth0", () => ({
  logout: jest.fn(),
}));

// Every account must have a name (enforced everywhere, see ProfileScreen), so
// the only thing the routing state machine needs from these screens for this
// test is which one rendered - stub them down to a bare marker.
jest.mock("./src/screens/LoginScreen", () => {
  const { Text } = require("react-native");
  return { __esModule: true, default: () => <Text testID="screen-login" /> };
});
jest.mock("./src/screens/ProfileScreen", () => {
  const { Text } = require("react-native");
  return {
    __esModule: true,
    default: (props: { requireName: boolean }) => (
      <Text testID={props.requireName ? "screen-profile-required" : "screen-profile-edit"} />
    ),
  };
});
jest.mock("./src/screens/MapScreen", () => {
  const { Text } = require("react-native");
  return { __esModule: true, default: () => <Text testID="screen-map" /> };
});

import { api } from "./src/api/client";
import { isLoggedIn } from "./src/auth/session";
import { clearProfile } from "./src/state/profileStore";

describe("App routing", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    // profileStore is real (not mocked) so App.tsx's caching behaves like
    // production; clear it between tests so one test's cached profile can't
    // leak into the next and short-circuit its getMe() mock.
    clearProfile();
  });

  it("routes to login when there is no stored session", async () => {
    (isLoggedIn as jest.Mock).mockResolvedValue(false);

    render(<App />);

    await waitFor(() => expect(screen.getByTestId("screen-login")).toBeTruthy());
    expect(api.getMe).not.toHaveBeenCalled();
  });

  it("routes to the mandatory-name profile screen when the account has no name yet", async () => {
    (isLoggedIn as jest.Mock).mockResolvedValue(true);
    (api.getMe as jest.Mock).mockResolvedValue({ id: "u1", email: "a@b.com", firstName: "", lastName: "" });

    render(<App />);

    await waitFor(() => expect(screen.getByTestId("screen-profile-required")).toBeTruthy());
  });

  it("routes straight to the map when logged in with a name already set", async () => {
    (isLoggedIn as jest.Mock).mockResolvedValue(true);
    (api.getMe as jest.Mock).mockResolvedValue({ id: "u1", email: "a@b.com", firstName: "Bubu", lastName: "" });

    render(<App />);

    await waitFor(() => expect(screen.getByTestId("screen-map")).toBeTruthy());
  });

  it("falls back to login when the stored session is stale and getMe fails", async () => {
    (isLoggedIn as jest.Mock).mockResolvedValue(true);
    (api.getMe as jest.Mock).mockRejectedValue(new Error("401"));

    render(<App />);

    await waitFor(() => expect(screen.getByTestId("screen-login")).toBeTruthy());
  });
});
