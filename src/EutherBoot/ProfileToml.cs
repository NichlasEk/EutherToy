namespace EutherBoot;

public static class ProfileToml
{
    public static BootProfile ParseFile(string path)
    {
        var profile = new BootProfile();
        var section = "";
        var pendingKey = "";
        var pendingArray = "";

        foreach (var rawLine in File.ReadLines(path))
        {
            var line = StripComment(rawLine).Trim();
            if (line.Length == 0)
                continue;

            if (pendingKey.Length > 0)
            {
                pendingArray += " " + line;
                if (line.EndsWith(']'))
                {
                    ApplyValue(profile, section, pendingKey, pendingArray);
                    pendingKey = "";
                    pendingArray = "";
                }

                continue;
            }

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                section = line[1..^1].Trim();
                continue;
            }

            var equalsIndex = line.IndexOf('=');
            if (equalsIndex < 0)
                continue;

            var key = line[..equalsIndex].Trim();
            var value = line[(equalsIndex + 1)..].Trim();

            if (value.StartsWith('[') && !value.EndsWith(']'))
            {
                pendingKey = key;
                pendingArray = value;
                continue;
            }

            ApplyValue(profile, section, key, value);
        }

        Validate(profile, path);
        return profile;
    }

    private static void ApplyValue(BootProfile profile, string section, string key, string value)
    {
        switch (section, key)
        {
            case ("id", "name"):
                profile.Name = ParseString(value);
                break;
            case ("id", "label"):
                profile.Label = ParseString(value);
                break;
            case ("id", "type"):
                profile.Type = ParseString(value);
                break;
            case ("match", "filename_contains"):
                profile.Match.FilenameContains = ParseStringArray(value);
                break;
            case ("boot", "kernel"):
                profile.Boot.Kernel = ParseString(value);
                break;
            case ("boot", "initrd"):
                profile.Boot.Initrd = ParseString(value);
                break;
            case ("boot", "args"):
                profile.Boot.Args = ParseStringArray(value);
                break;
        }
    }

    private static void Validate(BootProfile profile, string path)
    {
        if (string.IsNullOrWhiteSpace(profile.Name))
            throw new InvalidOperationException($"{path}: [id].name is required.");
        if (string.IsNullOrWhiteSpace(profile.Label))
            throw new InvalidOperationException($"{path}: [id].label is required.");
        if (string.IsNullOrWhiteSpace(profile.Boot.Kernel))
            throw new InvalidOperationException($"{path}: [boot].kernel is required.");
        if (string.IsNullOrWhiteSpace(profile.Boot.Initrd))
            throw new InvalidOperationException($"{path}: [boot].initrd is required.");
    }

    private static string StripComment(string line)
    {
        var inString = false;
        for (var i = 0; i < line.Length; i++)
        {
            if (line[i] == '"' && (i == 0 || line[i - 1] != '\\'))
                inString = !inString;
            if (!inString && line[i] == '#')
                return line[..i];
        }

        return line;
    }

    private static string ParseString(string value)
    {
        value = value.Trim();
        if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
            return value[1..^1].Replace("\\\"", "\"", StringComparison.Ordinal);

        return value;
    }

    private static List<string> ParseStringArray(string value)
    {
        value = value.Trim();
        if (!value.StartsWith('[') || !value.EndsWith(']'))
            return [];

        var items = new List<string>();
        var current = "";
        var inString = false;

        foreach (var ch in value[1..^1])
        {
            if (ch == '"')
            {
                inString = !inString;
                continue;
            }

            if (ch == ',' && !inString)
            {
                AddItem(items, current);
                current = "";
                continue;
            }

            current += ch;
        }

        AddItem(items, current);
        return items;
    }

    private static void AddItem(List<string> items, string value)
    {
        var item = value.Trim();
        if (item.Length > 0)
            items.Add(item);
    }
}
