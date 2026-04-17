using PropertyGuard.Dtos;

namespace PropertyGuard.Core;

public class PropertyGuardRegistry : IPropertyGuardRegistry
{
    private readonly Dictionary<string, IPropertyGuardMap> _maps = new(StringComparer.OrdinalIgnoreCase);

    public IPropertyGuardMap? GetMap(string documentTypeAlias)
    {
        _maps.TryGetValue(documentTypeAlias, out IPropertyGuardMap? map);
        return map;
    }

    public IReadOnlyDictionary<string, PropertyGuardEntry>? GetPropertyGuards(string documentTypeAlias)
    {
        return GetMap(documentTypeAlias)?.Guards;
    }

    public IReadOnlyDictionary<(string DocumentTypeAlias, string PropertyAlias), PropertyGuardEntry> GetAllPropertyGuards()
    {
        Dictionary<(string, string), PropertyGuardEntry> allGuards = [];

        foreach (KeyValuePair<string, IPropertyGuardMap> propertyGuardEntry in _maps)
        {
            string documentTypeAlias = propertyGuardEntry.Key;
            IReadOnlyDictionary<string, PropertyGuardEntry> guards = propertyGuardEntry.Value.Guards;

            foreach (KeyValuePair<string, PropertyGuardEntry> guard in guards)
            {
                (string documentTypeAlias, string Key) key = (documentTypeAlias, guard.Key);
                allGuards[key] = guard.Value;
            }
        }

        return allGuards;
    }

    public PropertyGuardRegistry RegisterGuard(string documentTypeAlias, IPropertyGuardMap map)
    {
        if (_maps.TryGetValue(documentTypeAlias, out IPropertyGuardMap? existing))
        {
            foreach (KeyValuePair<string, PropertyGuardEntry> kvp in map.Guards)
            {
                existing.Add(kvp.Key, kvp.Value.FeatureKey, kvp.Value.Message);
            }
        }
        else
        {
            _maps[documentTypeAlias] = map;
        }

        return this;
    }

    public PropertyGuardRegistry RegisterGuard(PropertyGuardDto propertyGuard)
    {
        if (!_maps.TryGetValue(propertyGuard.DocumentTypeAlias, out IPropertyGuardMap? map))
        {
            map = new PropertyGuardMap();
            _maps[propertyGuard.DocumentTypeAlias] = map;
        }

        map.Add(propertyGuard);

        return this;
    }
}
