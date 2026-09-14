#!/usr/bin/env bash
# Local test stack: Docker Compose (API + Postgres) + ngrok HTTPS tunnel,
# then patch BubuTracker/secrets.properties so the Android app hits the tunnel.
#
# Usage:
#   bash play/dev-ngrok.sh
#   bash play/dev-ngrok.sh --no-compose   # tunnel only (API already running)
#   bash play/dev-ngrok.sh --url-only     # print current ngrok URL (tunnel must be up)
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
API_DIR="$ROOT/BubuTrackerAPI-Go"
SECRETS="$ROOT/BubuTracker/secrets.properties"
COMPOSE_FILE="$API_DIR/deploy/docker-compose.yml"
ENV_FILE="$API_DIR/.env"
NGROK_API="http://127.0.0.1:4040/api/tunnels"
PORT=8080

DO_COMPOSE=1
URL_ONLY=0
for arg in "$@"; do
  case "$arg" in
    --no-compose) DO_COMPOSE=0 ;;
    --url-only) URL_ONLY=1; DO_COMPOSE=0 ;;
    -h|--help)
      sed -n '2,12p' "$0"
      exit 0
      ;;
    *)
      echo "Unknown arg: $arg" >&2
      exit 2
      ;;
  esac
done

need() {
  command -v "$1" >/dev/null 2>&1 || {
    echo "Missing dependency: $1" >&2
    exit 1
  }
}

fetch_ngrok_https() {
  python3 - <<'PY'
import json, sys, urllib.request
try:
    with urllib.request.urlopen("http://127.0.0.1:4040/api/tunnels", timeout=2) as r:
        data = json.load(r)
except Exception as e:
    print(f"ngrok API not reachable: {e}", file=sys.stderr)
    sys.exit(1)
for t in data.get("tunnels", []):
    url = t.get("public_url") or ""
    if url.startswith("https://"):
        print(url.rstrip("/") + "/")
        sys.exit(0)
print("No https ngrok tunnel found yet.", file=sys.stderr)
sys.exit(1)
PY
}

patch_secrets() {
  local url="$1"
  [[ -f "$SECRETS" ]] || {
    echo "Missing $SECRETS — copy secrets.properties.example first." >&2
    exit 1
  }
  python3 - "$SECRETS" "$url" <<'PY'
import pathlib, sys
path = pathlib.Path(sys.argv[1])
url = sys.argv[2]
text = path.read_text()
lines = []
seen_debug = seen_release = False
for line in text.splitlines():
    if line.startswith("apiBaseUrlDebug="):
        lines.append(f"apiBaseUrlDebug={url}")
        seen_debug = True
    elif line.startswith("apiBaseUrlRelease="):
        lines.append(f"apiBaseUrlRelease={url}")
        seen_release = True
    else:
        lines.append(line)
if not seen_debug:
    lines.append(f"apiBaseUrlDebug={url}")
if not seen_release:
    lines.append(f"apiBaseUrlRelease={url}")
path.write_text("\n".join(lines) + "\n")
print(f"Updated {path}")
print(f"  apiBaseUrlDebug={url}")
print(f"  apiBaseUrlRelease={url}")
PY
}

if [[ "$URL_ONLY" -eq 1 ]]; then
  fetch_ngrok_https
  exit 0
fi

need docker
need python3

if [[ ! -f "$ENV_FILE" ]]; then
  cp "$API_DIR/.env.example" "$ENV_FILE"
  echo "Created $ENV_FILE — set AUTH0_DOMAIN and AUTH0_AUDIENCE, then re-run." >&2
  exit 1
fi

if grep -q 'YOUR_TENANT\|YOUR_' "$ENV_FILE"; then
  echo "Fill real Auth0 values in $ENV_FILE first." >&2
  exit 1
fi

if [[ ! -f "$SECRETS" ]]; then
  cp "$ROOT/BubuTracker/secrets.properties.example" "$SECRETS"
  echo "Created $SECRETS — set Auth0 client id/domain, then re-run." >&2
  exit 1
fi

if ! command -v ngrok >/dev/null 2>&1; then
  cat <<EOF >&2
ngrok is not installed.

Install (pick one):
  # official (Linux amd64)
  curl -fsSL -o /tmp/ngrok.tgz https://bin.equinox.io/c/bNyj1mQVY4c/ngrok-v3-stable-linux-amd64.tgz
  tar -xzf /tmp/ngrok.tgz -C "\$HOME/.local/bin" ngrok

  # or: https://ngrok.com/download

Then create a free account, copy your authtoken, and run:
  ngrok config add-authtoken <YOUR_TOKEN>
EOF
  exit 1
fi

if [[ "$DO_COMPOSE" -eq 1 ]]; then
  echo "Starting Postgres + migrate + API…"
  docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d --build
  echo "Waiting for /healthz on :$PORT…"
  for i in $(seq 1 60); do
    if curl -sf "http://127.0.0.1:$PORT/healthz" >/dev/null; then
      echo "API is up."
      break
    fi
    if [[ "$i" -eq 60 ]]; then
      echo "API did not become healthy in time. Check: docker compose -f $COMPOSE_FILE logs" >&2
      exit 1
    fi
    sleep 2
  done
fi

if curl -sf "$NGROK_API" >/dev/null 2>&1; then
  echo "ngrok already running — reusing existing tunnel."
else
  echo "Starting ngrok http $PORT (leave this terminal open)…"
  # Run in background; local inspector on :4040
  ngrok http "$PORT" --log=stdout --log-format=logfmt >/tmp/bubutracker-ngrok.log 2>&1 &
  echo $! >/tmp/bubutracker-ngrok.pid
  for i in $(seq 1 30); do
    if curl -sf "$NGROK_API" >/dev/null 2>&1; then
      break
    fi
    if [[ "$i" -eq 30 ]]; then
      echo "ngrok failed to start. Log:" >&2
      cat /tmp/bubutracker-ngrok.log >&2 || true
      exit 1
    fi
    sleep 1
  done
fi

URL="$(fetch_ngrok_https)"
echo "Public URL: $URL"
patch_secrets "$URL"

cat <<EOF

Next:
  1. Keep this machine awake; leave ngrok + Docker running.
  2. Rebuild / install the app:
       cd $ROOT/BubuTracker && ./gradlew :app:installDebug
     or for a release AAB:
       cd $ROOT/BubuTracker && ./gradlew :app:bundleRelease
  3. Smoke-check from this machine:
       curl -sS ${URL}healthz
       curl -sS ${URL}readyz

Stop later:
  docker compose -f $COMPOSE_FILE down
  kill \$(cat /tmp/bubutracker-ngrok.pid) 2>/dev/null || true

Note: free ngrok URLs change when you restart ngrok — re-run this script, then rebuild the app.
EOF
