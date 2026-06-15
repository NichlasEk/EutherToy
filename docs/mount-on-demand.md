# Virtual mount boot model

EutherBoot does not extract ISO contents by default. The intended model is:

```text
iPXE menu item -> /api/boot/profile/<name> -> EutherBoot reads ISO -> iPXE loads kernel/initrd over HTTP
```

iPXE still needs kernel and initrd as normal HTTP files. EutherBoot exposes ISO
contents through virtual HTTP paths:

```text
http://SERVER:8080/mounts/<profile>/...
```

This is not a Linux kernel mount. The web app reads the ISO9660 filesystem
directly and streams files with HTTP range support, so the app can stay
unprivileged.

## Optional helper

The loop-mount helper is still available as a fallback for images the virtual
reader cannot handle:

```bash
EUTHERBOOT_MOUNT_HELPER=/home/nichlas/EutherToy/deploy/eutherboot-mount-iso.sh
```

The helper receives:

```text
<profile> <iso-path> <mount-path>
```

For a real service, allow only this exact helper through sudoers or systemd, not
the whole web process.

## Current profile paths

Arch:

```text
/mounts/archlinux/arch/boot/x86_64/vmlinuz-linux
/mounts/archlinux/arch/boot/x86_64/initramfs-linux.img
```

Debian Live:

```text
/mounts/debian-live/live/vmlinuz
/mounts/debian-live/live/initrd.img
/mounts/debian-live/live/filesystem.squashfs
```

If the virtual reader cannot serve the requested files and the helper is not
configured, profile selection returns an iPXE error script and drops to shell.
