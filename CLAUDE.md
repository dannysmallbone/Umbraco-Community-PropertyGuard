# Umbraco Community PropertyGuard

Personal open-source Umbraco community package. MIT licence. Published as `Umbraco.Community.PropertyGuard` on NuGet. Personal GitHub under `dannysmallbone` — stays here (not moving to `bifroststudiosuk`).

## Before doing anything

1. Read `f:\Bifrost-Studios\.bifrost\README.md` for orientation.
2. Read `f:\Bifrost-Studios\.bifrost\current-state.md` to see the current stage.
3. Read `f:\Bifrost-Studios\.bifrost\conventions.md` before writing any code.
4. Check `f:\Bifrost-Studios\.bifrost\decisions\` for relevant architecture decisions.
5. Read `ROADMAP.md` at repo root for this project's phase status.

> The `.bifrost/` knowledge base lives in `f:\Bifrost-Studios\` and governs all projects including this one. Read it; never edit it from this repo.

## This project

- **Package:** `Umbraco.Community.PropertyGuard`
- **Runtime:** .NET 10 / Umbraco 17+
- **Source:** `dannysmallbone/Umbraco-Community-PropertyGuard` (GitHub, personal — not bifroststudiosuk)
- **Local dev:** `https://localhost:44303` — SQLite, auto-installs on first run
- **Login:** see `PropertyGuard.Website/appsettings.Local.json`
- **Current branch:** `v17/dev`

## Core rules

- **Never commit directly to `main`.** Every change via a feature branch and PR.
- **Conventional commits, short:** `feat: add property-guard-mode enum`
- **No AI authorship trailers.** Never append `Co-Authored-By: Claude ...` or similar to commit messages.
- **Verify before committing:** `dotnet build` passes with zero warnings.
- **TypeScript API client in `Client/src/api/` is auto-generated — never edit manually.**
  Regenerate with `npm run generate-client <swagger-url>` after adding backend endpoints.
- **Feature specs live in `docs/features/`.** ADRs live in `docs/decisions/`.

## Current state (as of 2026-06-06)

Phase 1 (backend enforcement): **done and committed**
Phase 2 (UI — local state): **largely done** (commit `fee416a`)
- Built: inline add-feature/group, source tracking, remove/edit, copy-config, filter/search, workspace context, footer app
- Remaining: `PropertyGuardMode` enum + workspace context permissions-aware guard application
Phase 2 remainder spec: `docs/features/2026-06-phase-2-remainder/`

Phases 3–6: not started. Full plan: `ROADMAP.md`

## Structure

```
Umbraco-Community-PropertyGuard/
├── CLAUDE.md
├── ROADMAP.md
├── PropertyGuard/              → main package (C# + TypeScript)
│   ├── Client/src/             → Lit web components (auto-gen API client in api/)
│   ├── Controllers/            → REST API endpoints
│   ├── Core/                   → registry, entries, source enum
│   ├── Services/               → PropertyGuardService
│   ├── NotificationHandlers/   → saving + startup handlers
│   ├── Definitions/            → IPropertyGuardDefinition
│   ├── Dtos/                   → PropertyGuardDto
│   └── DependencyInjection/    → UmbracoBuilderExtensions
├── PropertyGuard.Website/      → local dev host
└── docs/
    ├── decisions/              → ADRs
    └── features/               → feature specs (one folder per feature, date-prefixed)
```
