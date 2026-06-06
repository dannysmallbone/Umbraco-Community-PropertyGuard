# Phase 2 Remainder — Plan

## Context

Phase 2 UI is largely complete (commit `fee416a`). Two items remain before Phase 2 is done and Phase 3 can begin.

## Tasks

- [ ] Add `PropertyGuardMode` enum with global default + per-guard override
- [ ] Fix workspace context to apply view vs. write guard based on permissions

---

## Task 1 — PropertyGuardMode enum

### What to build

Add a `PropertyGuardMode` enum to control how a guard is enforced:

```csharp
public enum PropertyGuardMode
{
    WriteOnly,  // property visible, not editable (write guard applied)
    ReadOnly,   // property hidden entirely (view guard applied)
    Both,       // both write and view guards applied
}
```

**Why:** Currently the permissions array (`["Read"]` vs `[]`) drives this, but a named enum is clearer for consumers of `IPropertyGuardDefinition` and for the backoffice UI. The permissions array remains as the internal representation; `PropertyGuardMode` is the ergonomic API.

### Files to change

- **New:** `PropertyGuard/Core/PropertyGuardMode.cs` — enum definition
- **Modify:** `PropertyGuard/Core/PropertyGuardEntry.cs` — add `Mode` property, derive from permissions if not set explicitly
- **Modify:** `PropertyGuard/Dtos/PropertyGuardDto.cs` — add `mode` field
- **Modify:** `PropertyGuard/Configuration/` — add `GlobalPropertyGuardMode` to options (default: `WriteOnly`)
- **Modify:** `PropertyGuard/Client/src/sections/views/propertyguard-section-view.element.ts` — show mode in guard list rows

### Acceptance

- `PropertyGuardMode.WriteOnly` → write guard applied (property read-only)
- `PropertyGuardMode.ReadOnly` → view guard applied (property hidden)
- `PropertyGuardMode.Both` → both guards applied
- Global default in options is respected when no per-guard mode is set
- Mode visible in section view UI

---

## Task 2 — Workspace context permissions-aware guard application

### What to fix

`PropertyGuard/Client/src/workspaces/propertyguard-workspace-context.ts` currently applies an unconditional write guard regardless of the guard's permissions. It should branch on the permissions array:

```typescript
const permissions = propertyGuard.permissions ?? ['Read'];
if (!permissions.includes('Read')) {
  this.#documentWorkspaceContext.propertyViewGuard.addRule({
    unique: `propertyguard-view-${propertyGuard.propertyAlias}`,
    schemaType: 'Element',
    contentTypeAlias: propertyGuard.contentTypeAlias,
    propertyAlias: propertyGuard.propertyAlias,
  });
}
if (!permissions.includes('Write')) {
  this.#documentWorkspaceContext.propertyWriteGuard.addRule({
    unique: `propertyguard-write-${propertyGuard.propertyAlias}`,
    schemaType: 'Element',
    contentTypeAlias: propertyGuard.contentTypeAlias,
    propertyAlias: propertyGuard.propertyAlias,
  });
}
```

### Files to change

- **Modify:** `PropertyGuard/Client/src/workspaces/propertyguard-workspace-context.ts` (or wherever the workspace context lives — check `workspace-contexts/` too)

### Acceptance

- Guard with `permissions: ["Read"]` → property read-only in backoffice
- Guard with `permissions: []` → property hidden in backoffice
- Guard with `permissions: ["Read", "Write"]` → property fully editable (no guard applied)
