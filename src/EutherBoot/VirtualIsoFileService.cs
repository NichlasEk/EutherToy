using DiscUtils.Iso9660;

namespace EutherBoot;

public sealed class VirtualIsoFileService
{
    private readonly string _isoDirectory;

    public VirtualIsoFileService(string isoDirectory)
    {
        _isoDirectory = isoDirectory;
    }

    public bool FileExists(BootProfile profile, string httpPath, IReadOnlyList<BootProfile> profiles)
    {
        if (!TryResolveIsoPath(profile, httpPath, profiles, out var isoPath, out var isoInnerPath))
            return false;

        using var isoStream = File.OpenRead(isoPath);
        var reader = new CDReader(isoStream, true);
        return reader.FileExists(isoInnerPath);
    }

    public VirtualIsoFile? Open(BootProfile profile, string httpPath, IReadOnlyList<BootProfile> profiles)
    {
        if (!TryResolveIsoPath(profile, httpPath, profiles, out var isoPath, out var isoInnerPath))
            return null;

        var isoStream = File.OpenRead(isoPath);
        var reader = new CDReader(isoStream, true);
        if (!reader.FileExists(isoInnerPath))
        {
            isoStream.Dispose();
            return null;
        }

        var fileStream = reader.OpenFile(isoInnerPath, FileMode.Open, FileAccess.Read);
        return new VirtualIsoFile(new OwnedStream(fileStream, isoStream), GuessContentType(isoInnerPath));
    }

    private bool TryResolveIsoPath(
        BootProfile profile,
        string httpPath,
        IReadOnlyList<BootProfile> profiles,
        out string isoPath,
        out string isoInnerPath)
    {
        isoPath = "";
        isoInnerPath = "";

        var mountPrefix = $"/mounts/{profile.Name}/";
        var normalizedPath = NormalizeHttpPath(httpPath);
        if (!normalizedPath.StartsWith(mountPrefix, StringComparison.OrdinalIgnoreCase))
            return false;

        var iso = FindIsoForProfile(profile, profiles);
        if (iso is null)
            return false;

        isoPath = Path.Combine(_isoDirectory, iso.FileName);
        isoInnerPath = normalizedPath[mountPrefix.Length..].Replace('/', '\\');
        return isoInnerPath.Length > 0;
    }

    private IsoImage? FindIsoForProfile(BootProfile profile, IReadOnlyList<BootProfile> profiles)
    {
        if (!Directory.Exists(_isoDirectory))
            return null;

        return Directory.EnumerateFiles(_isoDirectory, "*.iso")
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .Select(path => ToIsoImage(path, profiles))
            .FirstOrDefault(iso =>
                iso.Complete &&
                string.Equals(iso.MatchedProfileName, profile.Name, StringComparison.OrdinalIgnoreCase));
    }

    private static IsoImage ToIsoImage(string path, IReadOnlyList<BootProfile> profiles)
    {
        var info = new FileInfo(path);
        var profile = MatchProfile(info.Name, profiles);

        return new IsoImage(
            info.Name,
            info.Length,
            info.LastWriteTimeUtc,
            !File.Exists(path + ".aria2"),
            profile?.Name,
            profile?.Label,
            null);
    }

    private static BootProfile? MatchProfile(string fileName, IReadOnlyList<BootProfile> profiles)
    {
        return profiles.FirstOrDefault(profile =>
            profile.Match.FilenameContains.Count > 0 &&
            profile.Match.FilenameContains.All(needle =>
                fileName.Contains(needle, StringComparison.OrdinalIgnoreCase)));
    }

    private static string NormalizeHttpPath(string value)
    {
        var path = value.Trim();
        if (path.StartsWith("${boot-url}", StringComparison.OrdinalIgnoreCase))
            path = path["${boot-url}".Length..];

        if (Uri.TryCreate(path, UriKind.Absolute, out var uri))
            path = uri.AbsolutePath;

        return "/" + path.TrimStart('/').Replace('\\', '/');
    }

    private static string GuessContentType(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".txt" or ".cfg" or ".conf" => "text/plain",
            ".ipxe" => "text/plain",
            ".iso" => "application/octet-stream",
            ".img" or ".gz" or ".xz" or ".squashfs" => "application/octet-stream",
            _ => "application/octet-stream"
        };
    }
}

public sealed record VirtualIsoFile(Stream Stream, string ContentType);

public sealed class OwnedStream : Stream
{
    private readonly Stream _inner;
    private readonly IDisposable[] _owners;

    public OwnedStream(Stream inner, params IDisposable[] owners)
    {
        _inner = inner;
        _owners = owners;
    }

    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => _inner.CanSeek;
    public override bool CanWrite => false;
    public override long Length => _inner.Length;
    public override long Position
    {
        get => _inner.Position;
        set => _inner.Position = value;
    }

    public override void Flush() => _inner.Flush();
    public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _inner.Dispose();
            foreach (var owner in _owners)
                owner.Dispose();
        }

        base.Dispose(disposing);
    }
}
