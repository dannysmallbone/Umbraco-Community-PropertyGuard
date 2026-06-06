using System.Text.Json.Serialization;

namespace PropertyGuard.Core;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PropertyGuardSource
{
    Code,
    Config,
    Ui,
}
