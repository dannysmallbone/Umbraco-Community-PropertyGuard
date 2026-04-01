using PropertyGuard.Dtos;

namespace PropertyGuard.Core;

public interface IPropertyGuardMap
{
    IReadOnlyDictionary<string, PropertyGuardEntry> Guards { get; }
    IPropertyGuardMap Add(string propertyAlias, string? featureKey = null, string? message = null);
    IPropertyGuardMap Add(PropertyGuardDto propertyGuard);
    IPropertyGuardMap RegisterProperty(string propertyAlias, string? featureKey = null, string? message = null);
}
