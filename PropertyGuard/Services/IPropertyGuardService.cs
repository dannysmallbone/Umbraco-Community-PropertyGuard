using PropertyGuard.Dtos;

namespace PropertyGuard.Services;

public interface IPropertyGuardService
{
    IEnumerable<PropertyGuardDto> GetPropertyGuards(string documentTypeAlias);
    IEnumerable<PropertyGuardDto> GetPropertyGuards(string[] documentTypeAliases);
    IEnumerable<PropertyGuardDto> GetPropertyGuards();
}
