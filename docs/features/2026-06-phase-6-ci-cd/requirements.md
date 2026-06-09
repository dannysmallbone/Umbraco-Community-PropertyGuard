# Phase 6 — CI/CD: Requirements

## build-and-test.yml

- Workflow exists at `.github/workflows/build-and-test.yml`
- Triggers on push to `v17/dev`
- Triggers on pull requests targeting `main`
- Build step uses `/p:TreatWarningsAsErrors=true` — a compiler warning fails the workflow
- Test step uses `--no-build` — does not rebuild after the build step
- Frontend build step runs `npm ci && npm run build` in `PropertyGuard/Client`
- A failing test prevents the workflow from completing successfully

## publish.yml

- Workflow exists at `.github/workflows/publish.yml`
- Triggers on tag pushes matching `v*.*.*`
- Runs the same build and test steps as `build-and-test.yml` before packing
- Pack step uses `--no-build` and outputs to `./nupkg`
- Push step uses `secrets.NUGET_API_KEY` — does not hard-code credentials
- Publish step does not run if build or test steps fail

## General

- Neither workflow hard-codes secrets or credentials
- `NUGET_API_KEY` secret name is documented so it is not forgotten at first publish
