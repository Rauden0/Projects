#!/usr/bin/env bash
# Writes a production .env from .env.example with fresh secrets (docs/deploy.md).
# usage: deploy/gen-env.sh DOMAIN IMAGE [OUTPUT]   e.g. jobs.example.com ghcr.io/me/jobmatcher:latest
set -euo pipefail

domain=${1:?usage: gen-env.sh DOMAIN IMAGE [OUTPUT]}
image=${2:?usage: gen-env.sh DOMAIN IMAGE [OUTPUT]}
root=$(cd "$(dirname "$0")/.." && pwd)
out=${3:-$root/.env}

if [[ -e $out ]]; then
    echo "$out exists; refusing to overwrite its secrets" >&2
    exit 1
fi

secret() { openssl rand -hex "$1"; }

pg_pass=$(secret 24)
rmq_user=jobmatcher
rmq_pass=$(secret 24)

declare -A values=(
    [COMPOSE_PROJECT_NAME]=jobmatcher
    [COMPOSE_FILE]=docker-compose.yml:docker-compose.prod.yml
    [DOMAIN]=$domain
    [JOBMATCHER_IMAGE]=$image
    [POSTGRES_PASSWORD]=$pg_pass
    [RABBITMQ_DEFAULT_USER]=$rmq_user
    [RABBITMQ_DEFAULT_PASS]=$rmq_pass
    [DATABASE_URL]=postgresql+asyncpg://jobmatcher:$pg_pass@postgres:5432/jobmatcher
    [AMQP_URL]=amqp://$rmq_user:$rmq_pass@rabbitmq:5672/
    [AUTH_MODE]=session
    [SESSION_SECRET]=$(secret 32)
    [AUTH_INSECURE_DEV]=false
    [SESSION_COOKIE_SECURE]=true
    [LLM_PROVIDER]=openai_compat
    [LLM_BASE_URL]=https://api.groq.com/openai/v1
    [LLM_MODEL]=openai/gpt-oss-120b
    [LLM_REASONING_EFFORT]=low
    [LLM_TIMEOUT_S]=60
    [LLM_MONTHLY_BUDGET_USD]=5
    [LOG_FORMAT]=json
)

umask 077
tmp=$(mktemp "$out.XXXXXX")
trap 'rm -f "$tmp"' EXIT
cp "$root/.env.example" "$tmp"
{
    echo
    echo "# Production values written by deploy/gen-env.sh; they override the defaults above."
    for key in COMPOSE_PROJECT_NAME COMPOSE_FILE DOMAIN JOBMATCHER_IMAGE POSTGRES_PASSWORD \
        RABBITMQ_DEFAULT_USER RABBITMQ_DEFAULT_PASS DATABASE_URL AMQP_URL AUTH_MODE \
        SESSION_SECRET AUTH_INSECURE_DEV SESSION_COOKIE_SECURE LLM_PROVIDER LLM_BASE_URL \
        LLM_MODEL LLM_REASONING_EFFORT LLM_TIMEOUT_S LLM_MONTHLY_BUDGET_USD LOG_FORMAT; do
        echo "$key=${values[$key]}"
    done
    echo "# Fill in: a Groq key (console.groq.com), or switch to Anthropic as in docs/deploy.md."
    echo "LLM_API_KEY="
} >>"$tmp"
# Later lines win in compose's .env parser, but keep a single definition per key anyway.
awk -F= '
    /^[A-Z_]+=/ { last[$1] = NR }
    { line[NR] = $0; key[NR] = ($0 ~ /^[A-Z_]+=/) ? $1 : "" }
    END { for (i = 1; i <= NR; i++) if (key[i] == "" || last[key[i]] == i) print line[i] }
' "$tmp" >"$tmp.dedup"
mv "$tmp.dedup" "$tmp"
chmod 600 "$tmp"
mv "$tmp" "$out"
trap - EXIT
echo "wrote $out (mode 600); set LLM_API_KEY before the first deploy"
