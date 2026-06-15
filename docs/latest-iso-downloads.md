# Latest ISO download notes

Checked on 2026-06-15.

## Debian Live amd64

Latest current live release found on Debian cdimage:

```text
debian-live-13.5.0-amd64-standard.iso
sha256: 1194b02ab4ab370624497287ce9b82ebac06f9251867d12d0ef3b536d3de7332
```

Resume command:

```bash
aria2c -c -x 16 -s 16 -k 1M \
  -d eutherboot/isos \
  -o debian-live-13.5.0-amd64-standard.iso \
  https://mirrors.edge.kernel.org/debian-cd/current-live/amd64/iso-hybrid/debian-live-13.5.0-amd64-standard.iso
```

## Arch Linux x86_64

Latest Arch ISO found on Arch's latest ISO index:

```text
archlinux-2026.06.01-x86_64.iso
sha256: ec7a9c89aed7a59a76266ccf723c5e88480e47d7088c4482436f882fa37c3989
```

Resume command:

```bash
aria2c -c -x 16 -s 16 -k 1M \
  -d eutherboot/isos \
  -o archlinux-2026.06.01-x86_64.iso \
  https://mirrors.edge.kernel.org/archlinux/iso/latest/archlinux-2026.06.01-x86_64.iso \
  https://mirror.rackspace.com/archlinux/iso/latest/archlinux-2026.06.01-x86_64.iso
```

## Verification

```bash
sha256sum eutherboot/isos/debian-live-13.5.0-amd64-standard.iso
sha256sum eutherboot/isos/archlinux-2026.06.01-x86_64.iso
```

If an `.aria2` file exists beside an ISO, EutherBoot marks that ISO as
incomplete and keeps it out of the generated boot menu.
