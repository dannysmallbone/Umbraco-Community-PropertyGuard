using PropertyGuard.Dtos;

namespace PropertyGuard.Configuration;

public class PropertyGuardSettings
{
    public const string ConfigName = Constants.ConfigName;

    public List<PropertyGuardDto> Definitions { get; set; } = [];
}
