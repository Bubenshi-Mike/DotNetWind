---
name: release-checklist
description: Run the pre-merge verification checklist for DotNetWind before merging to main. Use before merging any PR that changes src/, since every such merge auto-publishes a new version to NuGet.org with no separate release step and no branch protection gate.
---

# Pre-merge release checklist

Merging to `main` in this repo is the release — `publish.yml` auto-publishes a new NuGet.org
version on any merge touching `src/`, `Directory.Build.props`/`.targets`, or the embedded
README/icon (see `docs/ARCHITECTURE.md#versioning--publishing`). There's no branch protection
requiring CI to pass first (tracked in `docs/BACKLOG.md`), so this checklist is the actual gate
today, not a formality.

## Steps

1. **Clean build and test.**
   ```
   dotnet restore DotNetWind.slnx
   dotnet build DotNetWind.slnx -c Release
   dotnet test DotNetWind.slnx -c Release
   ```
   All 4 test projects should report 0 failures. Note this only covers `init` end-to-end
   (`SmokeTests.cs`) plus unit-level coverage for a subset of use cases — it does **not**
   substitute for the manual smoke test below.

2. **Manual smoke-test matrix**, per `docs/maintenance-plan.md` — run against real temporary
   projects (not just the automated smoke test's skip-npm-install path):
   - `dotnetwind init`, `build`, `doctor`, `clean` against a Blazor Web App, Blazor WebAssembly,
     MVC, and Razor Pages project.
   - `repair` and `uninstall` idempotency: run each twice against a configured temp project and
     confirm the second run doesn't error or corrupt anything.
   - If touching Node/npm-install logic: confirm behavior when Node.js/npm are missing, including
     that `dotnetwind init --yes` actually installs Node.js LTS via `winget` on a clean Windows
     environment.
   - Run `init` twice against the same project and confirm generated `package.json` scripts and
     `.csproj` targets are idempotent (no duplicate entries).

3. **README accuracy check** — confirm the command reference table and every documented
   `--option` in `README.md` still matches `dotnetwind --help` and each command's own
   `--help` output exactly. This drifts easily since there's no automated check tying the two
   together.

4. **Confirm this change is actually ready to ship**, not just ready to merge — there is no
   staging step. If the change is experimental or needs more soak time, keep it on a branch
   rather than merging.

5. **After merging**, verify the publish actually went through:
   ```
   gh run list --workflow "Publish to NuGet" --limit 1
   ```
   Check every step succeeded (especially "Push to NuGet.org" — look for "Your package was
   pushed" in the log), then confirm the new version resolves:
   ```
   dotnet tool update --global DotNetWind
   ```
   NuGet.org's own indexing can lag a few minutes after a successful push before it's visible via
   some endpoints — a successful `Push to NuGet.org` step with a `201 Created` response in the
   log is the authoritative signal, not immediate visibility on nuget.org's website.
