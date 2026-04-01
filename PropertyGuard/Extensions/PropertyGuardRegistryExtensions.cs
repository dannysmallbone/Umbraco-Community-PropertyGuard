using PropertyGuard.Core;

namespace PropertyGuard.Extensions;

public static class PropertyGuardRegistryExtensions
{
    public static IPropertyGuardMap CreateGuard(this IPropertyGuardRegistry propertyGuardRegistry, string contentTypeAlias)
    {
        IPropertyGuardMap? map = propertyGuardRegistry.GetMap(contentTypeAlias);

        if (map == null)
        {
            map = new PropertyGuardMap();
            propertyGuardRegistry.RegisterGuard(contentTypeAlias, map);
        }

        return map;
    }
}
