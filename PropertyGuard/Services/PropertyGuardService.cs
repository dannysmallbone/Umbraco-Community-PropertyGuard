using Microsoft.Extensions.Logging;
using PropertyGuard.Core;
using PropertyGuard.Dtos;

namespace PropertyGuard.Services;

public class PropertyGuardService(
    ILogger<PropertyGuardService> logger,
    IPropertyGuardRegistry propertyGuardRegistry)
    : IPropertyGuardService
{
    private readonly ILogger<PropertyGuardService> _logger = logger;
    private readonly IPropertyGuardRegistry _propertyGuardRegistry = propertyGuardRegistry;

    public IEnumerable<PropertyGuardDto> GetPropertyGuards(string contentTypeAlias)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(contentTypeAlias);

            IReadOnlyDictionary<string, PropertyGuardEntry> guards = _propertyGuardRegistry.GetPropertyGuards(contentTypeAlias)
                ?? new Dictionary<string, PropertyGuardEntry>();

            if (guards.Count == 0) return [];

            IEnumerable<PropertyGuardDto> results = guards.Select(guard => new PropertyGuardDto
            {
                ContentTypeAlias = contentTypeAlias,
                PropertyAlias = guard.Key,
                FeatureKey = guard.Value.FeatureKey,
                Message = guard.Value.Message,
            });

            return results;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get property guards for {contentTypeAlias}", contentTypeAlias);
            return [];
        }
    }

    public IEnumerable<PropertyGuardDto> GetPropertyGuards(string[] contentTypeAliases)
    {
        List<PropertyGuardDto> results = [];

        foreach (string? contentTypeAlias in contentTypeAliases.Distinct())
        {
            IEnumerable<PropertyGuardDto> propertyGuards = GetPropertyGuards(contentTypeAlias);
            results.AddRange(propertyGuards);
        }

        return results;
    }
}
