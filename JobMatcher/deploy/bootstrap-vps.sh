#!/usr/bin/env bash
# One-time setup of a fresh Ubuntu 24.04 VPS for docs/deploy.md, run as root:
#   curl -fsSLO https://raw.githubusercontent.com/<owner>/Projects/main/JobMatcher/deploy/bootstrap-vps.sh
#   less bootstrap-vps.sh && bash bootstrap-vps.sh "ssh-ed25519 AAAA... you@laptop"
# Installs Docker from Docker's apt repository, adds a `deploy` user (docker group, your SSH
# key), enables ufw for 22/80/443, unattended upgrades, a 2 GB swap file and the backup dir.
set -euo pipefail

pubkey=${1:?usage: bootstrap-vps.sh "<ssh public key for the deploy user>"}
user=deploy
[[ $(id -u) -eq 0 ]] || { echo "run as root" >&2; exit 1; }

export DEBIAN_FRONTEND=noninteractive
apt-get update -q
apt-get install -yq ca-certificates curl git ufw unattended-upgrades rsync openssl

if ! command -v docker >/dev/null; then
    install -m 0755 -d /etc/apt/keyrings
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
    chmod a+r /etc/apt/keyrings/docker.asc
    . /etc/os-release
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu $VERSION_CODENAME stable" \
        >/etc/apt/sources.list.d/docker.list
    apt-get update -q
    apt-get install -yq docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
fi
systemctl enable --now docker

if ! id "$user" >/dev/null 2>&1; then
    useradd --create-home --shell /bin/bash "$user"
fi
usermod -aG docker "$user"
install -d -m 700 -o "$user" -g "$user" "/home/$user/.ssh"
touch "/home/$user/.ssh/authorized_keys"
grep -qxF "$pubkey" "/home/$user/.ssh/authorized_keys" || echo "$pubkey" >>"/home/$user/.ssh/authorized_keys"
chown "$user:$user" "/home/$user/.ssh/authorized_keys"
chmod 600 "/home/$user/.ssh/authorized_keys"

cat >/etc/ssh/sshd_config.d/10-jobmatcher.conf <<'SSHD'
PasswordAuthentication no
KbdInteractiveAuthentication no
PermitRootLogin prohibit-password
SSHD
systemctl reload ssh || systemctl reload sshd

# Docker publishes ports past ufw; the compose files publish only 80/443 beyond 127.0.0.1.
ufw default deny incoming
ufw default allow outgoing
ufw allow OpenSSH
ufw allow 80/tcp
ufw allow 443/tcp
ufw allow 443/udp
ufw --force enable

dpkg-reconfigure -f noninteractive unattended-upgrades

if [[ ! -f /swapfile ]]; then
    fallocate -l 2G /swapfile
    chmod 600 /swapfile
    mkswap /swapfile
    swapon /swapfile
    echo '/swapfile none swap sw 0 0' >>/etc/fstab
fi

install -d -m 750 -o "$user" -g "$user" /var/backups/jobmatcher
install -d -m 755 -o "$user" -g "$user" /opt/projects

echo "done. Next, as $user: clone the repo into /opt/projects (docs/deploy.md, First setup)."
