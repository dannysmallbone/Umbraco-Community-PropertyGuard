using PropertyGuard.Dtos;

namespace PropertyGuard.Core;

public class PropertyGuardMap : IPropertyGuardMap
{
    private readonly Dictionary<string, PropertyGuardEntry> _guards = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, PropertyGuardEntry> Guards => _guards;

    public IPropertyGuardMap Add(string propertyAlias, PropertyGuardEntry entry)
    {
        if (string.IsNullOrWhiteSpace(propertyAlias)) return this;

        _guards[propertyAlias] = entry;
        return this;
    }

    public IPropertyGuardMap Add(string propertyAlias, string? featureKey = null, string? message = null, PropertyGuardSource source = PropertyGuardSource.Code, PropertyGuardMode mode = PropertyGuardMode.ReadOnly)
        => Add(propertyAlias, new PropertyGuardEntry(featureKey, message, source, mode));

    public IPropertyGuardMap Add(PropertyGuardDto propertyGuard)
        => Add(propertyGuard.PropertyAlias, new PropertyGuardEntry(propertyGuard.FeatureKey, propertyGuard.Message, propertyGuard.Source, propertyGuard.Mode));

    public IPropertyGuardMap RegisterProperty(string propertyAlias, string? featureKey = null, string? message = null)
        => Add(propertyAlias, featureKey, message);

    public IPropertyGuardMap Remove(string propertyAlias)
    {
        _guards.Remove(propertyAlias);
        return this;
    }
}
