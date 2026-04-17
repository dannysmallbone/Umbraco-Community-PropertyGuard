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

    public IEnumerable<PropertyGuardDto> GetPropertyGuards(string documentTypeAlias)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(documentTypeAlias);

            string cacheKey = $"{CachePrefix}{documentTypeAlias}";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<PropertyGuardDto>? cached)) return cached!;

            IReadOnlyDictionary<string, PropertyGuardEntry> guards = _propertyGuardRegistry.GetPropertyGuards(documentTypeAlias)
                ?? new Dictionary<string, PropertyGuardEntry>();

            if (guards.Count == 0) return [];

            IEnumerable<PropertyGuardDto> dtos = [.. guards.Select(guard => new PropertyGuardDto
            {
                DocumentTypeAlias = documentTypeAlias,
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
            _logger.LogError(ex, "Failed to get property guards for {documentTypeAlias}", documentTypeAlias);
            return [];
        }
    }

    public IEnumerable<PropertyGuardDto> GetPropertyGuards(string[] documentTypeAliases)
    {
        List<PropertyGuardDto> dtos = [];

        foreach (string? documentTypeAlias in documentTypeAliases.Distinct())
        {
            IEnumerable<PropertyGuardDto> propertyGuards = GetPropertyGuards(documentTypeAlias);
            dtos.AddRange(propertyGuards);
        }

        return dtos;
    }

    public IEnumerable<PropertyGuardDto> GetPropertyGuards()
    {
        string cacheKey = $"{CachePrefix}All";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<PropertyGuardDto>? cached)) return cached!;

        IReadOnlyDictionary<(string DocumentTypeAlias, string PropertyAlias), PropertyGuardEntry> allGuards = _propertyGuardRegistry.GetAllPropertyGuards();

        List<PropertyGuardDto> dtos = [.. allGuards.Select(guard =>
        {
            (string? documentTypeAlias, string? propertyAlias) = guard.Key;
            PropertyGuardEntry value = guard.Value;

            return new PropertyGuardDto
            {
                DocumentTypeAlias = documentTypeAlias,
                PropertyAlias = propertyAlias,
                FeatureKey = value.FeatureKey,
                Message = value.Message
            };
        })];

        MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions()
               .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
               .SetSlidingExpiration(TimeSpan.FromMinutes(2));

        _cache.Set(cacheKey, dtos, cacheOptions);

        return dtos;
    }
}
