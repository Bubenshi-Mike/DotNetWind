# CLAUDE.md

Guidance for Claude Code sessions working in this repo. Read this first; it's a map, not a
replacement for the docs it points to.

## What this is

DotNetWind is a global .NET CLI tool (`dotnetwind`) that sets up Tailwind CSS in a .NET web
project: detects project type, writes `package.json`/CSS files, adds an MSBuild target to the
`.csproj`, and can install Node.js on Windows. Published to NuGet.org as a `dotnet tool`.

## Before making changes

- **Architecture**: [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) — the layering
  (`Core ← Templates ← ProjectSystem ← Tailwind ← Cli`), command flow, and the `Result` pattern.
  Read this before touching anything outside a single file — the layering is real and enforced
  by convention (no automated test catches a violation yet).
- **Coding conventions**: [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md) — the `Result`-not-exception
  pattern, `IConsoleOutput`-not-`Console.Write` rule, structured-parser-not-regex rule for file
  mutation, and testing expectations.
- **Known gaps**: [docs/BACKLOG.md](docs/BACKLOG.md) — a grounded, file-and-line-level audit of
  what's actually missing (no rollback/restore, thin test coverage on most command paths, no
  path-containment check, `cmd.exe` string-concatenation on Windows process invocation). Check
  this before assuming something works a certain way — several gaps here are easy to miss by
  reading code casually (e.g. `.dotnetwind.bak` looks like a general safety net but is currently
  write-only).
- **Security posture**: [SECURITY.md](SECURITY.md) documents the same known limitations
  transparently, from a user-facing angle.

## Adding a new CLI command

Use the `add-command` skill (`.claude/skills/add-command/`) — it walks the
Command → UseCase → Abstraction pattern with the exact files to touch, based on
`src/DotNetWind.Cli/Commands/RepairCommand.cs` as the reference shape.

## Before merging to main

Every merge to `main` that touches `src/`, `Directory.Build.props`/`.targets`, or the embedded
`README.md`/`assets/icon.png` **automatically publishes a new version to NuGet.org** — there is
no separate release step, no review gate beyond normal PR review, and no branch protection
enforcing that CI even passed first (tracked in `docs/BACKLOG.md`). Treat anything landing on
`main` as effectively shipping immediately. The `release-checklist` skill
(`.claude/skills/release-checklist/`) runs the pre-merge verification that substitutes for
automated coverage on most command paths.

## Repo-wide conventions carried over from the sibling Syntra project

This user maintains another repo (Syntra) with the same versioning scheme
(`Major.Minor.0`, auto-computed from PR-merge count) and the same auto-publish-on-merge model, by
deliberate choice — consistency across repos, not coincidence. Commit conventions to match:

- **Never add a `Co-Authored-By: Claude` trailer to commits in this user's repos.** This is a
  standing preference across all their projects, not specific to this one.
