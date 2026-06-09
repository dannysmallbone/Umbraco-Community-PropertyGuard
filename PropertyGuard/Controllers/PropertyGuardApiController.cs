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

    [HttpGet("GetPropertyGuardsByDocumentTypeAlias")]
    [ProducesResponseType<List<PropertyGuardDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public IActionResult GetPropertyGuardsByDocumentTypeAlias(string documentTypeAlias)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(documentTypeAlias);

            IEnumerable<PropertyGuardDto> propertyGuards = _propertyGuardService.GetPropertyGuards(documentTypeAlias);

            return Ok(propertyGuards);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get property guards for {documentTypeAlias}", documentTypeAlias);

            return BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Error")
                .WithDetail("Failed to load property guards")
                .Build());
        }
    }

    [HttpGet("GetPropertyGuardsByDocumentTypeAliases")]
    [ProducesResponseType<List<PropertyGuardDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public IActionResult GetPropertyGuardsByDocumentTypeAliases([FromQuery] string[] documentTypeAliases)
    {
        try
        {
            if (documentTypeAliases == null || documentTypeAliases.Length == 0) return Ok(new List<PropertyGuardDto>());

            IEnumerable<PropertyGuardDto> propertyGuards = _propertyGuardService.GetPropertyGuards(documentTypeAliases);

            return Ok(propertyGuards);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get property guards for {documentTypeAliases}", documentTypeAliases);

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

    [HttpPost("ApplyGuards")]
    [ProducesResponseType<List<PropertyGuardDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public IActionResult ApplyGuards([FromBody] List<PropertyGuardDto> guards)
    {
        try
        {
            IEnumerable<PropertyGuardDto> result = _propertyGuardService.ApplyGuards(guards);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply property guards");

            return BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Error")
                .WithDetail("Failed to apply property guards")
                .Build());
        }
    }
}
