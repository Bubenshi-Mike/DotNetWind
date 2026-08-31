# Maintenance Backlog

Working list of known gaps, grounded in an actual code-level audit (not general advice). Update
this as items get resolved or new ones surface — this is the list to look at before starting a
maintenance pass, not a permanent record of history.

## Safety (highest priority — this tool mutates users' project files)

- **No rollback and no restore.** `TailwindInitializer.InitializeAsync` writes `package.json`
  before `.csproj`; if the `.csproj` write fails after `package.json` already succeeded, the
  project is left half-configured with nothing to undo it
  (`src/DotNetWind.Tailwind/TailwindInitializer.cs:31-94`). Same shape of problem in
  `UninstallUseCase.ExecuteAsync` (`src/DotNetWind.Core/UseCases/UninstallUseCase.cs:25-62`).
  **Why it matters:** a failed `init`/`repair`/`uninstall` can leave a real project in a broken,
  inconsistent state with no automated way back.
  **How to apply:** either add a snapshot-and-restore wrapper around the mutation sequence, or at
  minimum make `.dotnetwind.bak` backups happen (and be restorable) for `init`/`repair`/`update`
  the same way `uninstall` already does — see the next item.
- **`.dotnetwind.bak` is write-only.** `uninstall` is the only command that creates a backup
  (`UninstallUseCase.cs:64-72`, gated by `UninstallOptions.Backup`), and nothing anywhere reads
  one back. There is no `dotnetwind restore` command. `repair`/`update` never back up at all and
  expose no `--backup` option (contrast `UninstallCommand.cs:34-37`), even though a user who's
  used `uninstall` would reasonably expect the same safety net elsewhere.
  **How to apply:** add backup-before-mutate to `repair`/`update` (they already delegate to
  `InitUseCase` — see `RepairUseCase.cs`/`UpdateUseCase.cs`), and add an actual restore path that
  reads `.dotnetwind.bak` back — even a manual "rename this file back" instruction in the error
  message would be better than the current silence.
- **`PackageJsonManager` silently discards non-object `scripts`/`devDependencies`.** If an
  existing `package.json` has `"scripts": "something-not-an-object"` (unusual but valid JSON),
  `AddScripts`/`AddDevDependencies` replace it with a fresh object with no warning
  (`src/DotNetWind.Tailwind/PackageJsonManager.cs:123-129,146-152`). Untested
  (`PackageJsonManagerTests.cs` only covers well-formed inputs).
  **How to apply:** detect the non-object case and fail with a clear message instead of silently
  overwriting, or at minimum log a warning.
- **No path containment check on `--input`/`--output`.** Every use case
  (`InitUseCase.cs:38-39`, `BuildUseCase.cs:29-30`, `WatchUseCase.cs:29-30`,
  `UninstallUseCase.cs:35-36`, `DoctorService.cs:80,85`, `InfoUseCase.cs:52-53`) resolves these
  via plain `Path.Combine`, which doesn't strip `..` segments — a crafted `--output ../../..`
  writes outside the project directory. Low real-world severity (local tool, user already
  controls the input) but genuinely unvalidated anywhere.
  **How to apply:** resolve the combined path with `Path.GetFullPath` and verify it starts with
  the project directory before writing/deleting.

## Security posture (not currently exploitable, but fragile)

- **Windows process invocation shells through `cmd.exe /c "<cmd> <args>"` via string
  concatenation**, not `ProcessStartInfo.ArgumentList`
  (`src/DotNetWind.Tailwind/Infrastructure/ProcessRunner.cs:104-130`). Every current call site
  passes fixed literal arguments (verified — no project-derived or user-supplied string reaches
  it today), so there's no live injection vector right now. But `IProcessRunner.RunAsync`/
  `RunStreamingAsync` (`src/DotNetWind.Core/Abstractions/IProcessRunner.cs:5-17`) takes
  `arguments` as one opaque string, so the next feature that builds a command line from a path or
  script name inherits shell-metacharacter risk with no compiler signal that it's unsafe.
  **How to apply:** before any feature passes a dynamic value through `IProcessRunner`, either
  switch the API to take an argument array, or resolve `.cmd` shims directly instead of routing
  through `cmd.exe`.

## Test coverage

- Zero automated coverage (unit or integration) for `repair`, `update`, `doctor`, `build`,
  `watch`, `clean`, `info` command paths, and no dedicated test for `InitUseCase` itself. Only
  `init` has an end-to-end smoke test (`tests/DotNetWind.Cli.Tests/SmokeTests.cs`), and even that
  runs with `--skip-npm-install --skip-build`, so it never exercises the real npm/build path.
- `ProcessRunner` — the class that actually shells out to npm/npx/node/winget — has no test file
  at all, on any platform. The `cmd.exe` wrapping logic specifically (see above) is untested.
- `docs/maintenance-plan.md`'s "smoke-test on a clean Windows environment" step is manual-only;
  there's no automated equivalent for the winget/Node-install path.

## Documentation

- Zero XML doc comments anywhere in `src/` — every public interface in
  `src/DotNetWind.Core/Abstractions/` is undocumented at the API level.
- `DotNetProjectType.MauiHybrid` (`src/DotNetWind.Core/Models/DotNetProjectType.cs:11`) is
  defined but unreachable — `ProjectDetector.DetectProjectType` never returns it, and it's not in
  `InitCommand`'s accepted `--framework` values either. Either implement detection for it or
  remove the dead enum member.
- README's Requirements section only mentions the Windows/winget auto-install path; the
  non-Windows fallback (works, just requires Node.js already on `PATH`) isn't stated explicitly.

## Minor

- `TailwindRunner.WatchAsync` (`src/DotNetWind.Tailwind/TailwindRunner.cs:51-52`) calls
  `Console.WriteLine`/`Console.Error.WriteLine` directly instead of going through
  `IConsoleOutput` like every other output path — a small layering leak (bypasses Spectre
  formatting, harder to test the same way as everything else).
- `DoctorCommand.cs:63`'s `--json` output also bypasses `IConsoleOutput` via a direct
  `Console.WriteLine` — smaller issue since it's still in the `Cli` layer, just inconsistent with
  the rest of the file.

## GitHub repo hygiene

- No branch protection on `main` — the PR-only workflow is convention, not enforced.
- Dependabot vulnerability alerts and security updates are both disabled; no `.github/dependabot.yml`.
- No `CODEOWNERS`.
