# Contributing to DotNetWind

Thanks for your interest in improving DotNetWind.

## Workflow

- All changes land through a pull request from a topic branch, reviewed before merge — direct
  pushes to `main` aren't accepted. This is currently enforced by convention only; GitHub branch
  protection isn't turned on yet (see [docs/BACKLOG.md](docs/BACKLOG.md#github-repo-hygiene)).
- Branch names follow `<type>/<short-description>`, e.g. `fix/repair-backup`,
  `docs/architecture`.
- Keep PRs focused: one logical change per PR is easier to review than a bundle of unrelated fixes.
- Every merge to `main` that touches `src/`, `Directory.Build.props`/`.targets`, or the embedded
  README/icon automatically publishes a new version to NuGet.org (see
  [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md#versioning--publishing)) — there's no separate
  "cut a release" step. Make sure what you're merging is actually ready to ship.

## Building and testing locally

```bash
dotnet restore DotNetWind.slnx
dotnet build DotNetWind.slnx -c Release
dotnet test DotNetWind.slnx -c Release
dotnet pack src/DotNetWind.Cli/DotNetWind.Cli.csproj -c Release
```

Before opening a PR that changes CLI behavior, run the manual smoke-test matrix described in
[docs/maintenance-plan.md](docs/maintenance-plan.md) — most command paths have no automated
coverage yet (see [docs/BACKLOG.md](docs/BACKLOG.md#test-coverage)), so this is currently the
real safety net, not a formality.

## Project layout and conventions

- **Layering**: `Core ← Templates ← ProjectSystem ← Tailwind ← Cli` — see
  [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for what belongs where and why.
- **Coding standards, the `Result` pattern, testing expectations**: see
  [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md).
- **Adding a new CLI command**: use the `add-command` skill (`.claude/skills/add-command/`) if
  you're working with Claude Code, or follow `src/DotNetWind.Cli/Commands/RepairCommand.cs` as a
  reference implementation — it's small and touches every layer once.

## Commit messages and changelog

Describe *why* a change was made, not just what changed. There's no separate `CHANGELOG.md` in
this repo currently — the PR description and commit history are the record.

## Reporting bugs and requesting features

Use the issue templates when opening a GitHub issue. For security vulnerabilities, see
[SECURITY.md](SECURITY.md) instead of filing a public issue.
