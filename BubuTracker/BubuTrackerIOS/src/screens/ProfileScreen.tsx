import React, { useEffect, useState } from "react";
import {
  ActivityIndicator,
  BackHandler,
  SafeAreaView,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from "react-native";
import { api } from "../api/client";
import AppDialog, { type DialogButton } from "../components/AppDialog";
import { MARKER_COLORS } from "../config";
import { getFreshProfile, setProfile } from "../state/profileStore";
import { colors } from "../theme";
import type { UserProfile } from "../types";
import { apiErrorMessage } from "../utils/errors";

interface Props {
  requireName: boolean;
  onSaved: () => void;
  onBack: () => void;
  onAccountDeleted: () => void;
}

type DialogState = {
  title?: string;
  message?: string;
  buttons: DialogButton[];
};

export default function ProfileScreen({ requireName, onSaved, onBack, onAccountDeleted }: Props) {
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [markerColor, setMarkerColor] = useState<string | null>(null);
  const [dialog, setDialog] = useState<DialogState | null>(null);

  const showMessage = (message: string) => setDialog({ message, buttons: [{ label: "OK" }] });

  useEffect(() => {
    const applyProfile = (profile: UserProfile) => {
      setFirstName(profile.firstName);
      setLastName(profile.lastName);
      setEmail(profile.email);
      setMarkerColor(profile.markerColor);
    };

    const cachedProfile = getFreshProfile();
    if (cachedProfile) {
      applyProfile(cachedProfile);
      setLoading(false);
      return;
    }

    api
      .getMe()
      .then((profile: UserProfile) => {
        setProfile(profile);
        applyProfile(profile);
      })
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => {
    if (!requireName) {
      return;
    }
    const subscription = BackHandler.addEventListener("hardwareBackPress", () => {
      showMessage("Please add your name to continue.");
      return true;
    });
    return () => subscription.remove();
  }, [requireName]);

  const handleSave = async () => {
    const trimmedFirst = firstName.trim();
    const trimmedLast = lastName.trim();
    if (!trimmedFirst || !trimmedLast) {
      showMessage("First and last name are required.");
      return;
    }
    setSaving(true);
    try {
      const updated = await api.updateMe({
        firstName: trimmedFirst,
        lastName: trimmedLast,
        markerColor: markerColor ?? undefined,
      });
      setProfile(updated);
      onSaved();
    } catch (error) {
      showMessage(apiErrorMessage(error, "Profile update failed. Please try again."));
    } finally {
      setSaving(false);
    }
  };

  const confirmDeleteAccount = () => {
    setDialog({
      title: "Delete your account?",
      message: "This permanently removes your profile, location, and every tracking relationship. This cannot be undone.",
      buttons: [
        { label: "Cancel", style: "cancel" },
        { label: "Delete", style: "destructive", onPress: handleDeleteAccount },
      ],
    });
  };

  const handleDeleteAccount = async () => {
    setDeleting(true);
    try {
      await api.deleteMe();
      onAccountDeleted();
    } catch (error) {
      showMessage(apiErrorMessage(error, "Couldn't delete your account. Please try again."));
      setDeleting(false);
    }
  };

  if (loading) {
    return (
      <SafeAreaView style={styles.loadingContainer}>
        <ActivityIndicator color={colors.brandPrimary} />
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      {!requireName && (
        <TouchableOpacity onPress={onBack} style={styles.backButton}>
          <Text style={styles.backText}>← Back</Text>
        </TouchableOpacity>
      )}
      <Text style={styles.title}>{requireName ? "Complete your profile" : "Edit profile"}</Text>
      <Text style={styles.email}>{email}</Text>

      <Text style={styles.label}>First name</Text>
      <TextInput style={styles.input} value={firstName} onChangeText={setFirstName} autoCapitalize="words" />

      <Text style={styles.label}>Last name</Text>
      <TextInput style={styles.input} value={lastName} onChangeText={setLastName} autoCapitalize="words" />

      <Text style={styles.label}>Marker color</Text>
      <View style={styles.colors}>
        {MARKER_COLORS.map((color) => (
          <TouchableOpacity
            key={color}
            style={[
              styles.swatch,
              { backgroundColor: color },
              markerColor === color && styles.swatchSelected,
            ]}
            onPress={() => setMarkerColor(color)}
          />
        ))}
      </View>

      <TouchableOpacity style={styles.saveButton} onPress={handleSave} disabled={saving}>
        {saving ? <ActivityIndicator color="#fff" /> : <Text style={styles.saveText}>Save</Text>}
      </TouchableOpacity>

      {!requireName && (
        <TouchableOpacity
          style={styles.deleteAccountButton}
          onPress={confirmDeleteAccount}
          disabled={deleting}
        >
          {deleting ? (
            <ActivityIndicator color={colors.brandPrimaryDark} />
          ) : (
            <Text style={styles.deleteAccountText}>Delete my account</Text>
          )}
        </TouchableOpacity>
      )}

      <AppDialog
        visible={dialog !== null}
        title={dialog?.title}
        message={dialog?.message}
        buttons={dialog?.buttons ?? []}
        onRequestClose={() => setDialog(null)}
      />
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.background, padding: 24 },
  loadingContainer: { flex: 1, backgroundColor: colors.background, alignItems: "center", justifyContent: "center" },
  backButton: { marginBottom: 12 },
  backText: { color: colors.brandPrimary, fontSize: 16, fontWeight: "600" },
  title: { fontSize: 24, fontWeight: "bold", color: colors.onSurface, marginBottom: 4 },
  email: { color: colors.onSurfaceVariant, marginBottom: 24 },
  label: { fontSize: 13, color: colors.onSurfaceVariant, marginBottom: 6 },
  input: {
    backgroundColor: colors.surface,
    borderRadius: 12,
    borderWidth: 1,
    borderColor: colors.outlineVariant,
    paddingHorizontal: 14,
    paddingVertical: 12,
    fontSize: 16,
    color: colors.onSurface,
    marginBottom: 16,
  },
  colors: { flexDirection: "row", flexWrap: "wrap", marginBottom: 28 },
  swatch: {
    width: 36,
    height: 36,
    borderRadius: 18,
    marginRight: 10,
    marginBottom: 10,
  },
  swatchSelected: {
    borderWidth: 3,
    borderColor: colors.onSurface,
  },
  saveButton: {
    backgroundColor: colors.brandPrimary,
    borderRadius: 28,
    paddingVertical: 16,
    alignItems: "center",
  },
  saveText: { color: colors.white, fontSize: 17, fontWeight: "600" },
  deleteAccountButton: { alignItems: "center", padding: 12, marginTop: 8 },
  deleteAccountText: { color: colors.brandPrimaryDark, fontSize: 14, fontWeight: "600" },
});
