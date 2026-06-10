using PropertyGuard.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace PropertyGuard.NotificationHandlers;

public class PropertyGuardSavingNotificationHandler(
    IPropertyGuardRegistry propertyGuardRegistry,
    IContentService contentService,
    IContentTypeService contentTypeService)
    : INotificationHandler<ContentSavingNotification>
{
    public void Handle(ContentSavingNotification notification)
    {
        foreach (IContent entity in notification.SavedEntities)
        {
            Dictionary<string, PropertyGuardEntry> guards = BuildGuardMap(entity.ContentType.Alias);

            if (guards.Count == 0) continue;

            IContent? original = entity.Id > 0 ? contentService.GetById(entity.Id) : null;

            if (original is null) continue;

            List<string> blocked = [];

            foreach (IProperty property in entity.Properties)
            {
                if (!guards.ContainsKey(property.Alias)) continue;

                IProperty? originalProperty = original.Properties.FirstOrDefault(p => p.Alias.Equals(property.Alias, StringComparison.OrdinalIgnoreCase));

                if (ValuesMatch(property, originalProperty, entity.AvailableCultures)) continue;

                blocked.Add(property.PropertyType.Name);
            }

            if (blocked.Count == 0) continue;

            notification.CancelOperation(new EventMessage(
                "Property Guard",
                $"The following properties are protected and cannot be changed: {string.Join(", ", blocked)}",
                EventMessageType.Error));
        }
    }

    private Dictionary<string, PropertyGuardEntry> BuildGuardMap(string contentTypeAlias)
    {
        Dictionary<string, PropertyGuardEntry> result = new(StringComparer.OrdinalIgnoreCase);

        Merge(contentTypeAlias);

        IContentType? fullType = contentTypeService.Get(contentTypeAlias);
        if (fullType is not null)
        {
            foreach (IContentTypeComposition composition in fullType.ContentTypeComposition)
            {
                Merge(composition.Alias);
            }
        }

        return result;

        void Merge(string alias)
        {
            IReadOnlyDictionary<string, PropertyGuardEntry>? guards = propertyGuardRegistry.GetPropertyGuards(alias);
            if (guards is null) return;
            foreach (KeyValuePair<string, PropertyGuardEntry> kvp in guards)
            {
                result.TryAdd(kvp.Key, kvp.Value);
            }
        }
    }

    private static bool ValuesMatch(IProperty updated, IProperty? original, IEnumerable<string> cultures)
    {
        if (original is null) return true;

        if (!Equals(updated.GetValue(), original.GetValue())) return false;

        foreach (string culture in cultures)
        {
            if (!Equals(updated.GetValue(culture), original.GetValue(culture))) return false;
        }

        return true;
    }
}
