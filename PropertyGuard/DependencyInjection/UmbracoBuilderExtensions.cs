using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PropertyGuard.Configuration;
using PropertyGuard.Core;
using PropertyGuard.Definitions;
using PropertyGuard.NotificationHandlers;
using PropertyGuard.Services;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace PropertyGuard.DependencyInjection;

public static partial class UmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddPropertyGuard(this IUmbracoBuilder builder)
    {
        builder.Services
            .AddOptions<PropertyGuardSettings>()
            .Bind(builder.Config.GetSection(PropertyGuardSettings.ConfigName));

        builder.Services.TryAddSingleton<IPropertyGuardRegistry, PropertyGuardRegistry>();

        builder.Services.AddScoped<IPropertyGuardService, PropertyGuardService>();

        builder.WithCollectionBuilder<PropertyGuardDefinitionCollectionBuilder>();

        builder.AddNotificationHandler<UmbracoApplicationStartedNotification, PropertyGuardStartupNotificationHandler>();

        return builder;
    }

    public static IUmbracoBuilder AddPropertyGuard<T>(this IUmbracoBuilder builder) where T : class, IPropertyGuardDefinition
    {
        builder.WithCollectionBuilder<PropertyGuardDefinitionCollectionBuilder>()
            .Add<T>();

        return builder;
    }
}
