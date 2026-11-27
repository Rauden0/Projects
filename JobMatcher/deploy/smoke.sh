#!/usr/bin/env bash
# Production-like smoke test on this machine: builds the image, generates an .env with
# deploy/gen-env.sh (DOMAIN=localhost, Caddy's internal CA), starts the prod stack as compose
# project jmprod behind Caddy on https://localhost:$SMOKE_HTTPS_PORT, checks it and tears it down.
# KEEP=1 leaves the stack running.
set -euo pipefail
cd "$(dirname "$0")/.."

export SMOKE_HTTPS_PORT=${SMOKE_HTTPS_PORT:-18443}
image=jobmatcher:smoke
work=$(mktemp -d)
export SMOKE_ENV_FILE=$work/.env
dc() {
    docker compose -p jmprod --env-file "$SMOKE_ENV_FILE" \
        -f docker-compose.yml -f docker-compose.prod.yml -f deploy/smoke.override.yml "$@"
}
cleanup() {
    if [[ -z ${KEEP:-} ]]; then
        dc down -v --remove-orphans >/dev/null 2>&1 || true
        docker image rm "$image" >/dev/null 2>&1 || true
    fi
    rm -rf "$work"
}
trap cleanup EXIT
base=https://localhost:$SMOKE_HTTPS_PORT

docker build -q -t "$image" . >/dev/null
deploy/gen-env.sh localhost "$image" "$SMOKE_ENV_FILE"

dc up -d --quiet-pull --wait --wait-timeout 600
dc ps -a

curl -fsSk "$base/healthz"; echo
curl -fsSk "$base/readyz"; echo
curl -sk -o /dev/null -w "metrics through caddy: %{http_code}\n" "$base/metrics"

cv='Senior Python developer from Brno with 6 years of experience (2019-2025) building FastAPI
and PostgreSQL services, Docker, Kubernetes, RabbitMQ and AWS. English and Czech.'
curl -fsSk -c "$work/cookies" -H 'Content-Type: application/json' \
    --data "$(python3 -c 'import json,sys; print(json.dumps({"text": sys.argv[1]}))' "$cv")" \
    "$base/cv"; echo
curl -fsSk -b "$work/cookies" "$base/jobs/search?q=python" -o /dev/null -w "search: %{http_code}\n"

sleep 20
dc ps -a --format '{{.Service}} {{.State}} {{.Status}}'
for service in api worker outbox-relay retention caddy postgres rabbitmq; do
    [[ $(dc ps --format '{{.State}}' "$service") == running ]] || { echo "$service not running" >&2; exit 1; }
done
[[ $(dc ps -a --format '{{.ExitCode}}' migrate) == 0 ]] || { echo "migrate failed" >&2; exit 1; }

start=$(date +%s)
dc stop
echo "stop took $(($(date +%s) - start))s"
echo "smoke test passed"
