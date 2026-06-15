#!/usr/bin/env bash
set -euo pipefail

if [[ $# -ne 3 ]]; then
  echo "usage: eutherboot-mount-iso.sh <profile> <iso-path> <mount-path>" >&2
  exit 64
fi

profile="$1"
iso_path="$2"
mount_path="$3"

case "$profile" in
  archlinux|debian-live|debian|systemrescue) ;;
  *)
    echo "refusing unknown profile: $profile" >&2
    exit 65
    ;;
esac

if [[ ! -f "$iso_path" ]]; then
  echo "ISO does not exist: $iso_path" >&2
  exit 66
fi

mkdir -p "$mount_path"

if mountpoint -q "$mount_path"; then
  exit 0
fi

exec /usr/bin/mount -o loop,ro "$iso_path" "$mount_path"
