namespace PropertyGuard.Dtos;

public class PropertyGuardDto
{
    public string DocumentTypeAlias { get; set; } = string.Empty;

    public string PropertyAlias { get; set; } = string.Empty;

    public string FeatureKey { get; set; } = Constants.FeatureKey;

    public string Message { get; set; } = Constants.GuardMessage;
}
