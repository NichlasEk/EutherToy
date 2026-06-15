namespace EutherBoot;

public sealed class BootAssetChecker
{
    private readonly string _staticRoot;
    private readonly VirtualIsoFileService? _virtualIsoFiles;

    public BootAssetChecker(string staticRoot, VirtualIsoFileService? virtualIsoFiles = null)
    {
        _staticRoot = staticRoot;
        _virtualIsoFiles = virtualIsoFiles;
    }

    public BootAssetStatus Check(BootProfile profile, IReadOnlyList<BootProfile>? profiles = null)
        => Check(profile.Boot.Kernel, profile.Boot.Initrd, profile, profiles);

    public BootAssetStatus Check(string kernelPath, string initrdPath, BootProfile? profile = null, IReadOnlyList<BootProfile>? profiles = null)
    {
        var kernelRelativePath = NormalizeHttpPath(kernelPath);
        var initrdRelativePath = NormalizeHttpPath(initrdPath);
        var kernelHttpPath = "/" + kernelRelativePath;
        var initrdHttpPath = "/" + initrdRelativePath;

        return new BootAssetStatus(
            kernelHttpPath,
            Exists(kernelRelativePath, kernelHttpPath, profile, profiles),
            initrdHttpPath,
            Exists(initrdRelativePath, initrdHttpPath, profile, profiles));
    }

    private bool Exists(string relativePath, string httpPath, BootProfile? profile, IReadOnlyList<BootProfile>? profiles)
    {
        if (File.Exists(Path.Combine(_staticRoot, relativePath)))
            return true;

        return profile is not null &&
               profiles is not null &&
               _virtualIsoFiles is not null &&
               _virtualIsoFiles.FileExists(profile, httpPath, profiles);
    }

    private static string NormalizeHttpPath(string value)
    {
        var path = value.Trim();
        if (path.StartsWith("${boot-url}", StringComparison.OrdinalIgnoreCase))
            path = path["${boot-url}".Length..];

        if (Uri.TryCreate(path, UriKind.Absolute, out var uri))
            path = uri.AbsolutePath;

        return path.TrimStart('/').Replace('\\', '/');
    }
}
