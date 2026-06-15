namespace EutherBoot;

public sealed class AssignmentStore
{
    private readonly string _path;

    public AssignmentStore(string path)
    {
        _path = path;
    }

    public string? GetProfileForMac(string mac)
    {
        var normalized = NormalizeMac(mac);
        return Load().FirstOrDefault(item => item.Mac == normalized)?.ProfileName;
    }

    public IReadOnlyList<BootAssignment> Load()
    {
        if (!File.Exists(_path))
            return [];

        return File.ReadLines(_path)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0 && !line.StartsWith('#'))
            .Select(line => line.Split('=', 2, StringSplitOptions.TrimEntries))
            .Where(parts => parts.Length == 2)
            .Select(parts => new BootAssignment(NormalizeMac(parts[0]), parts[1]))
            .ToList();
    }

    public void Set(string mac, string profileName)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);

        var normalized = NormalizeMac(mac);
        var assignments = Load()
            .Where(item => item.Mac != normalized)
            .Append(new BootAssignment(normalized, profileName))
            .OrderBy(item => item.Mac, StringComparer.OrdinalIgnoreCase)
            .ToList();

        File.WriteAllLines(_path, assignments.Select(item => $"{item.Mac}={item.ProfileName}"));
    }

    public static string NormalizeMac(string mac)
        => mac.Trim().Replace('-', ':').ToLowerInvariant();
}
