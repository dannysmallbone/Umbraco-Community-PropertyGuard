using PropertyGuard.Dtos;

namespace PropertyGuard.Core;

public interface IPropertyGuardMap
{
    IReadOnlyDictionary<string, PropertyGuardEntry> Guards { get; }
    IPropertyGuardMap Add(string propertyAlias, PropertyGuardEntry entry);
    IPropertyGuardMap Add(string propertyAlias, string? featureKey = null, string? message = null, PropertyGuardSource source = PropertyGuardSource.Code, PropertyGuardMode mode = PropertyGuardMode.ReadOnly);
    IPropertyGuardMap Add(PropertyGuardDto propertyGuard);
    IPropertyGuardMap RegisterProperty(string propertyAlias, string? featureKey = null, string? message = null);
    IPropertyGuardMap Remove(string propertyAlias);
}
