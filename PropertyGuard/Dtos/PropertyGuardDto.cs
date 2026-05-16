namespace PropertyGuard.Dtos;

public class PropertyGuardDto
{
    public string DocumentTypeAlias { get; set; } = string.Empty;

    public string PropertyAlias { get; set; } = string.Empty;

    public string? DocumentTypeUnique { get; set; }

    public string? PropertyTypeUnique { get; set; }

    public string? DocumentTypeName { get; set; }

    public string? PropertyTypeName { get; set; }

    public string? Icon { get; set; }

    public string FeatureKey { get; set; } = Constants.FeatureKey;

    public string Message { get; set; } = Constants.GuardMessage;

    public List<string> Permissions { get; set; } = ["Read"];
}
