import React, { useEffect, useState } from "react";
import { ActivityIndicator, SafeAreaView, StyleSheet } from "react-native";
import { setOnSessionExpired, api } from "./src/api/client";
import { isLoggedIn } from "./src/auth/session";
import { logout } from "./src/auth/auth0";
import LoginScreen from "./src/screens/LoginScreen";
import ProfileScreen from "./src/screens/ProfileScreen";
import MapScreen from "./src/screens/MapScreen";
import { clearProfile, getFreshProfile, setProfile } from "./src/state/profileStore";

type Screen = "loading" | "login" | "profile-required" | "profile-edit" | "map";

export default function App() {
  const [screen, setScreen] = useState<Screen>("loading");

  useEffect(() => {
    setOnSessionExpired(() => {
      clearProfile();
      setScreen("login");
    });
  }, []);

  useEffect(() => {
    (async () => {
      if (!(await isLoggedIn())) {
        setScreen("login");
        return;
      }
      await routeBasedOnProfile();
    })();
  }, []);

  const routeBasedOnProfile = async () => {
    try {
      const cachedProfile = getFreshProfile();
      const profile = cachedProfile ?? (await api.getMe());
      if (!cachedProfile) {
        setProfile(profile);
      }
      if (!profile.firstName.trim() && !profile.lastName.trim()) {
        setScreen("profile-required");
      } else {
        setScreen("map");
      }
    } catch {
      setScreen("login");
    }
  };

  const handleLogout = async () => {
    setScreen("loading");
    await logout();
    clearProfile();
    setScreen("login");
  };

  switch (screen) {
    case "loading":
      return (
        <SafeAreaView style={styles.loading}>
          <ActivityIndicator size="large" />
        </SafeAreaView>
      );
    case "login":
      return <LoginScreen onLoggedIn={routeBasedOnProfile} />;
    case "profile-required":
      return (
        <ProfileScreen
          requireName
          onSaved={() => setScreen("map")}
          onBack={() => undefined}
          onAccountDeleted={handleLogout}
        />
      );
    case "profile-edit":
      return (
        <ProfileScreen
          requireName={false}
          onSaved={() => setScreen("map")}
          onBack={() => setScreen("map")}
          onAccountDeleted={handleLogout}
        />
      );
    case "map":
      return <MapScreen onLogout={handleLogout} onOpenProfile={() => setScreen("profile-edit")} />;
  }
}

const styles = StyleSheet.create({
  loading: { flex: 1, alignItems: "center", justifyContent: "center", backgroundColor: "#FFF8FB" },
});
