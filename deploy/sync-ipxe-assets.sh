#!/usr/bin/env bash
set -euo pipefail

tftp_root="${1:-/srv/eutherboot-tftp}"

declare -A assets=(
  ["ipxe.pxe"]="/usr/share/ipxe/ipxe.pxe"
  ["ipxe.efi"]="/usr/share/ipxe/x86_64/ipxe.efi"
)

missing=0
install -d -m 0755 "$tftp_root"

for target in "${!assets[@]}"; do
  source_path="${assets[$target]}"
  if [[ ! -f "$source_path" ]]; then
    echo "Missing iPXE asset: $source_path" >&2
    missing=1
    continue
  fi

  install -m 0644 "$source_path" "$tftp_root/$target"
done

if (( missing != 0 )); then
  cat >&2 <<'MSG'

Install the Arch `ipxe` package or provide equivalent files before starting the
PXE stack again.
MSG
  exit 1
fi

cat <<MSG
Synced iPXE assets into $tftp_root:
  $tftp_root/ipxe.pxe
  $tftp_root/ipxe.efi
MSG
