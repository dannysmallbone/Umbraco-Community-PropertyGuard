namespace PropertyGuard.Dtos;

public class PropertyGuardDto
{
    public string PropertyAlias { get; set; } = string.Empty;

    public string ContentTypeAlias { get; set; } = string.Empty;

    public string FeatureKey { get; set; } = "Global.PropertyGuards";

    public string Message { get; set; } = "Property is protected by Property Guard";
}
