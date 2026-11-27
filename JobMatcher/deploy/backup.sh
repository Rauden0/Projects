#!/usr/bin/env bash
# Nightly pg_dump of the jobmatcher database, keeping BACKUP_KEEP_DAYS (deploy/jobmatcher-backup.*).
# BACKUP_RSYNC_TARGET (e.g. u123@u123.your-storagebox.de:jobmatcher) copies the dumps off the host.
set -euo pipefail
cd "$(dirname "$0")/.."

dir=${BACKUP_DIR:-/var/backups/jobmatcher}
keep=${BACKUP_KEEP_DAYS:-7}
mkdir -p "$dir"
umask 077
file=$dir/jobmatcher-$(date -u +%Y%m%dT%H%M%SZ).dump

docker compose exec -T postgres pg_dump -U jobmatcher -Fc jobmatcher >"$file.partial"
docker compose exec -T postgres pg_restore --list <"$file.partial" >/dev/null
mv "$file.partial" "$file"
find "$dir" -name 'jobmatcher-*.dump' -mtime "+$keep" -delete
find "$dir" -name 'jobmatcher-*.dump.partial' -mmin +60 -delete

if [[ -n ${BACKUP_RSYNC_TARGET:-} ]]; then
    rsync -a --delete -e "ssh -o BatchMode=yes" "$dir/" "$BACKUP_RSYNC_TARGET/"
fi
echo "backup: $file ($(du -h "$file" | cut -f1))"
