using PropertyGuard.Dtos;

namespace PropertyGuard.Services;

public interface IPropertyGuardService
{
    IEnumerable<PropertyGuardDto> GetPropertyGuards(string contentTypeAlias);
}