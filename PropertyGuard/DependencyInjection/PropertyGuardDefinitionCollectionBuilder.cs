using PropertyGuard.Definitions;
using Umbraco.Cms.Core.Composing;

namespace PropertyGuard.DependencyInjection;

public class PropertyGuardDefinitionCollectionBuilder
    : SetCollectionBuilderBase<PropertyGuardDefinitionCollectionBuilder, PropertyGuardDefinitionCollection, IPropertyGuardDefinition>
{
    protected override PropertyGuardDefinitionCollectionBuilder This => this;
}

public class PropertyGuardDefinitionCollection(Func<IEnumerable<IPropertyGuardDefinition>> items)
    : BuilderCollectionBase<IPropertyGuardDefinition>(items)
{
}
