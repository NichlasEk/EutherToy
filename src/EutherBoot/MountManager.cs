using System.Diagnostics;

namespace EutherBoot;

public sealed class MountManager
{
    private readonly IsoLibrary _isoLibrary;
    private readonly BootAssetChecker _assetChecker;
    private readonly string _mountRoot;
    private readonly string? _mountHelper;

    public MountManager(IsoLibrary isoLibrary, BootAssetChecker assetChecker, string mountRoot, string? mountHelper)
    {
        _isoLibrary = isoLibrary;
        _assetChecker = assetChecker;
        _mountRoot = mountRoot;
        _mountHelper = string.IsNullOrWhiteSpace(mountHelper) ? null : mountHelper;
    }

    public MountResult EnsureMounted(BootProfile profile, IReadOnlyList<BootProfile> profiles)
    {
        var currentAssets = _assetChecker.Check(profile, profiles);
        if (currentAssets.Ready)
            return new MountResult(true, "Ready through virtual ISO mount.", null, GetMountPath(profile));

        var iso = _isoLibrary.FindIsoForProfile(profile, profiles);
        if (iso is null)
            return new MountResult(false, $"No complete ISO matches profile '{profile.Name}'.", null, GetMountPath(profile));

        var isoPath = _isoLibrary.GetIsoPath(iso.FileName);
        var mountPath = GetMountPath(profile);

        if (_mountHelper is null)
            return new MountResult(false, "No mount helper configured. Set EUTHERBOOT_MOUNT_HELPER.", isoPath, mountPath);

        Directory.CreateDirectory(mountPath);

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = _mountHelper,
            ArgumentList = { profile.Name, isoPath, mountPath },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        });

        if (process is null)
            return new MountResult(false, $"Could not start mount helper '{_mountHelper}'.", isoPath, mountPath);

        process.WaitForExit(15_000);
        if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
            return new MountResult(false, "Mount helper timed out.", isoPath, mountPath);
        }

        if (process.ExitCode != 0)
        {
            var error = process.StandardError.ReadToEnd().Trim();
            return new MountResult(false, error.Length == 0 ? $"Mount helper exited {process.ExitCode}." : error, isoPath, mountPath);
        }

        var assets = _assetChecker.Check(profile, profiles);
        return assets.Ready
            ? new MountResult(true, "Mounted.", isoPath, mountPath)
            : new MountResult(false, "ISO mounted, but profile kernel/initrd paths are still missing.", isoPath, mountPath);
    }

    private string GetMountPath(BootProfile profile)
        => Path.Combine(_mountRoot, profile.Name);
}
