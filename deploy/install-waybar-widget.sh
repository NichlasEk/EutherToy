#!/usr/bin/env bash
set -euo pipefail

waybar_dir="${WAYBAR_DIR:-$HOME/.config/waybar}"
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
user_modules="$waybar_dir/UserModules"
config="$waybar_dir/config"
stamp="$(date +%Y%m%d-%H%M%S)"

if [[ ! -f "$user_modules" || ! -f "$config" ]]; then
  echo "Could not find Waybar UserModules/config in $waybar_dir" >&2
  exit 1
fi

cp "$user_modules" "$user_modules.eutherboot-backup-$stamp"
cp "$config" "$config.eutherboot-backup-$stamp"

python - "$repo_root" "$user_modules" "$config" <<'PY'
from pathlib import Path
import sys

repo = Path(sys.argv[1])
user_modules = Path(sys.argv[2])
config = Path(sys.argv[3])
module_snippet = (repo / "deploy/waybar/eutherboot-module.jsonc").read_text().strip()

text = user_modules.read_text()
if '"custom/eutherboot"' not in text:
    insert = "\n" + module_snippet + "\n"
    idx = text.rfind("}")
    if idx == -1:
        raise SystemExit("UserModules does not look like a Waybar JSONC object")
    before = text[:idx].rstrip()
    after = text[idx:]
    if before.endswith("{"):
        text = before + insert + after
    else:
        text = before + ",\n" + module_snippet + "\n" + after
    user_modules.write_text(text)

cfg = config.read_text()
if '"custom/eutherboot"' not in cfg:
    marker = '"tray",'
    if marker in cfg:
        cfg = cfg.replace(marker, marker + '\n\t"custom/eutherboot",', 1)
    else:
        marker = '"modules-right": ['
        if marker not in cfg:
            raise SystemExit("Could not find modules-right in Waybar config")
        cfg = cfg.replace(marker, marker + '\n\t"custom/eutherboot",', 1)
    config.write_text(cfg)
PY

if pgrep -x waybar >/dev/null 2>&1; then
  pkill -SIGUSR2 waybar || true
fi

cat <<'MSG'
Installed EutherBoot Waybar module.

If it does not appear immediately, restart Waybar:
  pkill waybar
  waybar &
MSG
