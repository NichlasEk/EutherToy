namespace EutherBoot;

public sealed class BootAssetChecker
{
    private readonly string _staticRoot;

    public BootAssetChecker(string staticRoot)
    {
        _staticRoot = staticRoot;
    }

    public BootAssetStatus Check(BootProfile profile)
        => Check(profile.Boot.Kernel, profile.Boot.Initrd);

    public BootAssetStatus Check(string kernelPath, string initrdPath)
    {
        var kernelRelativePath = NormalizeHttpPath(kernelPath);
        var initrdRelativePath = NormalizeHttpPath(initrdPath);

        return new BootAssetStatus(
            "/" + kernelRelativePath,
            File.Exists(Path.Combine(_staticRoot, kernelRelativePath)),
            "/" + initrdRelativePath,
            File.Exists(Path.Combine(_staticRoot, initrdRelativePath)));
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
