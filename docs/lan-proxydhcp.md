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
- `dhcp-range=192.168.32.0,proxy,255.255.255.0` prevents dnsmasq from handing
  out IP leases.
- The normal router/DHCP server still provides client IP, DNS, gateway, and
  lease time.
- dnsmasq only adds PXE boot metadata and serves `ipxe.efi` or `ipxe.pxe`
  over TFTP.

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
sudo cp /path/to/ipxe.efi /srv/eutherboot-tftp/ipxe.efi
sudo cp /path/to/ipxe.pxe /srv/eutherboot-tftp/ipxe.pxe
```

On Arch Linux these files normally come from an `ipxe` package if available in
the enabled repositories, or from official iPXE build artifacts.

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
