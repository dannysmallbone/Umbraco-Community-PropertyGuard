using Umbraco.Cms.Core.Composing;

namespace PropertyGuard.Website.Composers;

public class PropertyGuardComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Option A: single definition class
        //builder.AddPropertyGuard<HomePropertyGuardDefinition>();

        // Option B: multiple definition classes
        // builder.WithCollectionBuilder<PropertyGuardDefinitionCollectionBuilder>()
        //     .Add<HomePropertyGuardDefinition>()
        //     .Add<AnotherPropertyGuardDefinition>();
    }
}
