# Mount-on-demand boot model

EutherBoot does not extract ISO contents by default. The intended production
model is:

```text
iPXE menu item -> /api/boot/profile/<name> -> server loop-mounts ISO -> iPXE loads kernel/initrd over HTTP
```

iPXE still needs kernel and initrd as normal HTTP files. The ISO mount happens
on the server, read-only, under:

```text
eutherboot/www/boot/mounts/<profile>
```

## Helper

The app stays unprivileged. Mounting is delegated to a helper configured with:

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

If the helper is not configured, profile selection returns an iPXE error script
and drops to shell instead of pretending the boot is ready.
