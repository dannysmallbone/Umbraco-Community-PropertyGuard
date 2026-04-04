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

    public IReadOnlyDictionary<(string ContentTypeAlias, string PropertyAlias), PropertyGuardEntry> GetAllPropertyGuards()
    {
        Dictionary<(string, string), PropertyGuardEntry> allGuards = [];

        foreach (KeyValuePair<string, IPropertyGuardMap> propertyGuardEntry in _maps)
        {
            string contentTypeAlias = propertyGuardEntry.Key;
            IReadOnlyDictionary<string, PropertyGuardEntry> guards = propertyGuardEntry.Value.Guards;

            foreach (KeyValuePair<string, PropertyGuardEntry> guard in guards)
            {
                (string contentTypeAlias, string Key) key = (contentTypeAlias, guard.Key);
                allGuards[key] = guard.Value;
            }
        }

        return allGuards;
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
