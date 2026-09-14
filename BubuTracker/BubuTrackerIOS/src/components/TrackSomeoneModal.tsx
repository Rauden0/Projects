import React, { useEffect, useState } from "react";
import {
  ActivityIndicator,
  FlatList,
  Modal,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from "react-native";
import { api } from "../api/client";
import { colors } from "../theme";
import type { UserProfile } from "../types";
import { actionErrorMessage, apiErrorMessage } from "../utils/errors";
import Avatar from "./Avatar";

type Props = {
  visible: boolean;
  onClose: () => void;
  onError: (message: string) => void;
  onPickPending: (person: UserProfile) => void;
};

function displayName(person: UserProfile): string {
  const name = [person.firstName, person.lastName].filter((part) => part.trim()).join(" ");
  return name || person.email;
}

export default function TrackSomeoneModal({ visible, onClose, onError, onPickPending }: Props) {
  const [email, setEmail] = useState("");
  const [pending, setPending] = useState<UserProfile[]>([]);
  const [loadingPending, setLoadingPending] = useState(false);
  const [sending, setSending] = useState(false);

  const reloadPending = () => {
    setLoadingPending(true);
    api
      .getOutgoingRequests()
      .then(setPending)
      .catch((error) => onError(actionErrorMessage(error)))
      .finally(() => setLoadingPending(false));
  };

  useEffect(() => {
    if (!visible) {
      return;
    }
    setEmail("");
    reloadPending();
  }, [visible]);

  const submit = async () => {
    const trimmed = email.trim();
    if (!trimmed || sending) return;
    setSending(true);
    try {
      await api.addTracking(trimmed);
      setEmail("");
      reloadPending();
    } catch (error) {
      onError(apiErrorMessage(error, "Could not send the request. Please try again."));
    } finally {
      setSending(false);
    }
  };

  return (
    <Modal visible={visible} animationType="fade" transparent onRequestClose={onClose}>
      <View style={styles.backdrop}>
        <View style={styles.card}>
          <Text style={styles.title}>Track someone</Text>
          <Text style={styles.hint}>Enter their email — you can send several requests.</Text>
          <TextInput
            style={styles.input}
            placeholder="Email address"
            placeholderTextColor={colors.onSurfaceVariant}
            keyboardType="email-address"
            autoCapitalize="none"
            autoCorrect={false}
            value={email}
            onChangeText={setEmail}
            onSubmitEditing={submit}
            returnKeyType="send"
          />
          <TouchableOpacity style={styles.addButton} onPress={submit} disabled={sending}>
            {sending ? (
              <ActivityIndicator color={colors.white} />
            ) : (
              <Text style={styles.addButtonText}>Add</Text>
            )}
          </TouchableOpacity>

          <Text style={styles.sectionTitle}>Pending requests</Text>
          {loadingPending ? (
            <ActivityIndicator color={colors.brandPrimary} style={styles.loader} />
          ) : pending.length === 0 ? (
            <Text style={styles.empty}>No pending requests yet.</Text>
          ) : (
            <FlatList
              data={pending}
              keyExtractor={(item) => item.id}
              style={styles.list}
              renderItem={({ item }) => (
                <TouchableOpacity
                  style={styles.row}
                  onPress={() => {
                    onClose();
                    onPickPending(item);
                  }}
                >
                  <Avatar
                    firstName={item.firstName}
                    lastName={item.lastName}
                    email={item.email}
                    color={item.markerColor}
                    size={36}
                  />
                  <View style={styles.rowTextWrap}>
                    <Text style={styles.rowText}>{displayName(item)}</Text>
                  </View>
                </TouchableOpacity>
              )}
            />
          )}

          <TouchableOpacity style={styles.closeHit} onPress={onClose}>
            <Text style={styles.closeText}>Close</Text>
          </TouchableOpacity>
        </View>
      </View>
    </Modal>
  );
}

const styles = StyleSheet.create({
  backdrop: {
    flex: 1,
    backgroundColor: "rgba(28, 27, 31, 0.45)",
    justifyContent: "center",
    paddingHorizontal: 24,
  },
  card: {
    backgroundColor: colors.dialogSurface,
    borderRadius: 20,
    paddingTop: 22,
    paddingHorizontal: 20,
    paddingBottom: 12,
    maxHeight: "80%",
  },
  title: { fontSize: 18, fontWeight: "700", color: colors.onSurface },
  hint: { fontSize: 14, color: colors.onSurfaceVariant, marginTop: 6, marginBottom: 12 },
  input: {
    borderBottomWidth: 1.5,
    borderBottomColor: colors.brandPrimary,
    paddingVertical: 10,
    fontSize: 16,
    color: colors.onSurface,
  },
  addButton: {
    marginTop: 14,
    backgroundColor: colors.brandPrimary,
    borderRadius: 22,
    paddingVertical: 12,
    alignItems: "center",
  },
  addButtonText: { color: colors.white, fontWeight: "700", fontSize: 15 },
  sectionTitle: {
    marginTop: 20,
    fontSize: 14,
    fontWeight: "700",
    color: colors.onSurface,
  },
  loader: { marginTop: 16 },
  empty: { marginTop: 10, color: colors.onSurfaceVariant, fontSize: 14 },
  list: { marginTop: 6, maxHeight: 220 },
  row: { flexDirection: "row", alignItems: "center", paddingVertical: 10 },
  rowTextWrap: { marginLeft: 12, flex: 1 },
  rowText: { fontSize: 16, color: colors.onSurface, flex: 1 },
  closeHit: { alignSelf: "flex-end", paddingVertical: 12, paddingHorizontal: 8 },
  closeText: { color: colors.onSurfaceVariant, fontWeight: "600", fontSize: 15 },
});
