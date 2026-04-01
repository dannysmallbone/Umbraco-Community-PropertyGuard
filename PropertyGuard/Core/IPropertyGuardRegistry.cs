using PropertyGuard.Dtos;

namespace PropertyGuard.Core;

public interface IPropertyGuardRegistry
{
    IPropertyGuardMap? GetMap(string contentTypeAlias);
    IReadOnlyDictionary<string, PropertyGuardEntry>? GetPropertyGuards(string contentTypeAlias);
    PropertyGuardRegistry RegisterGuard(string contentTypeAlias, IPropertyGuardMap map);
    PropertyGuardRegistry RegisterGuard(PropertyGuardDto propertyGuard);
}
