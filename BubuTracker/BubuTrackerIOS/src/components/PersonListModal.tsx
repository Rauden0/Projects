import React from "react";
import { FlatList, Modal, StyleSheet, Text, TouchableOpacity, View } from "react-native";
import Avatar from "./Avatar";
import { colors } from "../theme";
import type { UserProfile } from "../types";

interface Props {
  visible: boolean;
  title: string;
  people: UserProfile[];
  emptyText: string;
  onPick?: (person: UserProfile) => void;
  onClose: () => void;
  actionLabel?: string;
  onAction?: () => void;
  /** When set, shows ✓ / ✕ instead of opening a confirm dialog. */
  onAccept?: (person: UserProfile) => void;
  onReject?: (person: UserProfile) => void;
}

function displayName(person: UserProfile): string {
  const name = [person.firstName, person.lastName].filter((part) => part.trim()).join(" ");
  return name || person.email;
}

export default function PersonListModal({
  visible,
  title,
  people,
  emptyText,
  onPick,
  onClose,
  actionLabel,
  onAction,
  onAccept,
  onReject,
}: Props) {
  const showDecide = Boolean(onAccept && onReject);

  return (
    <Modal visible={visible} animationType="slide" transparent onRequestClose={onClose}>
      <TouchableOpacity style={styles.backdrop} activeOpacity={1} onPress={onClose}>
        <View style={styles.sheet}>
          <Text style={styles.title}>{title}</Text>
          {people.length === 0 ? (
            <Text style={styles.empty}>{emptyText}</Text>
          ) : (
            <FlatList
              data={people}
              keyExtractor={(item) => item.id}
              renderItem={({ item }) => (
                <View style={styles.row}>
                  <TouchableOpacity
                    style={styles.rowMain}
                    disabled={!onPick || showDecide}
                    onPress={() => onPick?.(item)}
                  >
                    <Avatar
                      firstName={item.firstName}
                      lastName={item.lastName}
                      email={item.email}
                      color={item.markerColor}
                      size={36}
                    />
                    <Text style={styles.rowText}>{displayName(item)}</Text>
                  </TouchableOpacity>
                  {showDecide ? (
                    <View style={styles.decide}>
                      <TouchableOpacity
                        style={styles.decideHit}
                        onPress={() => onAccept?.(item)}
                        accessibilityLabel="Accept"
                      >
                        <Text style={styles.acceptMark}>✓</Text>
                      </TouchableOpacity>
                      <TouchableOpacity
                        style={styles.decideHit}
                        onPress={() => onReject?.(item)}
                        accessibilityLabel="Reject"
                      >
                        <Text style={styles.rejectMark}>✕</Text>
                      </TouchableOpacity>
                    </View>
                  ) : null}
                </View>
              )}
            />
          )}
          {actionLabel && onAction ? (
            <TouchableOpacity
              style={styles.actionButton}
              onPress={() => {
                onClose();
                onAction();
              }}
            >
              <Text style={styles.actionText}>{actionLabel}</Text>
            </TouchableOpacity>
          ) : null}
        </View>
      </TouchableOpacity>
    </Modal>
  );
}

const styles = StyleSheet.create({
  backdrop: { flex: 1, backgroundColor: "rgba(0,0,0,0.4)", justifyContent: "flex-end" },
  sheet: {
    backgroundColor: colors.surface,
    borderTopLeftRadius: 20,
    borderTopRightRadius: 20,
    padding: 20,
    maxHeight: "70%",
  },
  title: { fontSize: 18, fontWeight: "bold", color: colors.onSurface, marginBottom: 12 },
  empty: { color: colors.onSurfaceVariant, paddingVertical: 20, textAlign: "center" },
  row: { flexDirection: "row", alignItems: "center", paddingVertical: 8 },
  rowMain: { flexDirection: "row", alignItems: "center", flex: 1 },
  rowText: { marginLeft: 12, fontSize: 16, color: colors.onSurface, flexShrink: 1 },
  decide: { flexDirection: "row", alignItems: "center" },
  decideHit: { width: 40, height: 40, alignItems: "center", justifyContent: "center" },
  acceptMark: { color: "#2E7D32", fontSize: 22, fontWeight: "700" },
  rejectMark: { color: "#C62828", fontSize: 20, fontWeight: "700" },
  actionButton: {
    marginTop: 12,
    backgroundColor: colors.brandPrimary,
    borderRadius: 24,
    paddingVertical: 14,
    alignItems: "center",
  },
  actionText: { color: colors.white, fontWeight: "700", fontSize: 15 },
});
