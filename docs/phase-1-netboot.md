# EutherBoot phase 1 netboot

First target:

```text
PXE boot -> iPXE -> EutherBoot menu -> SystemRescue starts
```

## Local run

```bash
EUTHERBOOT_ROOT=/home/nichlas/EutherToy/eutherboot \
EUTHERBOOT_BOOT_URL=http://192.168.1.50:8080 \
dotnet run --project /home/nichlas/EutherToy/src/EutherBoot --urls http://0.0.0.0:8080
```

Useful endpoints:

```text
http://SERVER:8080/
http://SERVER:8080/simulator
http://SERVER:8080/generated/menu.ipxe
http://SERVER:8080/api/boot?mac=52:54:00:12:34:56
```

Use `/simulator` before testing with real PXE hardware. It calls the same boot
endpoint as iPXE, shows the generated menu, and previews the exact kernel,
initrd, and args for a selected item or assigned MAC.

The iPXE bootstrap script should chain:

```ipxe
chain http://SERVER:8080/api/boot?mac=${net0/mac}
```

## Directory layout

```text
eutherboot/
  isos/
  profiles/
  generated/
  www/
    boot/
```

Put extracted boot assets under `eutherboot/www/boot/extract/<profile>/...`.
Profile paths are HTTP paths relative to `EUTHERBOOT_BOOT_URL`.

## dnsmasq sketch

Install `dnsmasq` from the OS package manager and point it at `/srv/tftp` or another TFTP root:

```ini
port=0
interface=eth0
bind-interfaces
dhcp-range=192.168.1.100,192.168.1.200,12h
dhcp-match=set:efi-x86_64,option:client-arch,7
dhcp-match=set:efi-x86_64,option:client-arch,9
dhcp-boot=tag:efi-x86_64,ipxe.efi
dhcp-boot=undionly.kpxe
enable-tftp
tftp-root=/srv/tftp
log-dhcp
```

Place `ipxe.efi`, `undionly.kpxe`, and a chain script in the TFTP root.

## Generate a static menu

```bash
curl -X POST http://SERVER:8080/api/generate
```

This writes:

```text
eutherboot/generated/menu.ipxe
eutherboot/www/boot/menu.ipxe
```

## MAC assignment

```bash
curl -X POST "http://SERVER:8080/api/assignments?mac=52:54:00:12:34:56&profile=archlinux"
```

The next request to `/api/boot?mac=52:54:00:12:34:56` returns a one-shot boot script for that profile instead of the default menu.
