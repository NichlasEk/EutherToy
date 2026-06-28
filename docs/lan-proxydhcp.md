# LAN test with ProxyDHCP

This mode is intended for testing EutherBoot on a normal home/lab LAN without
changing the router DHCP/DNS configuration.

Host detected during setup:

- Interface: `enp6s0`
- Server IP: `192.168.32.88`
- LAN: `192.168.32.0/24`

## What dnsmasq does

`deploy/dnsmasq-eutherboot.conf` is configured as ProxyDHCP:

- `port=0` disables DNS service.
- `dhcp-no-override` keeps the boot filename in the classic BOOTP fields for
  firmware that misbehaves when dnsmasq compresses it into DHCP options.
- `dhcp-range=192.168.32.0,proxy,255.255.255.0` prevents dnsmasq from handing
  out IP leases.
- The normal router/DHCP server still provides client IP, DNS, gateway, and
  lease time.
- dnsmasq adds both PXE menu metadata and explicit `dhcp-boot` filenames so
  UEFI firmware that only partially implements ProxyDHCP still fetches
  `ipxe.efi` or `ipxe.pxe` over TFTP.

## Run the app on the LAN

```bash
EUTHERBOOT_ROOT=/home/nichlas/EutherToy/eutherboot \
EUTHERBOOT_BOOT_URL=http://192.168.32.88:8080 \
dotnet run --project src/EutherBoot --urls http://0.0.0.0:8080
```

Open:

```text
http://192.168.32.88:8080/
http://192.168.32.88:8080/simulator
http://192.168.32.88:8080/boot.ipxe
```

## TFTP files

Create a TFTP root and put iPXE binaries there:

```bash
sudo mkdir -p /srv/eutherboot-tftp
sudo /home/nichlas/EutherToy/deploy/sync-ipxe-assets.sh /srv/eutherboot-tftp
```

On Arch Linux the helper copies the packaged files from `/usr/share/ipxe/`.
If the `ipxe` package is not installed, install it first or provide equivalent
build artifacts manually.

## Start dnsmasq for the test

Use the repo config directly for a temporary foreground test:

```bash
sudo dnsmasq --keep-in-foreground --conf-file=/home/nichlas/EutherToy/deploy/dnsmasq-eutherboot.conf
```

When a PXE client starts, the flow is:

```text
firmware PXE -> TFTP ipxe.efi/ipxe.pxe -> http://192.168.32.88:8080/boot.ipxe -> /api/boot?mac=...
```

Firewall needs to allow UDP `67`, UDP `69`, UDP `4011`, and TCP `8080` from the
LAN for the full PXE path.

## Physical failure signals

When debugging a real client, watch both services at the same time:

```bash
journalctl -u eutherboot-pxe.service -f
journalctl -u eutherboot-web.service -f
```

Interpretation:

- PXE log shows `PXE(... ) ... proxy`, but the web log stays empty:
  firmware saw ProxyDHCP but never reached iPXE HTTP chain. Check Secure Boot,
  firmware PXE quality, and whether the client ever fetched `ipxe.efi`.
- PXE log shows the client and the web log shows `/boot.ipxe`:
  DHCP/TFTP succeeded and the failure moved into iPXE script or boot profile.
