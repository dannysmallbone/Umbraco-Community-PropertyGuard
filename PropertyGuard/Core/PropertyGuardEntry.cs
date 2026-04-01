namespace PropertyGuard.Core;

public class PropertyGuardEntry(string? featureKey, string? message = null)
{
    public string FeatureKey { get; init; } = featureKey ?? Constants.FeatureKey;

    public string Message { get; init; } = message ?? Constants.GuardMessage;
}
