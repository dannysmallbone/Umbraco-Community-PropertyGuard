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

    [HttpGet("GetPropertyGuardsByContentTypeAlias")]
    [ProducesResponseType<List<PropertyGuardDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public IActionResult GetPropertyGuardsByContentTypeAlias(string contentTypeAlias)
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

    [HttpGet("GetPropertyGuardsByContentTypeAliases")]
    [ProducesResponseType<List<PropertyGuardDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public IActionResult GetPropertyGuardsByContentTypeAliases([FromQuery] string[] contentTypeAliases)
    {
        try
        {
            if (contentTypeAliases == null || contentTypeAliases.Length == 0) return Ok(new List<PropertyGuardDto>());

            IEnumerable<PropertyGuardDto> propertyGuards = _propertyGuardService.GetPropertyGuards(contentTypeAliases);

            return Ok(propertyGuards);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get property guards for {contentTypeAliases}", contentTypeAliases);

            return BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Error")
                .WithDetail("Failed to load property guards")
                .Build());
        }
    }

    [HttpGet("GetPropertyGuards")]
    [ProducesResponseType<List<PropertyGuardDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public IActionResult GetPropertyGuards()
    {
        try
        {
            IEnumerable<PropertyGuardDto> propertyGuards = _propertyGuardService.GetPropertyGuards();
            return Ok(propertyGuards);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get property guards");

            return BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Error")
                .WithDetail("Failed to load property guards")
                .Build());
        }
    }
}
