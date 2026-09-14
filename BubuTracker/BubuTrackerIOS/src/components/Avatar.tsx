import React from "react";
import { StyleSheet, Text, View } from "react-native";
import Svg, { Circle, Path } from "react-native-svg";

/** Same silhouette as Android `ic_tracked_marker` (24×24 viewport). */
const LIZARD_PATHS = [
  "M7,15.5 C4.5,16.4 1.8,15.2 1.4,12.4 C1.1,10.1 2.7,8.1 5,8.3 C6.7,8.45 7.7,10.1 6.7,11.4 C6.1,12.15 4.9,12 4.6,11.1 C4.5,10.75 4.7,10.4 5,10.35 C4.7,10.9 5.3,11.4 5.8,11 C6.3,10.6 6.1,9.7 5.3,9.5 C4,9.15 2.9,10.3 3.1,11.9 C3.35,13.85 5.3,14.9 7.3,14.2 Z",
  "M5.5,17.3 A1.5,1.15 0 1,0 8.5,17.3 A1.5,1.15 0 1,0 5.5,17.3 Z",
  "M12.1,17.5 A1.5,1.15 0 1,0 15.1,17.5 A1.5,1.15 0 1,0 12.1,17.5 Z",
  "M4.8,14 A5.4,3.7 0 1,0 15.6,14 A5.4,3.7 0 1,0 4.8,14 Z",
  "M12.5,9.2 A4.1,4.1 0 1,0 20.7,9.2 A4.1,4.1 0 1,0 12.5,9.2 Z",
] as const;

function initialsFor(firstName: string, lastName: string, email: string): string {
  const computed = [firstName.trim()[0], lastName.trim()[0]].filter(Boolean).join("").toUpperCase();
  return computed || email.trim()[0]?.toUpperCase() || "?";
}

interface Props {
  firstName: string;
  lastName: string;
  email: string;
  color: string;
  size?: number;
}

export default function Avatar({ firstName, lastName, email, color, size = 44 }: Props) {
  const initials = initialsFor(firstName, lastName, email);
  const fontSize = size * 0.3;
  // Torso center in the 24×24 viewport — same as Android createMarkerBitmap.
  const torsoX = size * (10.2 / 24);
  const torsoY = size * (14 / 24);

  return (
    <View style={{ width: size, height: size }}>
      <Svg width={size} height={size} viewBox="0 0 24 24">
        {LIZARD_PATHS.map((d) => (
          <Path key={d} d={d} fill={color} />
        ))}
        <Circle cx={18.1} cy={8.1} r={0.85} fill="#FFFFFF" />
      </Svg>
      <Text
        style={[
          styles.initials,
          {
            fontSize,
            lineHeight: fontSize * 1.15,
            width: size * 0.55,
            left: torsoX - size * 0.275,
            top: torsoY - fontSize * 0.55,
          },
        ]}
        numberOfLines={1}
      >
        {initials}
      </Text>
    </View>
  );
}

const styles = StyleSheet.create({
  initials: {
    position: "absolute",
    color: "#FFFFFF",
    fontWeight: "700",
    textAlign: "center",
    textShadowColor: "rgba(0,0,0,0.85)",
    textShadowOffset: { width: 0, height: 0 },
    textShadowRadius: 2.5,
  },
});
