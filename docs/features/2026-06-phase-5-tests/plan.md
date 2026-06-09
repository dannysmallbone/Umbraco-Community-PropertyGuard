# Phase 5 — Unit Tests: Plan

## Context

Phases 1–4 are complete. The package has a working backend (enforcement, registry, service, API) and a fully wired backoffice UI. Phase 5 adds a unit test suite before NuGet publication.

There is no database, no persistence layer, and no HTTP middleware that requires a running Umbraco host. Everything meaningful is unit-testable. Integration tests and E2E Playwright tests are explicitly deferred.

---

## Stack

| Concern | Library | Notes |
|---|---|---|
| Framework | xUnit | Standard for .NET community packages |
| Mocking | NSubstitute | Clean modern API — `Substitute.For<T>()`, `.Returns()`, `.Received()` |
| Assertions | FluentAssertions | Dominant assertion library; excellent failure messages |
| Fake data | Bogus | Generates realistic fake strings/GUIDs; replaces hand-rolled helpers |
| Memory cache | `MemoryCache` (real) | Zero external deps — use directly, do not mock `out` parameters |

---

## Task 1 — Create `PropertyGuard.Tests` project

**New file: `PropertyGuard.Tests/PropertyGuard.Tests.csproj`**

- Target `net10.0`
- `ProjectReference` to `PropertyGuard/PropertyGuard.csproj`
- NuGet packages: `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `NSubstitute`, `FluentAssertions`, `Bogus`, `Microsoft.Extensions.Caching.Memory`

**Folder structure:**
```
PropertyGuard.Tests/
├── Core/
│   ├── PropertyGuardRegistryTests.cs
│   └── PropertyGuardMapTests.cs
├── Services/
│   └── PropertyGuardServiceTests.cs
├── NotificationHandlers/
│   └── PropertyGuardSavingNotificationHandlerTests.cs
└── Helpers/
    └── Fakes.cs
```

---

## Task 2 — `Fakes.cs` shared helper

**New file: `PropertyGuard.Tests/Helpers/Fakes.cs`**

Static class with pre-built DTO factory methods using Bogus. Keeps test setup concise.

```csharp
internal static class Fakes
{
    private static readonly Faker _faker = new();

    public static PropertyGuardDto UiGuard(string? docType = null, string? property = null) =>
        new()
        {
            DocumentTypeAlias = docType ?? _faker.Lorem.Slug(),
            PropertyAlias = property ?? _faker.Lorem.Word(),
            Source = PropertyGuardSource.Ui,
            FeatureKey = "General.PropertyGuards",
            Message = _faker.Lorem.Sentence(),
            Mode = PropertyGuardMode.ReadOnly,
        };

    public static PropertyGuardDto CodeGuard(string? docType = null, string? property = null) =>
        UiGuard(docType, property) with { Source = PropertyGuardSource.Code };

    public static PropertyGuardDto ConfigGuard(string? docType = null, string? property = null) =>
        UiGuard(docType, property) with { Source = PropertyGuardSource.Config };
}
```

---

## Task 3 — `PropertyGuardRegistryTests.cs`

No mocking required — `PropertyGuardRegistry` is pure in-memory.

| Test name | Scenario | Expected |
|---|---|---|
| `RegisterGuard_NewDocType_CreatesEntry` | DTO for unknown doc type | Entry created, property stored |
| `RegisterGuard_ExistingDocType_MergesProperty` | DTO for already-registered doc type | Property merged; existing properties untouched |
| `RegisterGuard_IsCaseInsensitive` | Register with `"Content"`, retrieve with `"content"` | Same entry returned |
| `RegisterGuard_WithMap_NewDocType_StoresMap` | Map for unknown doc type | Map stored as-is |
| `RegisterGuard_WithMap_ExistingDocType_MergesAllProperties` | Map for already-registered doc type | All properties from new map merged |
| `RemoveGuard_WhenOtherPropertiesRemain_KeepsDocTypeEntry` | Remove one of two properties | Doc type entry still present |
| `RemoveGuard_WhenLastProperty_RemovesDocTypeEntry` | Remove only remaining property | Doc type entry also removed |
| `RemoveGuard_UnknownDocType_DoesNotThrow` | Doc type not in registry | No exception |
| `GetPropertyGuards_UnregisteredDocType_ReturnsNull` | Alias not registered | Returns null |
| `GetPropertyGuards_IsCaseInsensitive` | Registered as `"Content"`, queried as `"CONTENT"` | Returns guards |
| `GetAllPropertyGuards_MultipleDocTypes_ReturnsFlatDictionary` | Two doc types registered | All properties in result |

---

## Task 4 — `PropertyGuardMapTests.cs`

| Test name | Scenario | Expected |
|---|---|---|
| `Add_NewProperty_StoresEntry` | Add a new alias | `Guards` contains the entry |
| `Add_DuplicateAlias_OverwritesEntry` | Add same alias twice | Second entry wins |
| `Add_NullAlias_IsNoOp` | Null alias passed | No exception; `Guards.Count` unchanged |
| `Add_EmptyAlias_IsNoOp` | Empty string alias | No exception; `Guards.Count` unchanged |
| `Add_FromDto_DerivesEntryCorrectly` | Add via `PropertyGuardDto` | `FeatureKey`, `Message`, `Source`, `Mode` match DTO |
| `Remove_ExistingAlias_RemovesEntry` | Remove known alias | `Guards` no longer contains alias |
| `Remove_UnknownAlias_IsNoOp` | Remove alias not in map | No exception; count unchanged |

---

## Task 5 — `PropertyGuardServiceTests.cs`

Setup per test class:
- `IPropertyGuardRegistry registry = Substitute.For<IPropertyGuardRegistry>()`
- `IContentTypeService contentTypeService = Substitute.For<IContentTypeService>()`
- `ILogger<PropertyGuardService> logger = Substitute.For<ILogger<PropertyGuardService>>()`
- `IMemoryCache cache = new MemoryCache(new MemoryCacheOptions())`
- `PropertyGuardService sut = new(logger, registry, cache, contentTypeService)`

### GetPropertyGuards(string)

| Test name | Scenario | Expected |
|---|---|---|
| `GetPropertyGuards_NullAlias_ReturnsEmpty` | Null alias | Empty result; registry not called |
| `GetPropertyGuards_WhitespaceAlias_ReturnsEmpty` | Whitespace alias | Empty result; registry not called |
| `GetPropertyGuards_NoGuardsRegistered_ReturnsEmpty` | Registry returns empty dict | Empty result |
| `GetPropertyGuards_WithGuards_ReturnsDtos` | Registry returns guards; content type found | DTOs include name, icon, unique from content type |
| `GetPropertyGuards_SecondCall_HitsCacheNotRegistry` | Same alias called twice | Registry called exactly once |
| `GetPropertyGuards_RegistryThrows_ReturnsEmpty` | Registry throws | Empty result; no exception propagated |

### ApplyGuards

| Test name | Scenario | Expected |
|---|---|---|
| `ApplyGuards_NewUiGuards_RegistersInRegistry` | Submit two UI guards | `RegisterGuard` called twice |
| `ApplyGuards_StaleUiGuards_RemovedFromRegistry` | One UI guard in registry, none in submission | `RemoveGuard` called for stale guard |
| `ApplyGuards_CodeGuardsInSubmission_Ignored` | Submission contains code-source guard | `RegisterGuard` not called for it |
| `ApplyGuards_ConfigGuardsInSubmission_Ignored` | Submission contains config-source guard | `RegisterGuard` not called for it |
| `ApplyGuards_CodeGuardsInRegistry_NotTouched` | Registry contains code guard not in submission | `RemoveGuard` not called for it |
| `ApplyGuards_InvalidatesCacheForAffectedDocTypes` | Apply then call `GetPropertyGuards` | Registry called again (cache evicted) |
| `ApplyGuards_InvalidatesAllCache` | Apply then call `GetPropertyGuards()` (all) | Registry called again (all-cache evicted) |

---

## Task 6 — `PropertyGuardSavingNotificationHandlerTests.cs`

`ContentSavingNotification` is constructable directly: `new ContentSavingNotification(entities, new EventMessages())`.  
`IContent` and `IProperty` are Umbraco interfaces — substituted with NSubstitute.

Setup helper:
```csharp
private static IProperty MakeProperty(string alias, string typeName, bool dirty)
{
    var prop = Substitute.For<IProperty>();
    var propType = Substitute.For<IPropertyType>();
    propType.Name.Returns(typeName);
    prop.Alias.Returns(alias);
    prop.IsDirty().Returns(dirty);
    prop.PropertyType.Returns(propType);
    return prop;
}

private static IContent MakeContent(string docTypeAlias, params IProperty[] properties)
{
    var content = Substitute.For<IContent>();
    var contentType = Substitute.For<IContentTypeComposition>();
    contentType.Alias.Returns(docTypeAlias);
    content.ContentType.Returns(contentType);
    content.Properties.Returns(new PropertyCollection(properties));
    return content;
}
```

| Test name | Scenario | Expected |
|---|---|---|
| `Handle_DirtyGuardedProperty_RevertsAndAddsMessage` | Property dirty, alias in guard map | `SetValue` + `ResetDirtyProperties` called; error message added |
| `Handle_DirtyUnguardedProperty_NotTouched` | Property dirty, alias not in guard map | No revert; no message |
| `Handle_GuardedButNotDirtyProperty_NotTouched` | Property guarded but `IsDirty()` false | No revert; no message |
| `Handle_DocTypeWithNoGuards_SkipsEntity` | Registry returns null for doc type | No properties inspected; no message |
| `Handle_MultipleDirtyGuardedProperties_AllReverted` | Two guarded dirty properties | Both reverted; one message listing both names |
| `Handle_MultipleEntities_EachHandledIndependently` | Two entities in notification | Guards checked per-entity; independent messages |

---

## Acceptance

- `dotnet test` from repo root passes with zero failures
- `dotnet build` passes with zero warnings
- No test project references Umbraco's database, HTTP pipeline, or any hosted service
