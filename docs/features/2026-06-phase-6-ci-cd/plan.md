# Phase 6 — CI/CD: Plan

## Context

Phase 5 delivers the unit test suite. Phase 6 wires those tests into GitHub Actions so every push and PR is verified automatically, and tags produce a NuGet package.

---

## Task 1 — `build-and-test.yml`

**New file: `.github/workflows/build-and-test.yml`**

Triggers:
- Push to `v17/dev`
- Pull request targeting `main`

Steps:
1. Checkout
2. Setup .NET 10
3. `dotnet restore`
4. `dotnet build --no-restore --configuration Release /p:TreatWarningsAsErrors=true`
5. `dotnet test --no-build --configuration Release`
6. Setup Node (LTS)
7. `cd PropertyGuard/Client && npm ci && npm run build`

---

## Task 2 — `publish.yml`

**New file: `.github/workflows/publish.yml`**

Trigger: tag push matching `v*.*.*`

Steps:
1. Checkout
2. Setup .NET 10
3. `dotnet restore`
4. `dotnet build --no-restore --configuration Release /p:TreatWarningsAsErrors=true`
5. `dotnet test --no-build --configuration Release`
6. `dotnet pack PropertyGuard/PropertyGuard.csproj --no-build --configuration Release --output ./nupkg`
7. `dotnet nuget push ./nupkg/*.nupkg --source https://api.nuget.org/v3/index.json --api-key ${{ secrets.NUGET_API_KEY }}`

Publish step only runs if build and test succeed — no `continue-on-error`.

---

## Acceptance

- `build-and-test.yml` triggers correctly on a push to `v17/dev` and on a PR to `main`
- A warning in the C# source causes the build step to fail
- A failing test causes the workflow to fail before reaching any pack/push step
- `publish.yml` triggers on a `v*.*.*` tag and produces a `.nupkg` pushed to NuGet
- `NUGET_API_KEY` secret documented in repo (README or `docs/`) so it is not forgotten at publish time
