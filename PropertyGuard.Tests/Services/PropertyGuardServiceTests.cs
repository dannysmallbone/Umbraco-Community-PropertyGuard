using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PropertyGuard.Core;
using PropertyGuard.Services;
using PropertyGuard.Tests.Helpers;
using Umbraco.Cms.Core.Services;

namespace PropertyGuard.Tests.Services;

public class PropertyGuardServiceTests
{
    private static PropertyGuardService BuildService(IPropertyGuardRegistry registry) =>
        new(
            Substitute.For<ILogger<PropertyGuardService>>(),
            registry,
            new MemoryCache(new MemoryCacheOptions()),
            Substitute.For<IContentTypeService>()
        );

    [Fact]
    public void GetPropertyGuards_CacheMiss_ReturnsGuardsFromRegistry()
    {
        var registry = new PropertyGuardRegistry();
        registry.RegisterGuard(Fakes.UiGuard("content", "title"));
        PropertyGuardService service = BuildService(registry);

        IEnumerable<PropertyGuard.Dtos.PropertyGuardDto> result = service.GetPropertyGuards("content");

        result.Should().ContainSingle(g => g.PropertyAlias == "title");
    }

    [Fact]
    public void GetPropertyGuards_CacheHit_ReturnsCachedResult()
    {
        var registry = new PropertyGuardRegistry();
        registry.RegisterGuard(Fakes.UiGuard("content", "title"));
        PropertyGuardService service = BuildService(registry);
        service.GetPropertyGuards("content");

        registry.RemoveGuard("content", "title");
        IEnumerable<PropertyGuard.Dtos.PropertyGuardDto> result = service.GetPropertyGuards("content");

        result.Should().ContainSingle(g => g.PropertyAlias == "title");
    }

    [Fact]
    public void ApplyGuards_NewUiGuards_RegistersInRegistry()
    {
        var registry = new PropertyGuardRegistry();
        PropertyGuardService service = BuildService(registry);

        service.ApplyGuards([Fakes.UiGuard("content", "title")]);

        registry.GetPropertyGuards("content").Should().ContainKey("title");
    }

    [Fact]
    public void ApplyGuards_StaleUiGuards_RemovedFromRegistry()
    {
        var registry = new PropertyGuardRegistry();
        registry.RegisterGuard(Fakes.UiGuard("content", "stale"));
        PropertyGuardService service = BuildService(registry);

        service.ApplyGuards([]);

        registry.GetPropertyGuards("content").Should().BeNull();
    }

    [Fact]
    public void ApplyGuards_CodeGuards_ArePreserved()
    {
        var registry = new PropertyGuardRegistry();
        registry.RegisterGuard(Fakes.CodeGuard("content", "title"));
        PropertyGuardService service = BuildService(registry);

        service.ApplyGuards([]);

        registry.GetPropertyGuards("content").Should().ContainKey("title");
    }

    [Fact]
    public void ApplyGuards_InvalidatesAffectedDocTypeCache()
    {
        var registry = new PropertyGuardRegistry();
        registry.RegisterGuard(Fakes.UiGuard("content", "title"));
        PropertyGuardService service = BuildService(registry);
        service.GetPropertyGuards("content");

        service.ApplyGuards([]);

        service.GetPropertyGuards("content").Should().BeEmpty();
    }
}
