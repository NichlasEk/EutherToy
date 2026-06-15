# ISO library workflow

EutherBoot treats `eutherboot/isos` as the source of truth for available
operating systems.

## Drop-in flow

1. Put one or more `*.iso` files in `eutherboot/isos`.
2. Refresh `http://SERVER:8080/`.
3. EutherBoot matches filenames against `profiles/*.toml`.
4. `/api/boot` and `/generated/menu.ipxe` rebuild from the matched profiles.
5. Open `/simulator` and check that kernel/initrd assets exist.

When `eutherboot/isos` is empty, EutherBoot keeps all profiles visible as a lab
mode. Once ISO files exist, the menu is filtered to profiles that match an ISO.

## Asset readiness

The ISO file itself is the library record. For iPXE boot, the kernel and initrd
paths in the matched profile must be available as HTTP paths. By default
EutherBoot serves them virtually from the ISO under:

```text
/mounts/<profile>/...
```

Example:

```text
profile path: /mounts/archlinux/arch/boot/x86_64/vmlinuz-linux
real source:  eutherboot/isos/archlinux-2026.06.01-x86_64.iso
```

The admin page and simulator both show whether those files exist.

## Asking Codex to fetch the latest ISO

Use Codex as the update helper instead of hardcoding moving download URLs in the
app. Ask something like:

```text
du, jag tänkte installera nytt OS. Hämta senaste SystemRescue ISO från officiell källa till eutherboot/isos och verifiera checksum om den publiceras.
```

Codex should:

1. Check the current official download page or release feed.
2. Download the latest ISO into `eutherboot/isos`.
3. Prefer checksum verification when the distro publishes checksums.
4. Leave the filename intact so profile matching keeps working.
5. Refresh `/api/isos` or `/simulator` to confirm the profile match.

This keeps the app simple and auditable while still making latest-ISO updates a
one-command assistant workflow.
