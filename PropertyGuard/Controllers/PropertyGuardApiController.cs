using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PropertyGuard.Core;
using PropertyGuard.Dtos;
using Umbraco.Cms.Api.Common.Builders;

namespace PropertyGuard.Controllers;

[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = Constants.Name)]
public class PropertyGuardApiController(
    ILogger<PropertyGuardApiController> logger,
    IPropertyGuardRegistry propertyGuardRegistry) : PropertyGuardApiControllerBase
{
    private readonly ILogger<PropertyGuardApiController> _logger = logger;
    private readonly IPropertyGuardRegistry _propertyGuardRegistry = propertyGuardRegistry;

    [HttpGet("GetPropertyGuards")]
    [ProducesResponseType<List<PropertyGuardDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public IActionResult GetPropertyGuards(string contentTypeAlias)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(contentTypeAlias);

            IReadOnlyDictionary<string, PropertyGuardEntry>? guards = _propertyGuardRegistry.GetPropertyGuards(contentTypeAlias);
            if (guards == null || guards.Count == 0)
            {
                return Ok(new List<PropertyGuardDto>());
            }

            IEnumerable<PropertyGuardDto> result = [.. guards.Select(kvp => new PropertyGuardDto
            {
                ContentTypeAlias = contentTypeAlias,
                PropertyAlias = kvp.Key,
                FeatureKey = kvp.Value.FeatureKey,
                Message = kvp.Value.Message
            })];

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get property guards for {contentTypeAlias}", contentTypeAlias);

            return BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Error")
                .WithDetail("Failed to load property guards")
                .Build());
        }
    }
}
