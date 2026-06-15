namespace EutherBoot;

public sealed class BootProfile
{
    public string Name { get; set; } = "";
    public string Label { get; set; } = "";
    public string Type { get; set; } = "";
    public BootPaths Boot { get; set; } = new();
    public MatchRules Match { get; set; } = new();
}

public sealed class BootPaths
{
    public string Kernel { get; set; } = "";
    public string Initrd { get; set; } = "";
    public List<string> Args { get; set; } = new();
}

public sealed class MatchRules
{
    public List<string> FilenameContains { get; set; } = new();
}

public sealed record BootAssignment(string Mac, string ProfileName);

public sealed record IsoImage(
    string FileName,
    long SizeBytes,
    DateTimeOffset LastWriteUtc,
    string? MatchedProfileName,
    string? MatchedProfileLabel,
    BootAssetStatus? Assets);

public sealed record BootAssetStatus(
    string KernelPath,
    bool KernelExists,
    string InitrdPath,
    bool InitrdExists)
{
    public bool Ready => KernelExists && InitrdExists;
}
