using FluentAssertions;
using PropertyGuard.Core;
using PropertyGuard.Tests.Helpers;

namespace PropertyGuard.Tests.Core;

public class PropertyGuardRegistryTests
{
    [Fact]
    public void RegisterGuard_NewDocType_CreatesEntry()
    {
        PropertyGuardRegistry registry = new();

        registry.RegisterGuard(Fakes.UiGuard("content", "title"));

        registry.GetPropertyGuards("content").Should().ContainKey("title");
    }

    [Fact]
    public void RegisterGuard_ExistingDocType_MergesProperty()
    {
        PropertyGuardRegistry registry = new();
        registry.RegisterGuard(Fakes.UiGuard("content", "title"));

        registry.RegisterGuard(Fakes.UiGuard("content", "summary"));

        registry.GetPropertyGuards("content").Should().HaveCount(2)
            .And.ContainKey("title")
            .And.ContainKey("summary");
    }

    [Fact]
    public void GetPropertyGuards_DifferentCase_ReturnsEntry()
    {
        PropertyGuardRegistry registry = new();
        registry.RegisterGuard(Fakes.UiGuard("HomePage", "title"));

        IReadOnlyDictionary<string, PropertyGuardEntry>? result = registry.GetPropertyGuards("homepage");

        result.Should().ContainKey("title");
    }

    [Fact]
    public void RemoveGuard_LastProperty_RemovesDocTypeEntry()
    {
        PropertyGuardRegistry registry = new();
        registry.RegisterGuard(Fakes.UiGuard("content", "title"));

        registry.RemoveGuard("content", "title");

        registry.GetPropertyGuards("content").Should().BeNull();
    }
}
