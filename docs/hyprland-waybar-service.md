# Hyprland Waybar service control

EutherBoot can be controlled as one systemd target:

```bash
sudo systemctl start eutherboot.target
sudo systemctl stop eutherboot.target
```

The target starts and stops:

- `eutherboot-web.service`: ASP.NET control panel on `http://192.168.32.88:8080/`
- `eutherboot-pxe.service`: dnsmasq ProxyDHCP/TFTP using
  `deploy/dnsmasq-eutherboot.conf`

Install the units:

```bash
sudo /home/nichlas/EutherToy/deploy/install-lan-services.sh
```

The Waybar helper lives at:

```text
/home/nichlas/EutherToy/deploy/waybar/eutherboot-toggle.sh
```

Add the module from `deploy/waybar/eutherboot-module.jsonc` to
`~/.config/waybar/UserModules`, then add `custom/eutherboot` to one of the
Waybar module lists, for example `modules-right`.

Or install it automatically:

```bash
/home/nichlas/EutherToy/deploy/install-waybar-widget.sh
```

The installer backs up `~/.config/waybar/UserModules` and
`~/.config/waybar/config` before editing them.

Click behavior:

- Left click toggles the full EutherBoot PXE stack through `pkexec systemctl`.
- Right click opens the admin panel.
- Text shows `PXE on`, `PXE off`, or `PXE partial`.

The first toggle may ask for polkit authentication. That is intentional: starting
PXE requires a root-owned dnsmasq service for DHCP/TFTP ports.
