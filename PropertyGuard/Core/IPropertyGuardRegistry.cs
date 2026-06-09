using PropertyGuard.Dtos;

namespace PropertyGuard.Core;

public interface IPropertyGuardRegistry
{
    IReadOnlyDictionary<(string DocumentTypeAlias, string PropertyAlias), PropertyGuardEntry> GetAllPropertyGuards();
    IPropertyGuardMap? GetMap(string documentTypeAlias);
    IReadOnlyDictionary<string, PropertyGuardEntry>? GetPropertyGuards(string documentTypeAlias);
    PropertyGuardRegistry RegisterGuard(string documentTypeAlias, IPropertyGuardMap map);
    PropertyGuardRegistry RegisterGuard(PropertyGuardDto propertyGuard);
    void RemoveGuard(string documentTypeAlias, string propertyAlias);
}
