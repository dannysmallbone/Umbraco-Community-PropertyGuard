using Bogus;
using PropertyGuard.Core;
using PropertyGuard.Dtos;

namespace PropertyGuard.Tests.Helpers;

internal static class Fakes
{
    private static readonly Faker _faker = new();

    public static PropertyGuardDto UiGuard(string? documentTypeAlias = null, string? propertyAlias = null) => new()
    {
        DocumentTypeAlias = documentTypeAlias ?? _faker.Lorem.Word(),
        PropertyAlias = propertyAlias ?? _faker.Lorem.Word(),
        Source = PropertyGuardSource.Ui,
    };

    public static PropertyGuardDto CodeGuard(string? documentTypeAlias = null, string? propertyAlias = null) => new()
    {
        DocumentTypeAlias = documentTypeAlias ?? _faker.Lorem.Word(),
        PropertyAlias = propertyAlias ?? _faker.Lorem.Word(),
        Source = PropertyGuardSource.Code,
    };
}
