using PropertyGuard.Dtos;

namespace PropertyGuard.Core;

public class PropertyGuardRegistry : IPropertyGuardRegistry
{
    private readonly Dictionary<string, IPropertyGuardMap> _maps = new(StringComparer.OrdinalIgnoreCase);

    public IPropertyGuardMap? GetMap(string contentTypeAlias)
    {
        _maps.TryGetValue(contentTypeAlias, out IPropertyGuardMap? map);
        return map;
    }

    public IReadOnlyDictionary<string, PropertyGuardEntry>? GetPropertyGuards(string contentTypeAlias)
    {
        return GetMap(contentTypeAlias)?.Guards;
    }

    public PropertyGuardRegistry RegisterGuard(string contentTypeAlias, IPropertyGuardMap map)
    {
        if (_maps.TryGetValue(contentTypeAlias, out IPropertyGuardMap? existing))
        {
            foreach (KeyValuePair<string, PropertyGuardEntry> kvp in map.Guards)
            {
                existing.Add(kvp.Key, kvp.Value.FeatureKey, kvp.Value.Message);
            }
        }
        else
        {
            _maps[contentTypeAlias] = map;
        }

        return this;
    }

    public PropertyGuardRegistry RegisterGuard(PropertyGuardDto propertyGuard)
    {
        if (!_maps.TryGetValue(propertyGuard.ContentTypeAlias, out IPropertyGuardMap? map))
        {
            map = new PropertyGuardMap();
            _maps[propertyGuard.ContentTypeAlias] = map;
        }

        map.Add(propertyGuard);

        return this;
    }
}
