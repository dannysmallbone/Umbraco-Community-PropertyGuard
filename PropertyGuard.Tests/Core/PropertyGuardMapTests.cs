using FluentAssertions;
using PropertyGuard.Core;
using PropertyGuard.Tests.Helpers;

namespace PropertyGuard.Tests.Core;

public class PropertyGuardMapTests
{
    [Fact]
    public void Add_NewAlias_AppearsInGuards()
    {
        PropertyGuardMap map = new();

        map.Add(Fakes.UiGuard(propertyAlias: "title"));

        map.Guards.Should().ContainKey("title");
    }

    [Fact]
    public void Add_ExistingAlias_Overwrites()
    {
        PropertyGuardMap map = new();
        map.Add(Fakes.UiGuard(propertyAlias: "title"));

        map.Add(Fakes.UiGuard(propertyAlias: "title"));

        map.Guards.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Add_NullOrWhitespaceAlias_DoesNothing(string? alias)
    {
        PropertyGuardMap map = new();

        map.Add(alias!, new PropertyGuardEntry(null));

        map.Guards.Should().BeEmpty();
    }

    [Fact]
    public void Remove_ExistingAlias_RemovesGuard()
    {
        PropertyGuardMap map = new();
        map.Add(Fakes.UiGuard(propertyAlias: "title"));

        map.Remove("title");

        map.Guards.Should().BeEmpty();
    }
}
