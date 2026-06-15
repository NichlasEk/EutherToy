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
            AppendBootBlock(sb, profile);

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
}
