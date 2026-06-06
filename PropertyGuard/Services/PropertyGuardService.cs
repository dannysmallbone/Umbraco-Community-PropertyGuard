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

            IContentType? documentType = _contentTypeService.Get(documentTypeAlias);

            IEnumerable<PropertyGuardDto> dtos = [.. guards.Select(guard =>
            {
                IPropertyType? propertyType = FindPropertyType(documentType, guard.Key);

                return new PropertyGuardDto
                {
                    DocumentTypeAlias = documentType?.Alias ?? documentTypeAlias,
                    PropertyAlias = propertyType?.Alias ?? guard.Key,
                    PropertyTypeUnique = propertyType?.Key.ToString(),
                    FeatureKey = guard.Value.FeatureKey,
                    Message = guard.Value.Message,
                    Source = guard.Value.Source,
                    Mode = guard.Value.Mode,
                    Permissions = guard.Value.Mode.ToPermissions(),
                };
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
            IPropertyType? propertyType = FindPropertyType(documentType, propertyAlias);

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
                Mode = value.Mode,
                Permissions = value.Mode.ToPermissions(),
            };
        })];

        _cache.Set(cacheKey, dtos, CreateCacheOptions());

        return dtos;
    }

    private static IPropertyType? FindPropertyType(IContentTypeComposition? contentType, string propertyAlias)
    {
        if (contentType is null) return null;

        IPropertyType? found = contentType.PropertyTypes
            .FirstOrDefault(p => p.Alias.Equals(propertyAlias, StringComparison.OrdinalIgnoreCase));

        if (found is not null) return found;

        foreach (IContentTypeComposition composition in contentType.ContentTypeComposition)
        {
            found = FindPropertyType(composition, propertyAlias);
            if (found is not null) return found;
        }

        return null;
    }

    private static MemoryCacheEntryOptions CreateCacheOptions() =>
        new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
            .SetSlidingExpiration(TimeSpan.FromMinutes(2));
}
