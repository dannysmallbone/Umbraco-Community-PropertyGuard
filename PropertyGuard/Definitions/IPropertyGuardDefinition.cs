using PropertyGuard.Core;

namespace PropertyGuard.Definitions;

public interface IPropertyGuardDefinition
{
    void DefineMaps(IPropertyGuardRegistry propertyGuardRegistry);
}
