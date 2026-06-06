namespace PropertyGuard.Core;

public class PropertyGuardEntry(string? featureKey, string? message = null, PropertyGuardSource source = PropertyGuardSource.Code)
{
    public string FeatureKey { get; init; } = featureKey ?? Constants.FeatureKey;

    public string Message { get; init; } = message ?? Constants.GuardMessage;

    public PropertyGuardSource Source { get; init; } = source;
}
