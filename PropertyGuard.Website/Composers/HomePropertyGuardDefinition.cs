using PropertyGuard.Core;
using PropertyGuard.Definitions;
using PropertyGuard.Dtos;
using PropertyGuard.Extensions;

namespace PropertyGuard.Website.Composers;

public class HomePropertyGuardDefinition : IPropertyGuardDefinition
{
    public void DefineMaps(IPropertyGuardRegistry propertyGuardRegistry)
    {
        // Pattern 1: RegisterGuard with a PropertyGuardDto — full control over all fields
        propertyGuardRegistry.RegisterGuard(new PropertyGuardDto
        {
            DocumentTypeAlias = "home",
            PropertyAlias = "heroHeader",
            FeatureKey = "Hero.Content",
            Message = "Locked via code (DTO)",
            Mode = PropertyGuardMode.ReadOnly,
        });

        // Pattern 2: IPropertyGuardMap — good for registering multiple properties at once
        IPropertyGuardMap map = new PropertyGuardMap()
            .Add("heroDescription", featureKey: "Hero.Content", message: "Locked via code (map)", mode: PropertyGuardMode.Hidden);

        propertyGuardRegistry.RegisterGuard("home", map);

        // Pattern 3: CreateGuard fluent extension — concise, chains off the registry directly
        propertyGuardRegistry
            .CreateGuard("home")
            .RegisterProperty("heroCtalink", featureKey: "Hero.Actions");
    }
}
