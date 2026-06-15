# EutherBoot

EutherBoot is a small iPXE control plane built around TOML boot profiles.

Current v0.1 scope:

- Read boot profiles from `eutherboot/profiles/*.toml`
- Generate a default iPXE menu
- Serve dynamic iPXE scripts from `/api/boot?mac=...`
- Store simple MAC-to-profile assignments
- Serve boot files from `eutherboot/www/boot`

Run locally:

```bash
EUTHERBOOT_ROOT=/home/nichlas/EutherToy/eutherboot \
EUTHERBOOT_BOOT_URL=http://192.168.1.50:8080 \
dotnet run --project src/EutherBoot --urls http://0.0.0.0:8080
```

Open `http://localhost:8080/` for the admin panel.

Open `http://localhost:8080/simulator` to test the same iPXE flow in a browser.
The simulator fetches `/api/boot?mac=...`, renders the menu, previews selected
kernel/initrd/args, and can set or clear a MAC assignment.

See [phase 1 netboot](docs/phase-1-netboot.md) for dnsmasq and iPXE wiring.
