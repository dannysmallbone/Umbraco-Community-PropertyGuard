# PropertyGuard: Implementation Plan

## Context

PropertyGuard is a standalone Umbraco package for protecting content properties from being edited. The primary user journey is:

1. Developer installs the package with zero config
2. Editor opens the **Guards** section in the backoffice
3. Editor picks a **Feature** (a named group) to add guards under — or creates a new one
4. Editor picks the document type and property to guard
5. Guards are **live immediately** (in-memory registry updated)
6. Editor hits **Save** — guards are persisted to a JSON file
7. On app restart the JSON file is loaded and guards are restored

Code/config registration (via `IPropertyGuardDefinition` or `appsettings.json`) remains supported as an alternative for developers who prefer it. The UI story is the zero-friction path.

### Feature keys in PropertyGuard

`FeatureKey` is a **first-class organisational concept** within PropertyGuard — not just metadata for external packages. It lets users group guards into named features (e.g. `"Pricing"`, `"SensitiveData"`) and sub-groups (e.g. `"Pricing.Basic"`, `"Pricing.Advanced"`).

The format is `"Feature.Group"` using dot notation. The backoffice section view uses this to render a two-level sidebar + tabs layout. Guards with no explicit feature key use the default `"PropertyGuards.General"`.

When **FeatureGuard** is installed, these same feature key strings map to FeatureGuard's DB-backed feature definitions — enabling the richer toggle/per-node behaviour. PropertyGuard itself has no enable/disable logic for features; that is FeatureGuard's job.

FeatureGuard integration is **out of scope for this plan** — tracked separately in the FeatureGuard project.

### Section access

The Guards section is already hidden from users whose user group does not include `PropertyGuard.Section`. On install, only the Administrators group sees it. Admins can grant access to other groups via **Users → User Groups** in the backoffice — no extra configuration needed.

---

## Status

| Phase | Item | Status |
|---|---|---|
| 1 | Backend enforcement (ContentSavingNotification handler) | ✅ Done |
| 2 | UI — layout, inline add flow, remove, Save (local state only) | ✅ Done |
| 3 | Backend — RemoveGuard + JSON persistence + management API endpoints | Pending |
| 4 | Wire up — connect UI to Phase 3 API endpoints | Pending |
| 5 | Tests & CI/CD | Pending |
| 6 | README | Pending |

---

## Phase 2 — UI: Local state (no API calls yet)

**File: `PropertyGuard/Client/src/sections/views/propertyguard-section-view.element.ts`**

### Layout overview

The section view always renders the full sidebar + content layout — even on day 0 with no guards. The current code branches to a plain text `#renderNoPropertyGuards()` when empty; this should be replaced so the sidebar and its add button are always present.

```
┌─────────────────────────────────────────────────────┐
│  Property Guards                         [Save]     │
│ ┌─────────────────┬──────────────────────────────┐  │
│ │  Features       │  [General]  [Advanced]  [+]  │  │
│ │ ─────────────── │                              │  │
│ │  Pricing        │  • Content: Title      [x]   │  │
│ │  Content        │  • Content: Summary    [x]   │  │
│ │ ─────────────── │                              │  │
│ │  [+ Add feature]│                              │  │
│ └─────────────────┴──────────────────────────────┘  │
└─────────────────────────────────────────────────────┘
```

**Day 0 (no guards yet):**
```
┌─────────────────────────────────────────────────────┐
│  Property Guards                                    │
│ ┌─────────────────┬──────────────────────────────┐  │
│ │                 │  No guards have been added.  │  │
│ │                 │  Pick a feature to get       │  │
│ │                 │  started.                    │  │
│ │  [+ Add feature]│                              │  │
│ └─────────────────┴──────────────────────────────┘  │
└─────────────────────────────────────────────────────┘
```

### Add flow — fully inline, three steps

No intermediate dialog for naming. Each step uses an inline text input directly in the UI. The property picker only opens at step 3.

#### Step 1 — Add a feature (sidebar inline input)

- A `[+ Add feature]` button sits at the bottom of the sidebar, always visible
- Clicking it **replaces the button with an inline `<input>`** in the sidebar
- User types a feature name and presses **Enter** → feature added to sidebar, immediately selected
- Pressing **Escape** cancels without adding anything
- State: `_addingFeature: boolean` toggles the input; `_pendingFeatureName: string` holds the typed value
- A newly added feature auto-selects a default `"General"` group so the user can skip Step 2 for the simple case

#### Step 2 — Add a group (tab-bar inline input)

- A `[+]` button sits at the end of the tab bar, visible when a feature is selected
- Clicking it **replaces the `[+]` with an inline `<input>`** in the tab bar
- User types a group name and presses **Enter** → new tab created and selected
- Pressing **Escape** cancels
- State: `_addingGroup: boolean` toggles the input; `_pendingGroupName: string` holds the value
- If `"General"` is sufficient, the user skips this step entirely

#### Step 3 — Add a property guard (property picker)

- The `[+ Add property guard]` button in the guard list opens **directly** to `UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_FLOW_MODAL` — no pre-dialog
- The `FeatureKey` is composed from the currently selected feature + group: `"Feature.Group"`
- User picks document type + property
- On confirm → add to local `_propertyGuards` state with `source: "ui"` (no API call yet — wired up in Phase 4)
- Sidebar and tabs update reactively from local state

#### Complete day-0 walkthrough

```
1. User clicks [+ Add feature] in sidebar
   → inline input appears in sidebar
   → types "Pricing", presses Enter
   → "Pricing" appears in sidebar (selected), "General" tab auto-created

2. (Optional) User clicks [+] in tab bar
   → inline input appears in tab bar
   → types "Advanced", presses Enter
   → "Advanced" tab created and selected

3. User clicks [+ Add property guard] in the guard list
   → UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_FLOW_MODAL opens
   → picks "Home Page" → "Hero Header" property
   → guard added to local state with featureKey "Pricing.Advanced"

4. User clicks [Save]
   → (Phase 2: local state only — Save button visible but disabled/no-op until Phase 4 wiring)
```

> Feature keys are derived purely from guards — an empty feature/group created in steps 1–2 exists only in local UI state. If the user navigates away without adding a guard, it disappears. Nothing is persisted until Phase 4.

### Remove guard flow (local state only)

- Each guard with `source: "ui"` in local state has a remove (`x`) button
- `"code"` and `"config"` source guards show a lock badge, no remove button
- On click → remove from `_propertyGuards` local state (no API call yet — wired up in Phase 4)

### Save button (stub)

- Positioned top-right of the `uui-box` headline area
- Rendered but disabled/no-op in Phase 2 — fully wired in Phase 4
- Shows unsaved-changes indicator (badge dot) when `_propertyGuards` contains any `source: "ui"` entries

### Source field (local only in Phase 2)

- `PropertyGuardDto` gets a `source` field: `"ui"` | `"config"` | `"code"`
- In Phase 2 all new guards added via the UI get `source: "ui"` in local state
- Backend source tagging (`PropertyGuardStartupNotificationHandler`) is part of Phase 3

---

## Phase 3 — Backend: Guard removal + JSON persistence

### 3a — Add `Remove` to `IPropertyGuardRegistry`

**Modify: `PropertyGuard/Core/IPropertyGuardRegistry.cs`**
- Add `IPropertyGuardRegistry RemoveGuard(string documentTypeAlias, string propertyAlias)`

**Modify: `PropertyGuard/Core/PropertyGuardRegistry.cs`**
- Implement `RemoveGuard` — removes the property alias from the document type's map; if the map becomes empty, remove the document type entry entirely

### 3b — JSON persistence layer

Stores UI-added guards to `App_Data/PropertyGuard/guards.json`. Code/appsettings guards are not written here.

**New: `PropertyGuard/Persistence/IPropertyGuardStore.cs`**
```
IReadOnlyList<PropertyGuardDto> Load();
void Save(IEnumerable<PropertyGuardDto> guards);
```

**New: `PropertyGuard/Persistence/JsonPropertyGuardStore.cs`**
- Resolves path via `IWebHostEnvironment.ContentRootPath` + `"App_Data/PropertyGuard/guards.json"`
- `Load()` — deserialises from file; returns empty list if file does not exist
- `Save(guards)` — serialises to file using `System.Text.Json`; creates directories if needed

**Modify: `PropertyGuard/DependencyInjection/UmbracoBuilderExtensions.cs`**
- Register `IPropertyGuardStore` → `JsonPropertyGuardStore` (singleton)

**Modify: `PropertyGuard/NotificationHandlers/PropertyGuardStartupNotificationHandler.cs`**
- After loading code + appsettings guards, also load from `IPropertyGuardStore` and register each
- Order: code → appsettings → JSON store (additive — JSON store cannot override others)
- Tag each guard with its source: `"code"`, `"config"`, or `"ui"`

### 3c — Management API endpoints

**Modify: `PropertyGuard/Controllers/PropertyGuardApiController.cs`**

Add three new endpoints alongside the existing GET endpoints:

- `POST /AddGuard` — body: `PropertyGuardDto` → validates `FeatureKey` is non-empty → calls `registry.RegisterGuard(dto)` → returns updated `List<PropertyGuardDto>`
- `DELETE /RemoveGuard` — query params: `documentTypeAlias`, `propertyAlias` → calls `registry.RemoveGuard(...)` → returns updated `List<PropertyGuardDto>`
- `POST /SaveGuards` — no body → reads UI-managed guards, calls `store.Save(...)` → returns `200 OK`

> `SaveGuards` only writes guards where `Source == "ui"`.

**Regenerate TypeScript client after adding these endpoints.**

### 3d — Fix API authorization

Currently `PropertyGuardApiControllerBase` uses `AuthorizationPolicies.SectionAccessContent` — this allows any user with Content section access to call the API regardless of Guards section access.

**Modify: `PropertyGuard/DependencyInjection/UmbracoBuilderExtensions.cs`**
- Register a custom authorization policy `"PropertyGuardSectionAccess"` that checks the user has the `PropertyGuard.Section` section assigned

**Modify: `PropertyGuard/Controllers/PropertyGuardApiControllerBase.cs`**
- Change `[Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]` to `[Authorize(Policy = "PropertyGuardSectionAccess")]`

---

## Phase 4 — Wire up UI to backend

After Phase 3 is complete, swap local state mutations for real API calls.

**File: `PropertyGuard/Client/src/sections/views/propertyguard-section-view.element.ts`**

- Regenerate TypeScript API client (`npm run generate-client <swagger-url>`) after Phase 3 adds the new endpoints
- `#addPropertyGuard()` confirm → call `POST /AddGuard`; update local state from response
- Remove `[x]` click → call `DELETE /RemoveGuard`; remove from local state on success
- Save button → call `POST /SaveGuards`; show success/failure notification via `UMB_NOTIFICATION_CONTEXT`
- Load initial guards from `GET /PropertyGuards` on context initialisation

---

## Phase 5 — Tests & CI/CD

### Unit tests (new project `PropertyGuard.Tests`)

- `PropertyGuardRegistry` — `RegisterGuard` merge; `RemoveGuard` removes correctly; case-insensitive lookups
- `PropertyGuardSavingNotificationHandler` — dirty guarded property reverts; non-guarded property untouched; no-guard doc type skipped
- `JsonPropertyGuardStore` — round-trip save/load; missing file returns empty list; directories created on save

### CI/CD (`.github/workflows/`)

**`build-and-test.yml`** — on push to `v17/dev` and PRs targeting `main`:
1. `dotnet restore && dotnet build && dotnet test`
2. `cd PropertyGuard/Client && npm ci && npm run build`

**`publish.yml`** — on tag push `v*.*.*`:
1. Build + test
2. `dotnet pack PropertyGuard/PropertyGuard.csproj`
3. `dotnet nuget push` (secret: `NUGET_API_KEY`)

---

## Phase 6 — README

`README.md` at repo root covering:
1. What it does (one paragraph)
2. Requirements (Umbraco v17+)
3. Installation: `dotnet add package Umbraco.Community.PropertyGuard`
4. Register in `Program.cs`: `.AddPropertyGuard()`
5. **UI path** — open the Guards section, name a feature, add properties, hit Save
6. **Code path** — implement `IPropertyGuardDefinition` example
7. **Config path** — `appsettings.json` example with `FeatureKey` shown
8. **Feature keys explained** — what they are, the `"Feature.Group"` format, and how FeatureGuard builds on them
9. **Section access** — how to grant non-admin user groups access via User Groups in the backoffice

---

## Critical Files

| File | Role |
|---|---|
| [PropertyGuard/Client/src/sections/views/propertyguard-section-view.element.ts](PropertyGuard/Client/src/sections/views/propertyguard-section-view.element.ts) | Phase 2 — full UI rewrite (local state) |
| [PropertyGuard/Core/IPropertyGuardRegistry.cs](PropertyGuard/Core/IPropertyGuardRegistry.cs) | Phase 3 — add `RemoveGuard` method |
| [PropertyGuard/Core/PropertyGuardRegistry.cs](PropertyGuard/Core/PropertyGuardRegistry.cs) | Phase 3 — implement `RemoveGuard` |
| [PropertyGuard/NotificationHandlers/PropertyGuardStartupNotificationHandler.cs](PropertyGuard/NotificationHandlers/PropertyGuardStartupNotificationHandler.cs) | Phase 3 — load from JSON store; tag guards with source |
| [PropertyGuard/Controllers/PropertyGuardApiController.cs](PropertyGuard/Controllers/PropertyGuardApiController.cs) | Phase 3 — add `AddGuard`, `RemoveGuard`, `SaveGuards` endpoints |
| [PropertyGuard/Controllers/PropertyGuardApiControllerBase.cs](PropertyGuard/Controllers/PropertyGuardApiControllerBase.cs) | Phase 3 — fix authorization policy |
| [PropertyGuard/DependencyInjection/UmbracoBuilderExtensions.cs](PropertyGuard/DependencyInjection/UmbracoBuilderExtensions.cs) | Phase 3 — register store + auth policy |
| `PropertyGuard/Persistence/IPropertyGuardStore.cs` | Phase 3 — persistence interface (new) |
| `PropertyGuard/Persistence/JsonPropertyGuardStore.cs` | Phase 3 — JSON file implementation (new) |
