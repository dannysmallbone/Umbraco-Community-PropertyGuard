using FluentAssertions;
using NSubstitute;
using PropertyGuard.Core;
using PropertyGuard.NotificationHandlers;
using PropertyGuard.Tests.Helpers;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace PropertyGuard.Tests.NotificationHandlers;

public class PropertyGuardSavingNotificationHandlerTests
{
    private readonly PropertyGuardRegistry _registry = new();
    private readonly IContentService _contentService = Substitute.For<IContentService>();
    private readonly IContentTypeService _contentTypeService = Substitute.For<IContentTypeService>();

    private PropertyGuardSavingNotificationHandler Handler() => new(_registry, _contentService, _contentTypeService);

    private static IProperty Property(string alias, object? invariantValue = null, Dictionary<string, object?>? cultureValues = null)
    {
        IPropertyType propertyType = Substitute.For<IPropertyType>();
        propertyType.Name.Returns(alias);

        IProperty property = Substitute.For<IProperty>();
        property.Alias.Returns(alias);
        property.PropertyType.Returns(propertyType);
        property.GetValue().Returns(invariantValue);

        if (cultureValues is not null)
        {
            foreach (KeyValuePair<string, object?> entry in cultureValues)
            {
                property.GetValue(entry.Key).Returns(entry.Value);
            }
        }

        return property;
    }

    private static IPropertyCollection CollectionOf(params IProperty[] properties)
    {
        IPropertyCollection collection = Substitute.For<IPropertyCollection>();
        collection.GetEnumerator().Returns(_ => ((IEnumerable<IProperty>)properties).GetEnumerator());
        return collection;
    }

    private static IContent ContentWith(string docTypeAlias, int id, params IProperty[] properties) =>
        ContentWith(docTypeAlias, id, [], properties);

    private static IContent ContentWith(string docTypeAlias, int id, string[] cultures, params IProperty[] properties)
    {
        ISimpleContentType contentType = Substitute.For<ISimpleContentType>();
        contentType.Alias.Returns(docTypeAlias);

        IPropertyCollection props = CollectionOf(properties);

        IContent content = Substitute.For<IContent>();
        content.Id.Returns(id);
        content.ContentType.Returns(contentType);
        content.Properties.Returns(props);
        content.AvailableCultures.Returns(cultures);
        return content;
    }

    private static IContentType WithCompositions(params string[] compositionAliases)
    {
        List<IContentTypeComposition> comps = [];
        foreach (string alias in compositionAliases)
        {
            IContentTypeComposition comp = Substitute.For<IContentTypeComposition>();
            comp.Alias.Returns(alias);
            comps.Add(comp);
        }

        IContentType contentType = Substitute.For<IContentType>();
        contentType.ContentTypeComposition.Returns([.. comps]);
        return contentType;
    }

    [Fact]
    public void Handle_GuardedPropertyValueChanged_CancelsOperation()
    {
        _registry.RegisterGuard(Fakes.UiGuard("page", "title"));
        IContentType pageType = WithCompositions();
        _contentTypeService.Get("page").Returns(pageType);

        IContent entity = ContentWith("page", id: 1, Property("title", invariantValue: "new value"));
        IContent original = ContentWith("page", id: 1, Property("title", invariantValue: "original value"));
        _contentService.GetById(1).Returns(original);

        ContentSavingNotification notification = new(entity, new EventMessages());

        Handler().Handle(notification);

        notification.Cancel.Should().BeTrue();
    }

    [Fact]
    public void Handle_GuardedPropertyValueUnchanged_DoesNotCancelOperation()
    {
        _registry.RegisterGuard(Fakes.UiGuard("page", "title"));
        IContentType pageType = WithCompositions();
        _contentTypeService.Get("page").Returns(pageType);

        IContent entity = ContentWith("page", id: 1, Property("title", invariantValue: "same value"));
        IContent original = ContentWith("page", id: 1, Property("title", invariantValue: "same value"));
        _contentService.GetById(1).Returns(original);

        ContentSavingNotification notification = new(entity, new EventMessages());

        Handler().Handle(notification);

        notification.Cancel.Should().BeFalse();
    }

    [Fact]
    public void Handle_UnguardedPropertyChanged_DoesNotCancelOperation()
    {
        _registry.RegisterGuard(Fakes.UiGuard("page", "title"));
        IContentType pageType = WithCompositions();
        _contentTypeService.Get("page").Returns(pageType);

        IContent entity = ContentWith("page", id: 1, Property("body", invariantValue: "new value"));
        IContent original = ContentWith("page", id: 1, Property("body", invariantValue: "original value"));
        _contentService.GetById(1).Returns(original);

        ContentSavingNotification notification = new(entity, new EventMessages());

        Handler().Handle(notification);

        notification.Cancel.Should().BeFalse();
    }

    [Fact]
    public void Handle_DocTypeWithNoGuards_IsSkipped()
    {
        IContent entity = ContentWith("page", id: 1, Property("title", invariantValue: "new value"));

        ContentSavingNotification notification = new(entity, new EventMessages());

        Handler().Handle(notification);

        notification.Cancel.Should().BeFalse();
        _contentService.DidNotReceive().GetById(Arg.Any<int>());
    }

    [Fact]
    public void Handle_NewEntity_IsNotBlocked()
    {
        _registry.RegisterGuard(Fakes.UiGuard("page", "title"));
        IContentType pageType = WithCompositions();
        _contentTypeService.Get("page").Returns(pageType);

        IContent entity = ContentWith("page", id: 0, Property("title", invariantValue: "value"));

        ContentSavingNotification notification = new(entity, new EventMessages());

        Handler().Handle(notification);

        notification.Cancel.Should().BeFalse();
        _contentService.DidNotReceive().GetById(Arg.Any<int>());
    }

    [Fact]
    public void Handle_GuardOnCompositionAlias_CancelsOperation()
    {
        _registry.RegisterGuard(Fakes.UiGuard("seoBase", "metaTitle"));
        IContentType homePageType = WithCompositions("seoBase");
        _contentTypeService.Get("homePage").Returns(homePageType);

        IContent entity = ContentWith("homePage", id: 1, Property("metaTitle", invariantValue: "new value"));
        IContent original = ContentWith("homePage", id: 1, Property("metaTitle", invariantValue: "original value"));
        _contentService.GetById(1).Returns(original);

        ContentSavingNotification notification = new(entity, new EventMessages());

        Handler().Handle(notification);

        notification.Cancel.Should().BeTrue();
    }

    [Fact]
    public void Handle_GuardedCultureVariantPropertyChanged_CancelsOperation()
    {
        _registry.RegisterGuard(Fakes.UiGuard("page", "title"));
        IContentType pageType = WithCompositions();
        _contentTypeService.Get("page").Returns(pageType);

        IContent entity = ContentWith("page", id: 1, ["en-US"], Property("title", cultureValues: new() { ["en-US"] = "new" }));
        IContent original = ContentWith("page", id: 1, ["en-US"], Property("title", cultureValues: new() { ["en-US"] = "original" }));
        _contentService.GetById(1).Returns(original);

        ContentSavingNotification notification = new(entity, new EventMessages());

        Handler().Handle(notification);

        notification.Cancel.Should().BeTrue();
    }

    [Fact]
    public void Handle_GuardedCultureVariantPropertyUnchanged_DoesNotCancelOperation()
    {
        _registry.RegisterGuard(Fakes.UiGuard("page", "title"));
        IContentType pageType = WithCompositions();
        _contentTypeService.Get("page").Returns(pageType);

        IContent entity = ContentWith("page", id: 1, ["en-US"], Property("title", cultureValues: new() { ["en-US"] = "same" }));
        IContent original = ContentWith("page", id: 1, ["en-US"], Property("title", cultureValues: new() { ["en-US"] = "same" }));
        _contentService.GetById(1).Returns(original);

        ContentSavingNotification notification = new(entity, new EventMessages());

        Handler().Handle(notification);

        notification.Cancel.Should().BeFalse();
    }
}
