#!/usr/bin/env bash
# Pulls and starts the image named in .env, then waits until the API is ready (docs/deploy.md).
# usage: deploy/deploy.sh [TAG]   TAG (e.g. a commit sha) pins JOBMATCHER_IMAGE in .env first.
# SKIP_PULL=1 uses a locally built image (deploy/smoke.sh).
set -euo pipefail
cd "$(dirname "$0")/.."

[[ -f .env ]] || { echo ".env missing: run deploy/gen-env.sh first" >&2; exit 1; }
if [[ -n ${1:-} ]]; then
    [[ $1 =~ ^[A-Za-z0-9._-]+$ ]] || { echo "bad tag: $1" >&2; exit 1; }
    sed -i -E "s#^(JOBMATCHER_IMAGE=[^:]+(:[0-9]+/[^:]+)?)(:[^:/]*)?\$#\1:$1#" .env
fi
grep -E '^JOBMATCHER_IMAGE=' .env

profiles=()
if grep -qE '^EXTRACTION_MODE=batch' .env; then profiles=(--profile batch); fi

[[ -n ${SKIP_PULL:-} ]] || docker compose "${profiles[@]}" pull --quiet
docker compose "${profiles[@]}" up -d --quiet-pull --remove-orphans --wait --wait-timeout 300
docker compose exec -T api python -c \
    "import urllib.request; urllib.request.urlopen('http://127.0.0.1:8000/readyz', timeout=5)"
docker image prune -f >/dev/null
echo "deployed; ready"
