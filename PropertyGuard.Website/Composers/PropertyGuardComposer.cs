using Umbraco.Cms.Core.Composing;

namespace PropertyGuard.Website.Composers;

public class PropertyGuardComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Composer Example

        //builder.AddPropertyGuard();
        //builder.AddPropertyGuard<HomePropertyGuardDefinition>();
        //builder.WithCollectionBuilder<PropertyGuardDefinitionCollectionBuilder>()
        //    .Add<HomePropertyGuardDefinition>();
    }
}
