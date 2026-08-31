# Architecture

## Project layout and dependency direction

```
Core  ←  Templates  ←  ProjectSystem  ←  Tailwind  ←  Cli
```

Each project only references the ones to its left (verified against every `.csproj`'s
`<ProjectReference>` — there are no back-references anywhere):

| Project | References | Responsibility |
|---|---|---|
| `DotNetWind.Core` | *(none)* | Models (`Result`, `ProjectInfo`, `DotNetProjectType`, ...), abstractions (`IProcessRunner`, `IFileSystem`, ...), and the use cases (`InitUseCase`, `BuildUseCase`, ...) that orchestrate them. Pure logic, no I/O implementations. |
| `DotNetWind.Templates` | Core | The actual CSS/`package.json`/MSBuild target content that gets written into a project. |
| `DotNetWind.ProjectSystem` | Core, Templates | Detects project type (`ProjectDetector`), finds the host file to inject a CSS reference into (`HostFileDetector`), and mutates `.csproj` (`ProjectFileUpdater`). |
| `DotNetWind.Tailwind` | Core, Templates, ProjectSystem | Everything Tailwind/npm-specific: `TailwindInitializer` (the `init` orchestration), `TailwindRunner` (build/watch), `PackageJsonManager`, `NodeJsInstaller`, `DoctorService`, and the actual process-shelling implementation (`ProcessRunner`). |
| `DotNetWind.Cli` | Core, ProjectSystem, Tailwind | The packed tool itself: `System.CommandLine` command definitions (`Commands/*.cs`), `Spectre.Console`-based output (`IConsoleOutput`/`SpectreConsoleOutput`), and DI wiring (`ServiceCollectionExtensions`). |

Keep it this way. A dependency pointing right-to-left (e.g. `Core` referencing `Cli`, or
`Templates` referencing `ProjectSystem`) would be a genuine architecture regression — there's no
automated test enforcing this today (see [DEVELOPMENT.md](DEVELOPMENT.md) for what's untested),
so it's on code review to catch.

## Command flow

Every CLI command follows the same shape:

```
Program.cs registers Commands ──▶ Command.Create(services) builds a System.CommandLine Command
                                          │
                                          ▼
                              command.SetAction(...) resolves a UseCase from DI,
                              builds an options record from parsed arguments,
                              calls useCase.ExecuteAsync(options, ct)
                                          │
                                          ▼
                              UseCase (Core) orchestrates ProjectSystem/Tailwind
                              abstractions, returns a Result / Result<T>
                                          │
                                          ▼
                              Command maps Result.IsFailure → console.WriteError(...)
                              + ToExitCode(result.ErrorKind), or IsSuccess → console.WriteSuccess(...)
```

Concretely: `RepairCommand.Create` → `RepairUseCase.ExecuteAsync` → (delegates to)
`InitUseCase.ExecuteAsync` → `TailwindInitializer.InitializeAsync`, which calls into
`ProjectFileUpdater`, `PackageJsonManager`, `NodeJsInstaller`, `TailwindRunner` in sequence. See
`src/DotNetWind.Cli/Commands/RepairCommand.cs` for a complete, representative example of the
command layer, and use the `add-command` skill (`.claude/skills/add-command/`) when adding a new
one so the pattern stays consistent.

## The `Result` pattern

`src/DotNetWind.Core/Models/Result.cs` defines a railway-oriented `Result`/`Result<T>` — use
cases return this instead of throwing for expected failure modes (missing dependency, validation
error, unsupported project type, user cancellation). `ResultErrorKind` classifies the failure so
the CLI layer can map it to a specific process exit code (`ExitCode.cs`). This is why process
failures show up as clear one-line messages instead of stack traces in almost every path (see
[SECURITY.md](../SECURITY.md) for the one gap in this: raw file I/O calls like
`File.WriteAllTextAsync` have no surrounding `try/catch`, so an `IOException`/
`UnauthorizedAccessException` there isn't currently converted to a `Result.Failure` - whatever
happens next depends on `System.CommandLine`'s own top-level exception handling, not this
codebase).

**Known gap, tracked in [BACKLOG.md](BACKLOG.md):** `Result` is a pure outcome type with no
compensating-action/rollback support. A use case that writes two files in sequence (most of them
do) has no way to undo the first write if the second fails.

## Versioning & publishing

Package version is `Major.Minor.0`, computed automatically from the count of `Merge pull
request #N` commits reachable from `HEAD` (`Directory.Build.props`) — one increment per PR
merged into `main`, not per commit. `Minor` cycles 0-9 and rolls into an automatic `Major` +1
every 10 merges, keeping every published version strictly increasing. `Major`'s base value is
bumped by hand only for a real breaking-change milestone.

Publishing (`.github/workflows/publish.yml`) triggers automatically on every merge to `main` that
touches `src/`, `Directory.Build.props`/`.targets`, or the embedded `README.md`/`icon.png` — a
docs-only or test-only merge is skipped. A manual `workflow_dispatch` run always publishes
regardless, as an explicit override. Publishing uses NuGet Trusted Publishing (OIDC via
`NuGet/login@v1`) — no stored API key.

## Cross-platform behavior

File mutation (`.csproj`, `package.json`, CSS) is pure .NET I/O and works identically on every
platform. Process invocation differs by design: on Windows, commands route through `cmd.exe /c`
(needed to resolve `npm`/`npx`'s `.cmd` shims); on Linux/macOS, the target executable runs
directly with no shell (`ProcessRunner.BuildStartInfo`,
`src/DotNetWind.Tailwind/Infrastructure/ProcessRunner.cs:104-130`) - non-Windows is actually the
simpler and safer of the two paths. Auto-installing Node.js only exists on Windows (via winget);
on other platforms, a missing Node/npm surfaces as a clear `MissingDependency` failure telling
the user to install it manually, never an unhandled crash.
