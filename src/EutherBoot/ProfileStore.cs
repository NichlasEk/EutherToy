namespace EutherBoot;

public sealed class ProfileStore
{
    private readonly string _profileDirectory;

    public ProfileStore(string profileDirectory)
    {
        _profileDirectory = profileDirectory;
    }

    public IReadOnlyList<BootProfile> LoadProfiles()
    {
        if (!Directory.Exists(_profileDirectory))
            return [];

        return Directory.EnumerateFiles(_profileDirectory, "*.toml")
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .Select(ProfileToml.ParseFile)
            .ToList();
    }

    public BootProfile? FindProfile(string name)
        => LoadProfiles().FirstOrDefault(profile => string.Equals(profile.Name, name, StringComparison.OrdinalIgnoreCase));
}
