#!/usr/bin/env bash
# Upload app-release.aab to the Play internal testing track via the Google Play Developer API.
# Requires: play/service-account.json with permission to release to testing tracks.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
export AAB="$ROOT/BubuTracker/app/build/outputs/bundle/release/app-release.aab"
export SA="$ROOT/play/service-account.json"
export PACKAGE="com.martinmucka.bubutracker"
export TRACK="internal"

if [[ ! -f "$SA" ]]; then
  cat <<EOF
Missing $SA

1. Play Console → Setup → API access
2. Link a Google Cloud project
3. Create a service account, grant "Release to testing tracks" (or Admin)
4. Download the JSON key to play/service-account.json
5. Re-run this script

Until then, upload manually: see play/UPLOAD_CHECKLIST.md
EOF
  exit 1
fi

if [[ ! -f "$AAB" ]]; then
  echo "Building release bundle…"
  (cd "$ROOT/BubuTracker" && ./gradlew :app:bundleRelease)
fi

python3 - <<'PY'
import os
import sys
from pathlib import Path

try:
    from google.oauth2 import service_account
    from googleapiclient.discovery import build
    from googleapiclient.http import MediaFileUpload
except ImportError:
    print(
        "Install deps: pip install google-api-python-client google-auth",
        file=sys.stderr,
    )
    raise SystemExit(2)

sa = Path(os.environ["SA"])
package = os.environ["PACKAGE"]
track = os.environ["TRACK"]
aab = Path(os.environ["AAB"])

creds = service_account.Credentials.from_service_account_file(
    str(sa),
    scopes=["https://www.googleapis.com/auth/androidpublisher"],
)
service = build("androidpublisher", "v3", credentials=creds)

edit = service.edits().insert(body={}, packageName=package).execute()
edit_id = edit["id"]
print(f"Edit {edit_id}")

media = MediaFileUpload(str(aab), mimetype="application/octet-stream", resumable=True)
bundle = (
    service.edits()
    .bundles()
    .upload(packageName=package, editId=edit_id, media_body=media)
    .execute()
)
version_code = bundle["versionCode"]
print(f"Uploaded versionCode={version_code}")

service.edits().tracks().update(
    packageName=package,
    editId=edit_id,
    track=track,
    body={
        "track": track,
        "releases": [
            {
                "name": f"{version_code}",
                "status": "completed",
                "versionCodes": [str(version_code)],
            }
        ],
    },
).execute()

service.edits().commit(packageName=package, editId=edit_id).execute()
print(f"Committed to track={track}. Add testers in Play Console if needed.")
PY
