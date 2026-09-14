import React from "react";
import {
  Modal,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from "react-native";
import { colors } from "../theme";

export type DialogButton = {
  label: string;
  onPress?: () => void;
  style?: "default" | "cancel" | "destructive";
};

type Props = {
  visible: boolean;
  title?: string;
  message?: string;
  prompt?: {
    placeholder?: string;
    keyboardType?: "default" | "email-address";
    value: string;
    onChangeText: (text: string) => void;
  };
  buttons: DialogButton[];
  onRequestClose: () => void;
};

export default function AppDialog({
  visible,
  title,
  message,
  prompt,
  buttons,
  onRequestClose,
}: Props) {
  return (
    <Modal visible={visible} transparent animationType="fade" onRequestClose={onRequestClose}>
      <View style={styles.backdrop}>
        <View style={styles.card}>
          {title ? <Text style={styles.title}>{title}</Text> : null}
          {message ? (
            <Text style={title ? styles.message : styles.messageOnly}>{message}</Text>
          ) : null}
          {prompt ? (
            <TextInput
              style={styles.input}
              placeholder={prompt.placeholder}
              placeholderTextColor={colors.onSurfaceVariant}
              keyboardType={prompt.keyboardType ?? "default"}
              autoCapitalize="none"
              autoCorrect={false}
              value={prompt.value}
              onChangeText={prompt.onChangeText}
            />
          ) : null}
          <View style={styles.actions}>
            {buttons.map((button) => (
              <TouchableOpacity
                key={button.label}
                style={styles.actionHit}
                onPress={() => {
                  onRequestClose();
                  button.onPress?.();
                }}
              >
                <Text
                  style={[
                    styles.actionLabel,
                    button.style === "cancel" && styles.actionCancel,
                    button.style === "destructive" && styles.actionDestructive,
                  ]}
                >
                  {button.label}
                </Text>
              </TouchableOpacity>
            ))}
          </View>
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
    paddingHorizontal: 28,
  },
  card: {
    backgroundColor: colors.dialogSurface,
    borderRadius: 20,
    paddingTop: 22,
    paddingHorizontal: 20,
    paddingBottom: 10,
  },
  title: {
    fontSize: 18,
    fontWeight: "700",
    color: colors.onSurface,
    marginBottom: 8,
  },
  message: {
    fontSize: 15,
    lineHeight: 22,
    color: colors.onSurfaceVariant,
    marginBottom: 8,
  },
  messageOnly: {
    fontSize: 16,
    lineHeight: 24,
    fontWeight: "600",
    color: colors.onSurface,
    marginBottom: 8,
  },
  input: {
    marginTop: 8,
    marginBottom: 4,
    borderBottomWidth: 1.5,
    borderBottomColor: colors.brandPrimary,
    paddingVertical: 10,
    fontSize: 16,
    color: colors.onSurface,
  },
  actions: {
    flexDirection: "row",
    justifyContent: "flex-end",
    flexWrap: "wrap",
    marginTop: 12,
  },
  actionHit: {
    paddingVertical: 12,
    paddingHorizontal: 14,
  },
  actionLabel: {
    fontSize: 15,
    fontWeight: "700",
    color: colors.brandPrimary,
  },
  actionCancel: {
    color: colors.onSurfaceVariant,
    fontWeight: "600",
  },
  actionDestructive: {
    color: colors.brandPrimaryDark,
  },
});
