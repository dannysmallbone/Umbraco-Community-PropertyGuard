using FluentAssertions;
using NSubstitute;
using PropertyGuard.Core;
using PropertyGuard.NotificationHandlers;
using PropertyGuard.Tests.Helpers;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;

namespace PropertyGuard.Tests.NotificationHandlers;

public class PropertyGuardSavingNotificationHandlerTests
{
    private static IPropertyCollection PropertiesFrom(params IProperty[] properties)
    {
        IPropertyCollection collection = Substitute.For<IPropertyCollection>();
        collection.GetEnumerator().Returns(_ => ((IEnumerable<IProperty>)properties).GetEnumerator());
        return collection;
    }

    private static (IContent content, IProperty property) BuildContent(
        string docTypeAlias,
        string propertyAlias,
        bool isDirty)
    {
        IPropertyType propertyType = Substitute.For<IPropertyType>();
        propertyType.Name.Returns(propertyAlias);

        IProperty property = Substitute.For<IProperty>();
        property.Alias.Returns(propertyAlias);
        property.IsDirty().Returns(isDirty);
        property.GetValue().Returns("original");
        property.PropertyType.Returns(propertyType);

        ISimpleContentType contentType = Substitute.For<ISimpleContentType>();
        contentType.Alias.Returns(docTypeAlias);

        IPropertyCollection props = PropertiesFrom(property);

        IContent content = Substitute.For<IContent>();
        content.ContentType.Returns(contentType);
        content.Properties.Returns(props);

        return (content, property);
    }

    [Fact]
    public void Handle_GuardedDirtyProperty_IsReverted()
    {
        PropertyGuardRegistry registry = new();
        registry.RegisterGuard(Fakes.UiGuard("content", "title"));
        PropertyGuardSavingNotificationHandler handler = new(registry);
        (IContent content, IProperty property) = BuildContent("content", "title", isDirty: true);
        EventMessages messages = new();
        ContentSavingNotification notification = new(content, messages);

        handler.Handle(notification);

        property.Received().ResetDirtyProperties();
        messages.GetAll().Should().ContainSingle(m => m.MessageType == EventMessageType.Error);
    }

    [Fact]
    public void Handle_UnguardedDirtyProperty_IsNotReverted()
    {
        PropertyGuardRegistry registry = new();
        registry.RegisterGuard(Fakes.UiGuard("content", "title"));
        PropertyGuardSavingNotificationHandler handler = new(registry);
        (IContent content, IProperty property) = BuildContent("content", "body", isDirty: true);
        ContentSavingNotification notification = new(content, new EventMessages());

        handler.Handle(notification);

        property.DidNotReceive().ResetDirtyProperties();
        notification.Messages.GetAll().Should().BeEmpty();
    }

    [Fact]
    public void Handle_DocTypeWithNoGuards_IsSkipped()
    {
        PropertyGuardRegistry registry = new();
        PropertyGuardSavingNotificationHandler handler = new(registry);
        (IContent content, IProperty property) = BuildContent("content", "title", isDirty: true);
        ContentSavingNotification notification = new(content, new EventMessages());

        handler.Handle(notification);

        property.DidNotReceive().ResetDirtyProperties();
        notification.Messages.GetAll().Should().BeEmpty();
    }
}
