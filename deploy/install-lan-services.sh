#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

install -Dm644 "$repo_root/deploy/systemd/eutherboot.target" /etc/systemd/system/eutherboot.target
install -Dm644 "$repo_root/deploy/systemd/eutherboot-web.service" /etc/systemd/system/eutherboot-web.service
install -Dm644 "$repo_root/deploy/systemd/eutherboot-pxe.service" /etc/systemd/system/eutherboot-pxe.service
install -d -m 0755 /srv/eutherboot-tftp

systemctl daemon-reload

cat <<'MSG'
Installed EutherBoot systemd units.

Start:
  sudo systemctl start eutherboot.target

Stop:
  sudo systemctl stop eutherboot.target

Status:
  systemctl status eutherboot-web.service eutherboot-pxe.service
MSG

if [[ ! -f /srv/eutherboot-tftp/ipxe.efi || ! -f /srv/eutherboot-tftp/undionly.kpxe ]]; then
  cat <<'MSG'

TFTP root is ready at /srv/eutherboot-tftp, but one or both iPXE binaries are
missing:
  /srv/eutherboot-tftp/ipxe.efi
  /srv/eutherboot-tftp/undionly.kpxe
MSG
fi
