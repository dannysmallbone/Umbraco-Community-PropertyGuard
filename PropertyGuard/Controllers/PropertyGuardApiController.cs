using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PropertyGuard.Dtos;
using PropertyGuard.Services;
using Umbraco.Cms.Api.Common.Builders;

namespace PropertyGuard.Controllers;

[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = Constants.Name)]
public class PropertyGuardApiController(
    ILogger<PropertyGuardApiController> logger,
    IPropertyGuardService propertyGuardService) : PropertyGuardApiControllerBase
{
    private readonly ILogger<PropertyGuardApiController> _logger = logger;
    private readonly IPropertyGuardService _propertyGuardService = propertyGuardService;

    [HttpGet("GetPropertyGuards")]
    [ProducesResponseType<List<PropertyGuardDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public IActionResult GetPropertyGuards(string contentTypeAlias)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(contentTypeAlias);

            IEnumerable<PropertyGuardDto> propertyGuards = _propertyGuardService.GetPropertyGuards(contentTypeAlias);

            return Ok(propertyGuards);
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
