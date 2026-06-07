using System.Text.Json.Serialization;

namespace PropertyGuard.Core;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PropertyGuardMode
{
    ReadOnly,
    Hidden,
}

public static class PropertyGuardModeExtensions
{
    public static PropertyGuardMode FromPermissions(IEnumerable<string> permissions)
    {
        bool canRead = permissions.Contains("Read", StringComparer.OrdinalIgnoreCase);
        return canRead ? PropertyGuardMode.ReadOnly : PropertyGuardMode.Hidden;
    }

    public static List<string> ToPermissions(this PropertyGuardMode mode) =>
        mode switch
        {
            PropertyGuardMode.ReadOnly => ["Read"],
            PropertyGuardMode.Hidden => [],
            _ => ["Read"],
        };
}
