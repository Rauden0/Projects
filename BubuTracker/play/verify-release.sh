#!/usr/bin/env bash
# Validates the release AAB locally and prints install instructions.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
APP="$ROOT/BubuTracker"
AAB="$APP/app/build/outputs/bundle/release/app-release.aab"
OUT_DIR="$ROOT/play/out"
APKS="$OUT_DIR/app-release.apks"
BT_JAR="$OUT_DIR/bundletool.jar"
SDK="${ANDROID_HOME:-$HOME/Android/Sdk}"

mkdir -p "$OUT_DIR"

if [[ ! -f "$AAB" ]]; then
  echo "Missing AAB. Building…"
  (cd "$APP" && ./gradlew :app:bundleRelease)
fi

echo "AAB: $AAB"
ls -lh "$AAB"

if [[ ! -f "$BT_JAR" ]]; then
  echo "Downloading bundletool…"
  curl -fsSL -o "$BT_JAR" \
    https://github.com/google/bundletool/releases/download/1.17.2/bundletool-all-1.17.2.jar
fi

java -jar "$BT_JAR" validate --bundle="$AAB"

KS="$APP/upload-keystore.jks"
STORE_PASS=$(grep '^storePassword=' "$APP/keystore.properties" | cut -d= -f2-)
KEY_PASS=$(grep '^keyPassword=' "$APP/keystore.properties" | cut -d= -f2-)
KEY_ALIAS=$(grep '^keyAlias=' "$APP/keystore.properties" | cut -d= -f2-)

rm -f "$APKS"
java -jar "$BT_JAR" build-apks \
  --bundle="$AAB" \
  --output="$APKS" \
  --mode=universal \
  --ks="$KS" \
  --ks-pass="pass:$STORE_PASS" \
  --ks-key-alias="$KEY_ALIAS" \
  --key-pass="pass:$KEY_PASS"

cd "$OUT_DIR"
unzip -o app-release.apks universal.apk >/dev/null
if [[ -x "$SDK/build-tools/34.0.0/apksigner" ]]; then
  "$SDK/build-tools/34.0.0/apksigner" verify --verbose universal.apk
  "$SDK/build-tools/34.0.0/aapt" dump badging universal.apk | head -5
fi

ADB="$SDK/platform-tools/adb"
if [[ -x "$ADB" ]] && "$ADB" get-state >/dev/null 2>&1; then
  echo "Device connected — installing universal.apk…"
  "$ADB" install -r "$OUT_DIR/universal.apk"
  echo "Launch:"
  echo "  $ADB shell am start -n com.martinmucka.bubutracker/com.example.bubutracker.feature.auth.LoginActivity"
else
  echo "No adb device. Sideload later:"
  echo "  $ADB install -r $OUT_DIR/universal.apk"
fi

echo
echo "See play/E2E_CHECKLIST.md for Auth0 / Maps / API checks on a real device."
