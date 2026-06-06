using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PropertyGuard.Core;
using PropertyGuard.Dtos;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace PropertyGuard.Services;

public class PropertyGuardService(
    ILogger<PropertyGuardService> logger,
    IPropertyGuardRegistry propertyGuardRegistry,
    IMemoryCache cache,
    IContentTypeService contentTypeService)
    : IPropertyGuardService
{
    private readonly ILogger<PropertyGuardService> _logger = logger;
    private readonly IPropertyGuardRegistry _propertyGuardRegistry = propertyGuardRegistry;
    private readonly IMemoryCache _cache = cache;
    private readonly IContentTypeService _contentTypeService = contentTypeService;
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
                Source = guard.Value.Source,
            })];

                _cache.Set(cacheKey, dtos, CreateCacheOptions());

            return dtos;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get property guards for {documentTypeAlias}", documentTypeAlias);
            return [];
        }
    }

    public IEnumerable<PropertyGuardDto> GetPropertyGuards(string[] documentTypeAliases)
        => documentTypeAliases.Distinct().SelectMany(GetPropertyGuards);

    public IEnumerable<PropertyGuardDto> GetPropertyGuards()
    {
        string cacheKey = $"{CachePrefix}All";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<PropertyGuardDto>? cached)) return cached!;

        IReadOnlyDictionary<(string DocumentTypeAlias, string PropertyAlias), PropertyGuardEntry> allGuards = _propertyGuardRegistry.GetAllPropertyGuards();

        List<PropertyGuardDto> dtos = [.. allGuards.Select(guard =>
        {
            (string? documentTypeAlias, string? propertyAlias) = guard.Key;
            PropertyGuardEntry value = guard.Value;

            IContentType? documentType = _contentTypeService.Get(documentTypeAlias);
            IPropertyType? propertyType = documentType?.PropertyTypes.FirstOrDefault(p => p.Alias.Equals(propertyAlias, StringComparison.OrdinalIgnoreCase));

            return new PropertyGuardDto
            {
                DocumentTypeAlias = documentType?.Alias ?? documentTypeAlias,
                PropertyAlias = propertyType?.Alias ?? propertyAlias,
                DocumentTypeName = documentType?.Name,
                PropertyTypeName = propertyType?.Name,
                DocumentTypeUnique = documentType?.Key.ToString(),
                PropertyTypeUnique = propertyType?.Key.ToString(),
                Icon = documentType?.Icon,
                FeatureKey = value.FeatureKey,
                Message = value.Message,
                Source = value.Source,
            };
        })];

        _cache.Set(cacheKey, dtos, CreateCacheOptions());

        return dtos;
    }

    private static MemoryCacheEntryOptions CreateCacheOptions() =>
        new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
            .SetSlidingExpiration(TimeSpan.FromMinutes(2));
}
