# Phase 2 Remainder — Validation

## Setup

Start the local dev site: `https://localhost:44303`
Log in: `dannysmallbone@gmail.com` / `password321`

Register a test guard in code (or appsettings) targeting a known content type and property.

---

## PropertyGuardMode validation

1. Register a guard with `Mode = PropertyGuardMode.WriteOnly` (or rely on default)
   - Open a document of that content type
   - Expected: the property is visible but the editor is disabled/read-only
   - Expected: the property can be read but saving a change to it is blocked

2. Register a guard with `Mode = PropertyGuardMode.ReadOnly`
   - Open the same document
   - Expected: the property is not visible at all in the backoffice

3. Register a guard with `Mode = PropertyGuardMode.Both`
   - Open the same document
   - Expected: the property is not visible (view guard takes precedence)

4. Set `GlobalPropertyGuardMode = ReadOnly` in appsettings; register a guard with no explicit mode
   - Expected: property hidden (global default applied)

5. Open the Guards section
   - Expected: the mode is shown on each guard row in the section view

---

## Workspace context permissions validation

1. Register a guard with `permissions: ["Read"]`
   - Open a document of that content type in the backoffice
   - Expected: the guarded property is visible but read-only (editor disabled)

2. Register a guard with `permissions: []`
   - Open the same document
   - Expected: the guarded property is not visible

3. Register a guard with `permissions: ["Read", "Write"]`
   - Open the same document
   - Expected: the property is fully editable — no guard applied

4. Register two guards on the same property — one `["Read"]`, one `[]`
   - Expected: most restrictive wins — property hidden (view guard)
