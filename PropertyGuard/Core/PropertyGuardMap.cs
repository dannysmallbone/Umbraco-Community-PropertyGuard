using PropertyGuard.Dtos;

namespace PropertyGuard.Core;

public class PropertyGuardMap : IPropertyGuardMap
{
    private readonly Dictionary<string, PropertyGuardEntry> _guards = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, PropertyGuardEntry> Guards => _guards;

    public IPropertyGuardMap Add(string propertyAlias, string? featureKey = null, string? message = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyAlias);

        _guards[propertyAlias] = new PropertyGuardEntry(featureKey, message);
        return this;
    }

    public IPropertyGuardMap Add(PropertyGuardDto propertyGuard)
        => Add(propertyGuard.PropertyAlias, propertyGuard.FeatureKey, propertyGuard.Message);

    public IPropertyGuardMap RegisterProperty(string propertyAlias, string? featureKey = null, string? message = null)
        => Add(propertyAlias, featureKey, message);
}
