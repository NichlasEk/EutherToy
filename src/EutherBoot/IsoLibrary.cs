namespace EutherBoot;

public sealed class IsoLibrary
{
    private readonly string _isoDirectory;
    private readonly BootAssetChecker _assetChecker;

    public IsoLibrary(string isoDirectory, BootAssetChecker assetChecker)
    {
        _isoDirectory = isoDirectory;
        _assetChecker = assetChecker;
    }

    public IReadOnlyList<IsoImage> Scan(IReadOnlyList<BootProfile> profiles)
    {
        if (!Directory.Exists(_isoDirectory))
            return [];

        return Directory.EnumerateFiles(_isoDirectory, "*.iso")
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .Select(path => ToIsoImage(path, profiles))
            .ToList();
    }

    public IReadOnlyList<BootProfile> GetMenuProfiles(IReadOnlyList<BootProfile> profiles)
    {
        var matchedNames = Scan(profiles)
            .Where(iso => iso.MatchedProfileName is not null)
            .Select(iso => iso.MatchedProfileName!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (matchedNames.Count == 0)
            return profiles;

        return profiles
            .Where(profile => matchedNames.Contains(profile.Name))
            .OrderBy(profile => profile.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private IsoImage ToIsoImage(string path, IReadOnlyList<BootProfile> profiles)
    {
        var info = new FileInfo(path);
        var profile = MatchProfile(info.Name, profiles);

        return new IsoImage(
            info.Name,
            info.Length,
            info.LastWriteTimeUtc,
            profile?.Name,
            profile?.Label,
            profile is null ? null : _assetChecker.Check(profile));
    }

    private static BootProfile? MatchProfile(string fileName, IReadOnlyList<BootProfile> profiles)
    {
        return profiles.FirstOrDefault(profile =>
            profile.Match.FilenameContains.Count > 0 &&
            profile.Match.FilenameContains.All(needle =>
                fileName.Contains(needle, StringComparison.OrdinalIgnoreCase)));
    }
}
