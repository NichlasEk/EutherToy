# EutherBoot development handoff

Last updated: 2026-06-16

## Current state

EutherBoot is a small ASP.NET Minimal API control plane for LAN PXE/iPXE boot.
The current flow is:

```text
PXE firmware -> dnsmasq ProxyDHCP/TFTP -> iPXE -> /boot.ipxe -> /api/boot?mac=...
```

The app currently supports:

- Browser-based iPXE simulator at `/simulator`.
- Dynamic iPXE menu generation from TOML profiles.
- Drop-in ISO library under `eutherboot/isos`.
- Profile matching from `eutherboot/profiles/*.toml`.
- Virtual ISO file serving through `DiscUtils.Iso9660`, without extracting or
  mounting ISOs as root.
- MAC-to-profile boot assignments.
- LAN ProxyDHCP setup that avoids taking over normal DNS or DHCP.
- Systemd target and Waybar toggle helper for turning the PXE stack on/off.

Latest pushed commits of interest:

- `a1765a8 Use packaged iPXE PXE binary`
- `d26620f Add systemd and Waybar controls`
- `d56b6ad Add ProxyDHCP LAN test setup`
- `36a3658 Serve ISO contents through virtual mounts`

## Local machine state

Detected LAN setup:

- Interface: `enp6s0`
- Server IP: `192.168.32.88`
- Boot URL: `http://192.168.32.88:8080`
- ProxyDHCP subnet: `192.168.32.0/24`

Installed locally:

- Arch package: `ipxe`
- Systemd units:
  - `eutherboot.target`
  - `eutherboot-web.service`
  - `eutherboot-pxe.service`
- TFTP root: `/srv/eutherboot-tftp`
- TFTP files:
  - `/srv/eutherboot-tftp/ipxe.efi`
  - `/srv/eutherboot-tftp/ipxe.pxe`
- Waybar module installed in:
  - `~/.config/waybar/UserModules`
  - `~/.config/waybar/config`

Current service state at handoff:

```text
eutherboot.target inactive
eutherboot-web.service inactive
eutherboot-pxe.service inactive
```

PXE is therefore off until started through the Waybar widget or systemctl.

## How to start and stop

Start the full PXE stack:

```bash
pkexec systemctl start eutherboot.target
```

Stop it again:

```bash
pkexec systemctl stop eutherboot.target
```

Status:

```bash
systemctl status eutherboot-web.service eutherboot-pxe.service
```

Waybar:

- Left click toggles `eutherboot.target`.
- Right click opens `http://192.168.32.88:8080/`.
- The label should show `PXE on`, `PXE off`, or `PXE partial`.

## Quick verification

With the target running:

```bash
curl http://192.168.32.88:8080/boot.ipxe
curl http://192.168.32.88:8080/api/isos
```

Expected `/boot.ipxe`:

```ipxe
#!ipxe

set boot-url http://192.168.32.88:8080
chain ${boot-url}/api/boot?mac=${net0/mac}
```

Expected ISO readiness:

- Arch Linux ISO matched to `archlinux`.
- Debian Live ISO matched to `debian-live`.
- Kernel and initrd checks should be `true` through virtual mount paths.

## Physical PXE test recipe

1. Connect the target machine with Ethernet, not WiFi.
2. Start EutherBoot from Waybar or `pkexec systemctl start eutherboot.target`.
3. Boot the target machine into UEFI network/PXE boot.
4. It should fetch `ipxe.efi`, then `/boot.ipxe`, then show the EutherBoot menu.
5. Start with Debian Live or Arch.
6. Stop EutherBoot after testing if it should not advertise PXE on the LAN.

The current setup is ProxyDHCP only. It should not replace the router as DNS or
normal DHCP server.

## Important implementation files

- `src/EutherBoot/Program.cs`: HTTP endpoints and app wiring.
- `src/EutherBoot/MenuGenerator.cs`: iPXE script generation.
- `src/EutherBoot/VirtualIsoFileService.cs`: reads files from ISO9660 images.
- `src/EutherBoot/IsoLibrary.cs`: scans ISO library and matches profiles.
- `src/EutherBoot/BootAssetChecker.cs`: checks physical and virtual boot assets.
- `src/EutherBoot/AssignmentStore.cs`: MAC-to-profile assignment storage.
- `eutherboot/profiles/*.toml`: boot profiles.
- `deploy/dnsmasq-eutherboot.conf`: ProxyDHCP/TFTP config.
- `deploy/systemd/*`: installed service templates.
- `deploy/waybar/eutherboot-toggle.sh`: Waybar JSON/status/toggle helper.

## Known limitations

- Windows boot is intentionally not implemented yet.
- ISO profile support is still manual TOML, not auto-discovered from upstream
  metadata.
- No in-app upload UI is committed yet, only filesystem drop-in behavior.
- The app uses `dotnet run` in the systemd service for development convenience.
  A later production pass should publish the app and run the built DLL.
- No firewall automation is included. The LAN path needs UDP `67`, UDP `69`, UDP
  `4011`, and TCP `8080` allowed.
- The Waybar helper uses `pkexec systemctl ...`, so the first toggle can ask for
  polkit authentication.

## Suggested next steps

1. Do the first physical PXE boot with an Ethernet-connected UEFI client.
2. If firmware reaches iPXE but menu fails, inspect:
   - `journalctl -u eutherboot-pxe.service -f`
   - `journalctl -u eutherboot-web.service -f`
3. Replace `dotnet run` service with `dotnet publish` output.
4. Add a real ISO upload/drop-watch UI in the admin panel.
5. Add a "refresh latest ISO" helper flow for Debian Live and Arch.
6. Add better profile validation in the web UI.
7. Add optional one-shot MAC assignment clearing after a successful boot request.
8. Later: add autoinstall profile metadata for Debian preseed, Ubuntu autoinstall,
   Arch scripts, NixOS configs, and similar workflows.

## Related docs

- `docs/lan-proxydhcp.md`
- `docs/hyprland-waybar-service.md`
- `docs/mount-on-demand.md`
- `docs/iso-library.md`
- `docs/latest-iso-downloads.md`
- `docs/phase-1-netboot.md`
