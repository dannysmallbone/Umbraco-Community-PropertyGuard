using Bogus;
using PropertyGuard.Core;
using PropertyGuard.Dtos;

namespace PropertyGuard.Tests.Helpers;

internal static class Fakes
{
    private static readonly Faker s_faker = new();

    public static PropertyGuardDto UiGuard(string? documentTypeAlias = null, string? propertyAlias = null) => new()
    {
        DocumentTypeAlias = documentTypeAlias ?? s_faker.Lorem.Word(),
        PropertyAlias = propertyAlias ?? s_faker.Lorem.Word(),
        Source = PropertyGuardSource.Ui,
    };

    public static PropertyGuardDto CodeGuard(string? documentTypeAlias = null, string? propertyAlias = null) => new()
    {
        DocumentTypeAlias = documentTypeAlias ?? s_faker.Lorem.Word(),
        PropertyAlias = propertyAlias ?? s_faker.Lorem.Word(),
        Source = PropertyGuardSource.Code,
    };
}
