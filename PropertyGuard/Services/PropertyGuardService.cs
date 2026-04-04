using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PropertyGuard.Core;
using PropertyGuard.Dtos;

namespace PropertyGuard.Services;

public class PropertyGuardService(
    ILogger<PropertyGuardService> logger,
    IPropertyGuardRegistry propertyGuardRegistry,
    IMemoryCache cache)
    : IPropertyGuardService
{
    private readonly ILogger<PropertyGuardService> _logger = logger;
    private readonly IPropertyGuardRegistry _propertyGuardRegistry = propertyGuardRegistry;
    private readonly IMemoryCache _cache = cache;

    private const string CachePrefix = "PropertyGuard_Guards_";

    public IEnumerable<PropertyGuardDto> GetPropertyGuards(string contentTypeAlias)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(contentTypeAlias);

            string cacheKey = $"{CachePrefix}{contentTypeAlias}";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<PropertyGuardDto>? cached)) return cached!;

            IReadOnlyDictionary<string, PropertyGuardEntry> guards = _propertyGuardRegistry.GetPropertyGuards(contentTypeAlias)
                ?? new Dictionary<string, PropertyGuardEntry>();

            if (guards.Count == 0) return [];

            IEnumerable<PropertyGuardDto> dtos = [.. guards.Select(guard => new PropertyGuardDto
            {
                ContentTypeAlias = contentTypeAlias,
                PropertyAlias = guard.Key,
                FeatureKey = guard.Value.FeatureKey,
                Message = guard.Value.Message,
            })];

            MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));

            _cache.Set(cacheKey, dtos, cacheOptions);

            return dtos;

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
