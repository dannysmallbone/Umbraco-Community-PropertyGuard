# Phase 2 Remainder — Requirements

## PropertyGuardMode enum

- A `PropertyGuardMode` enum exists in the C# package with values `WriteOnly`, `ReadOnly`, `Both`
- A global default mode can be set in `appsettings.json` under the PropertyGuard config section
- Individual guards can override the global default via a `Mode` property on their definition
- The DTO exposes `mode` so the backoffice UI can display it
- `WriteOnly` (default) = write guard only — property visible but not editable
- `ReadOnly` = view guard only — property hidden
- `Both` = both guards applied

## Workspace context permissions

- Guards with `permissions: ["Read"]` → `propertyWriteGuard.addRule(...)` applied (write guard)
- Guards with `permissions: []` → `propertyViewGuard.addRule(...)` applied (view guard)
- Guards with `permissions: ["Read", "Write"]` → no guard applied (FeatureGuard override state)
- Each rule uses a stable `unique` key incorporating the property alias to prevent duplicates
