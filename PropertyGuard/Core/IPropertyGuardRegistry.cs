using PropertyGuard.Dtos;

namespace PropertyGuard.Core;

public interface IPropertyGuardRegistry
{
    IReadOnlyDictionary<(string ContentTypeAlias, string PropertyAlias), PropertyGuardEntry> GetAllPropertyGuards();
    IPropertyGuardMap? GetMap(string contentTypeAlias);
    IReadOnlyDictionary<string, PropertyGuardEntry>? GetPropertyGuards(string contentTypeAlias);
    PropertyGuardRegistry RegisterGuard(string contentTypeAlias, IPropertyGuardMap map);
    PropertyGuardRegistry RegisterGuard(PropertyGuardDto propertyGuard);
}
