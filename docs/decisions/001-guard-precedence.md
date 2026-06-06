# ADR 001 — Guard Precedence

**Date:** 2026-06-06
**Status:** Accepted

## Decision

All registered guards always apply. When multiple guards target the same property, the most restrictive wins. There is no override mechanism within PropertyGuard.

## Rationale

Predictable and safe. The outcome of any set of guards is always knowable — the most restrictive permission on the property is what the user experiences. Removing a restriction requires deleting or disabling the guard, not adding a counter-guard.

This keeps PropertyGuard's scope tight: it is a restriction system, not a permission resolution system.

## Consequences

- Conditional override logic (per-client, per-tier, per-feature state) is **FeatureGuard's responsibility**, not PropertyGuard's. FeatureGuard controls *what is in* PropertyGuard's registry at any given time; PropertyGuard enforces whatever is registered.
- Anyone who needs "lift a restriction for a specific context" must use FeatureGuard.
- `FeatureKey` within PropertyGuard is **organisational** (UI sidebar grouping, dot-notation hierarchy) — it does not create precedence or override logic.
- `permissions: ["Read", "Write"]` is a valid guard state used by FeatureGuard to represent "no restriction active" for a property it manages. PropertyGuard treats this as a no-op.

## Permissions model

| `permissions` value | Effect |
|---|---|
| `["Read"]` | Write guard — property visible but not editable |
| `[]` | View guard — property hidden entirely |
| `["Read", "Write"]` | No guard — property fully editable (used by FeatureGuard overrides) |
