using PropertyGuard.Core;
using PropertyGuard.Definitions;
using PropertyGuard.Dtos;
using PropertyGuard.Extensions;

namespace PropertyGuard.Website.Composers;

public class HomePropertyGuardDefinition : IPropertyGuardDefinition
{
    public void DefineMaps(IPropertyGuardRegistry propertyGuardRegistry)
    {
        // Definition Example

        PropertyGuardDto propertyGuard = new()
        {
            DocumentTypeAlias = "home",
            PropertyAlias = "heroHeader",
            Message = "Property protected by code Property Guard"
        };

        propertyGuardRegistry
            .RegisterGuard(propertyGuard)
            .RegisterGuard(new PropertyGuardDto
            {
                DocumentTypeAlias = "home",
                PropertyAlias = "heroDescription"
            });

        IPropertyGuardMap map = new PropertyGuardMap()
            .Add("heroHeader")
            .Add("heroDescription");

        propertyGuardRegistry.RegisterGuard("home", map);

        propertyGuardRegistry
            .CreateGuard("home")
            .RegisterProperty("HeroCtalink");
    }
}
