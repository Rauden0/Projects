import { fireEvent, render, screen } from "@testing-library/react-native";
import React from "react";
import ProfileScreen from "./ProfileScreen";

jest.mock("../api/client", () => ({
  api: { getMe: jest.fn(), updateMe: jest.fn(), deleteMe: jest.fn() },
}));

import { api } from "../api/client";
import { clearProfile } from "../state/profileStore";

const profile = { id: "u1", email: "a@b.com", firstName: "Alice", lastName: "A", markerColor: "#FF9800" };

describe("ProfileScreen account deletion", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    // profileStore is real (not mocked); clear it so one test's cached
    // profile can't short-circuit the next test's getMe() mock.
    clearProfile();
    (api.getMe as jest.Mock).mockResolvedValue(profile);
  });

  it("deletes the account and notifies the caller after confirming", async () => {
    (api.deleteMe as jest.Mock).mockResolvedValue(undefined);
    const onAccountDeleted = jest.fn();

    await render(
      <ProfileScreen requireName={false} onSaved={() => {}} onBack={() => {}} onAccountDeleted={onAccountDeleted} />
    );

    fireEvent.press(await screen.findByText("Delete my account"));
    fireEvent.press(await screen.findByText("Delete"));

    await screen.findByText("Delete my account"); // still on screen; just confirms the state settled
    expect(api.deleteMe).toHaveBeenCalledTimes(1);
    expect(onAccountDeleted).toHaveBeenCalledTimes(1);
  });

  it("does not delete anything when the confirmation is cancelled", async () => {
    const onAccountDeleted = jest.fn();

    await render(
      <ProfileScreen requireName={false} onSaved={() => {}} onBack={() => {}} onAccountDeleted={onAccountDeleted} />
    );

    fireEvent.press(await screen.findByText("Delete my account"));
    fireEvent.press(await screen.findByText("Cancel"));

    expect(api.deleteMe).not.toHaveBeenCalled();
    expect(onAccountDeleted).not.toHaveBeenCalled();
  });

  it("does not offer account deletion on the mandatory-name screen", async () => {
    await render(
      <ProfileScreen requireName onSaved={() => {}} onBack={() => {}} onAccountDeleted={() => {}} />
    );

    await screen.findByText("Complete your profile");
    expect(screen.queryByText("Delete my account")).toBeNull();
  });
});
