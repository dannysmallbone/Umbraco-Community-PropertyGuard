using PropertyGuard.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;

namespace PropertyGuard.NotificationHandlers;

public class PropertyGuardSavingNotificationHandler(IPropertyGuardRegistry propertyGuardRegistry)
    : INotificationHandler<ContentSavingNotification>
{
    private readonly IPropertyGuardRegistry _propertyGuardRegistry = propertyGuardRegistry;

    public void Handle(ContentSavingNotification notification)
    {
        foreach (IContent entity in notification.SavedEntities)
        {
            IReadOnlyDictionary<string, PropertyGuardEntry>? guards = _propertyGuardRegistry.GetPropertyGuards(entity.ContentType.Alias);

            if (guards is null || guards.Count == 0) continue;

            List<string> reverted = [];

            foreach (IProperty property in entity.Properties)
            {
                if (!guards.ContainsKey(property.Alias) || !property.IsDirty()) continue;

                object? currentValue = property.GetValue();
                property.SetValue(currentValue);
                property.ResetDirtyProperties();

                reverted.Add(property.PropertyType.Name);
            }

            if (reverted.Count == 0) continue;

            notification.Messages.Add(new EventMessage(
                "Property Guard",
                $"The following properties are protected and cannot be changed: {string.Join(", ", reverted)}",
                EventMessageType.Error));
        }
    }
}
