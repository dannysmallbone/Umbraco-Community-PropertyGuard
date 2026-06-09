using Microsoft.Extensions.Options;
using PropertyGuard.Configuration;
using PropertyGuard.Core;
using PropertyGuard.Definitions;
using PropertyGuard.DependencyInjection;
using PropertyGuard.Dtos;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace PropertyGuard.NotificationHandlers;

public class PropertyGuardStartupNotificationHandler(
    IPropertyGuardRegistry propertyGuardRegistry,
    PropertyGuardDefinitionCollection propertyGuardDefinitions,
    IOptionsMonitor<PropertyGuardSettings> propertyGuardSettings)
    : INotificationHandler<UmbracoApplicationStartedNotification>
{
    private readonly IPropertyGuardRegistry _propertyGuardRegistry = propertyGuardRegistry;
    private readonly IEnumerable<IPropertyGuardDefinition> _propertyGuardDefinitions = propertyGuardDefinitions;
    private readonly IOptionsMonitor<PropertyGuardSettings> _propertyGuardSettings = propertyGuardSettings;

    public void Handle(UmbracoApplicationStartedNotification notification)
    {
        foreach (IPropertyGuardDefinition definition in _propertyGuardDefinitions)
        {
            definition.DefineMaps(_propertyGuardRegistry);
        }

        List<PropertyGuardDto> propertyGuards = _propertyGuardSettings.CurrentValue.Definitions;

        IEnumerable<IGrouping<string, PropertyGuardDto>> grouped = propertyGuards.GroupBy(x => x.DocumentTypeAlias, StringComparer.OrdinalIgnoreCase);

        foreach (IGrouping<string, PropertyGuardDto> group in grouped)
        {
            foreach (PropertyGuardDto propertyGuard in group)
            {
                propertyGuard.Source = PropertyGuardSource.Config;
                _propertyGuardRegistry.RegisterGuard(propertyGuard);
            }
        }
    }
}
