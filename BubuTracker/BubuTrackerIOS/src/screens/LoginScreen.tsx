import React, { useState } from "react";
import { ActivityIndicator, SafeAreaView, StyleSheet, Text, TouchableOpacity, View } from "react-native";
import { getRedirectUri, login } from "../auth/auth0";
import AppDialog from "../components/AppDialog";
import { colors } from "../theme";

interface Props {
  onLoggedIn: () => void;
}

export default function LoginScreen({ onLoggedIn }: Props) {
  const [loading, setLoading] = useState(false);
  const [dialog, setDialog] = useState<string | null>(null);

  const handleLogin = async () => {
    setLoading(true);
    try {
      const result = await login();
      if (result) {
        onLoggedIn();
      }
    } catch (error) {
      setDialog(
          error instanceof Error
            ? error.message
            : "Login failed. Check your connection and try again."
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.content}>
        <View style={styles.badge}>
          <Text style={styles.badgeEmoji}>🦎</Text>
        </View>
        <Text style={styles.title}>Bubu Tracker</Text>
        <Text style={styles.subtitle}>Bubu wants to know where everyone is 🦎</Text>

        <TouchableOpacity style={styles.button} onPress={handleLogin} disabled={loading}>
          {loading ? <ActivityIndicator color="#fff" /> : <Text style={styles.buttonText}>Log in / Sign up</Text>}
        </TouchableOpacity>

        <Text style={styles.debugLabel}>Auth0 redirect URI (add to dashboard):</Text>
        <Text selectable style={styles.debugValue}>
          {getRedirectUri()}
        </Text>
      </View>

      <AppDialog
        visible={dialog !== null}
        message={dialog ?? undefined}
        buttons={[{ label: "OK" }]}
        onRequestClose={() => setDialog(null)}
      />
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.background },
  content: { flex: 1, alignItems: "center", justifyContent: "center", paddingHorizontal: 24 },
  badge: {
    width: 96,
    height: 96,
    borderRadius: 48,
    backgroundColor: colors.brandPrimary,
    alignItems: "center",
    justifyContent: "center",
    marginBottom: 20,
  },
  badgeEmoji: { fontSize: 44 },
  title: { fontSize: 30, fontWeight: "bold", color: colors.onSurface },
  subtitle: { fontSize: 15, color: colors.onSurfaceVariant, marginTop: 8, textAlign: "center" },
  button: {
    marginTop: 32,
    backgroundColor: colors.brandPrimary,
    borderRadius: 28,
    paddingVertical: 16,
    paddingHorizontal: 32,
    width: "100%",
    alignItems: "center",
  },
  buttonText: { color: colors.white, fontSize: 17, fontWeight: "600" },
  debugLabel: { marginTop: 40, fontSize: 12, color: "#8a8a8a" },
  debugValue: { fontSize: 12, color: "#3949AB", marginTop: 4, textAlign: "center" },
});
