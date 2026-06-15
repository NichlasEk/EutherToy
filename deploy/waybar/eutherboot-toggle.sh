#!/usr/bin/env bash
set -euo pipefail

target="eutherboot.target"
web="eutherboot-web.service"
pxe="eutherboot-pxe.service"
url="http://192.168.32.88:8080/"

is_active() {
  systemctl is-active --quiet "$1"
}

status_json() {
  local web_state pxe_state text class tooltip

  web_state="$(systemctl is-active "$web" 2>/dev/null || true)"
  pxe_state="$(systemctl is-active "$pxe" 2>/dev/null || true)"
  [[ -n "$web_state" ]] || web_state="unknown"
  [[ -n "$pxe_state" ]] || pxe_state="unknown"

  if [[ "$web_state" == "active" && "$pxe_state" == "active" ]]; then
    text="PXE on"
    class="on"
  elif [[ "$web_state" == "active" || "$pxe_state" == "active" ]]; then
    text="PXE partial"
    class="partial"
  else
    text="PXE off"
    class="off"
  fi

  tooltip="EutherBoot\nweb: $web_state\npxe: $pxe_state\nleft click: toggle\nright click: open panel\n$url"
  printf '{"text":"%s","class":"%s","tooltip":"%s"}\n' "$text" "$class" "${tooltip//$'\n'/\\n}"
}

toggle() {
  if is_active "$target" || is_active "$web" || is_active "$pxe"; then
    pkexec systemctl stop "$target"
  else
    pkexec systemctl start "$target"
  fi
}

case "${1:-status}" in
  status)
    status_json
    ;;
  toggle)
    toggle
    status_json
    ;;
  open)
    xdg-open "$url" >/dev/null 2>&1 &
    ;;
  *)
    echo "usage: $0 [status|toggle|open]" >&2
    exit 2
    ;;
esac
