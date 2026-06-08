# Phase 5 — Unit Tests: Requirements

## Project

- A `PropertyGuard.Tests` xUnit project exists at `PropertyGuard.Tests/PropertyGuard.Tests.csproj`
- Project references `PropertyGuard/PropertyGuard.csproj`
- Test files mirror source folder structure (`Core/`, `Services/`, `NotificationHandlers/`)
- A shared `Helpers/Fakes.cs` provides factory methods for `PropertyGuardDto` using Bogus
- NuGet dependencies: `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `NSubstitute`, `FluentAssertions`, `Bogus`, `Microsoft.Extensions.Caching.Memory`
- `IMemoryCache` is not mocked — a real `MemoryCache` instance is used in service tests

---

## PropertyGuardRegistry

- `RegisterGuard(dto)` with a new doc type alias creates an entry with the property stored
- `RegisterGuard(dto)` with an existing doc type alias merges the new property without replacing existing properties
- `RegisterGuard(dto)` is case-insensitive on `DocumentTypeAlias`
- `RegisterGuard(map)` with a new doc type stores the entire map
- `RegisterGuard(map)` with an existing doc type merges all properties from the new map
- `RemoveGuard` removes the named property; doc type entry is kept if other properties remain
- `RemoveGuard` removes the doc type entry when its last property is removed
- `RemoveGuard` with an unknown doc type does not throw
- `GetPropertyGuards` returns null for an unregistered doc type
- `GetPropertyGuards` is case-insensitive on `documentTypeAlias`
- `GetAllPropertyGuards` returns a flat dictionary covering all registered doc types and properties

---

## PropertyGuardMap

- `Add(alias, entry)` stores the entry
- `Add(alias, entry)` with a duplicate alias overwrites the existing entry
- `Add` with a null or empty alias is a no-op and does not throw
- `Add(dto)` derives `PropertyGuardEntry` fields correctly from the DTO (`FeatureKey`, `Message`, `Source`, `Mode`)
- `Remove` with an existing alias removes the entry
- `Remove` with an unknown alias is a no-op and does not throw

---

## PropertyGuardService

### GetPropertyGuards(string)

- Null or whitespace alias returns empty without calling the registry
- A doc type with no registered guards returns empty
- A doc type with guards returns DTOs enriched with display names, icon, and unique keys from `IContentTypeService`
- Calling `GetPropertyGuards` twice with the same alias results in exactly one registry call (second is a cache hit)
- An exception during retrieval is caught, logged, and returns empty — it does not propagate

### ApplyGuards

- UI guards in the submitted list are registered in the registry
- UI guards currently in the registry but absent from the submission are removed
- Guards with `Source == Code` in the submission are ignored — not registered
- Guards with `Source == Config` in the submission are ignored — not registered
- Code and config guards already in the registry are not touched
- After `ApplyGuards`, calling `GetPropertyGuards` for an affected doc type results in a cache miss (registry re-queried)
- After `ApplyGuards`, calling `GetPropertyGuards()` (all) results in a cache miss

---

## PropertyGuardSavingNotificationHandler

- When a property is dirty and guarded: `SetValue` is called with the current value, `ResetDirtyProperties` is called, and an error `EventMessage` is added to the notification
- When a property is dirty but not guarded: the property is not touched and no message is added
- When a property is guarded but not dirty: the property is not touched and no message is added
- When the doc type has no guards: the entity is skipped without inspecting any properties
- When multiple guarded properties on one entity are dirty: all are reverted and a single message listing all property names is added
- When a notification contains multiple entities: each entity is handled independently

---

## General

- `dotnet test` passes with zero failures
- `dotnet build` passes with zero warnings
- No test references Umbraco's database, HTTP pipeline, or any hosted service
