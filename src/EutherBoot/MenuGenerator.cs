using System.Text;

namespace EutherBoot;

public static class MenuGenerator
{
    public static string GenerateDefaultMenu(IEnumerable<BootProfile> profiles, string bootUrl)
    {
        var orderedProfiles = profiles.OrderBy(profile => profile.Label, StringComparer.OrdinalIgnoreCase).ToList();
        var sb = new StringBuilder();

        sb.AppendLine("#!ipxe");
        sb.AppendLine();
        sb.AppendLine($"set boot-url {bootUrl.TrimEnd('/')}");
        sb.AppendLine();
        sb.AppendLine("menu EutherBoot");

        foreach (var profile in orderedProfiles)
            sb.AppendLine($"item {SanitizeLabel(profile.Name)} {profile.Label}");

        sb.AppendLine("item shell iPXE shell");
        sb.AppendLine("choose target && goto ${target}");
        sb.AppendLine();

        foreach (var profile in orderedProfiles)
            AppendChainBlock(sb, profile);

        sb.AppendLine(":shell");
        sb.AppendLine("shell");

        return sb.ToString();
    }

    public static string GenerateSingleBoot(BootProfile profile, string bootUrl)
    {
        var sb = new StringBuilder();

        sb.AppendLine("#!ipxe");
        sb.AppendLine();
        sb.AppendLine($"set boot-url {bootUrl.TrimEnd('/')}");
        sb.AppendLine("echo EutherBoot assigned profile: " + profile.Label);
        sb.AppendLine();
        AppendBootBlock(sb, profile);

        return sb.ToString();
    }

    public static string GenerateMountError(BootProfile profile, MountResult mountResult, string bootUrl)
    {
        var sb = new StringBuilder();

        sb.AppendLine("#!ipxe");
        sb.AppendLine();
        sb.AppendLine($"set boot-url {bootUrl.TrimEnd('/')}");
        sb.AppendLine($"echo EutherBoot could not prepare {profile.Label}");
        sb.AppendLine($"echo {EscapeEcho(mountResult.Message)}");
        if (mountResult.IsoPath is not null)
            sb.AppendLine($"echo ISO: {EscapeEcho(mountResult.IsoPath)}");
        if (mountResult.MountPath is not null)
            sb.AppendLine($"echo Mount: {EscapeEcho(mountResult.MountPath)}");
        sb.AppendLine("shell");

        return sb.ToString();
    }

    private static void AppendChainBlock(StringBuilder sb, BootProfile profile)
    {
        sb.AppendLine($":{SanitizeLabel(profile.Name)}");
        sb.AppendLine($"chain ${{boot-url}}/api/boot/profile/{Uri.EscapeDataString(profile.Name)}");
        sb.AppendLine();
    }

    private static void AppendBootBlock(StringBuilder sb, BootProfile profile)
    {
        var args = profile.Boot.Args.Count == 0 ? "" : " " + string.Join(" ", profile.Boot.Args);

        sb.AppendLine($":{SanitizeLabel(profile.Name)}");
        sb.AppendLine($"kernel ${{boot-url}}{EnsureLeadingSlash(profile.Boot.Kernel)}{args}");
        sb.AppendLine($"initrd ${{boot-url}}{EnsureLeadingSlash(profile.Boot.Initrd)}");
        sb.AppendLine("boot");
        sb.AppendLine();
    }

    private static string EnsureLeadingSlash(string value)
        => value.StartsWith('/') ? value : "/" + value;

    private static string SanitizeLabel(string value)
        => string.Concat(value.Select(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_' ? ch : '-'));

    private static string EscapeEcho(string value)
        => value.Replace('\n', ' ').Replace('\r', ' ');
}
