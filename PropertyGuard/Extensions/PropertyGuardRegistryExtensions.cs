using PropertyGuard.Core;

namespace PropertyGuard.Extensions;

public static class PropertyGuardRegistryExtensions
{
    public static IPropertyGuardMap CreateGuard(this IPropertyGuardRegistry propertyGuardRegistry, string documentTypeAlias)
    {
        IPropertyGuardMap? map = propertyGuardRegistry.GetMap(documentTypeAlias);

        if (map == null)
        {
            map = new PropertyGuardMap();
            propertyGuardRegistry.RegisterGuard(documentTypeAlias, map);
        }

        return map;
    }
}
