# Development Guidelines

Coding conventions and standards for this repo — see [ARCHITECTURE.md](ARCHITECTURE.md) for the
project layout these rules assume, and [CONTRIBUTING.md](../CONTRIBUTING.md) for the PR workflow
itself.

## Principles

1. **Fail with a `Result`, not an exception, for anything expected.** Missing dependency, bad
   input, unsupported project type, user cancellation — all of these return
   `Result.Failure(message, errorKind)`. Reserve thrown exceptions for genuinely unexpected
   conditions (a bug, not a user-facing outcome). This is what keeps CLI error output readable
   instead of a stack trace — don't break the pattern for a new command.
2. **All console output goes through `IConsoleOutput`, never `Console.Write*` directly**, outside
   of `Program.cs` itself. `src/DotNetWind.Tailwind/TailwindRunner.cs`'s `WatchAsync` currently
   violates this (calls `Console.WriteLine` directly from the `Tailwind` layer) — that's a known,
   tracked exception, not a precedent to follow. New code should go through the abstraction so
   output stays testable and consistently formatted.
3. **File mutation uses structured parsers, never regex/string surgery on file content.**
   `.csproj` edits go through `System.Xml.Linq`; `package.json` edits go through
   `System.Text.Json.Nodes`. `ProjectFileUpdater` has a string-based fallback for the case where
   existing XML doesn't parse — that's a deliberate degrade path, not a template to copy for new
   features; keep new file-format handling parser-based from the start.
4. **Process invocation only ever uses fixed, literal argument strings today** — nothing
   project-derived or user-supplied is currently interpolated into a command line. If a new
   feature needs to build a command from a dynamic value (a path, a script name), don't just
   string-concatenate it into `IProcessRunner`'s `arguments` parameter — that parameter routes
   through `cmd.exe /c` on Windows (see [ARCHITECTURE.md](ARCHITECTURE.md)) and string
   concatenation into a shell command is exactly the pattern that becomes exploitable once
   dynamic input reaches it. Either widen `IProcessRunner` to take an argument array first, or
   get a second pair of eyes on the change specifically for this.
5. **Respect the dependency direction** (`Core ← Templates ← ProjectSystem ← Tailwind ← Cli`).
   If you find yourself wanting to reference something to the left from something further right's
   dependency, that's usually a sign the abstraction belongs in `Core` instead.

## Testing expectations

Current coverage has real gaps — see [BACKLOG.md](BACKLOG.md#test-coverage) for specifics. When
touching any of the following, add the missing test rather than extending the gap:

- **A `UseCase` in `src/DotNetWind.Core/UseCases/`**: add a `*UseCaseTests.cs` with the
  dependencies mocked (see `UninstallUseCaseTests.cs` for the existing pattern) — assert on both
  the success path and the specific failure branches (missing dependency, validation failure,
  dry-run).
- **A new `Command` in `src/DotNetWind.Cli/Commands/`**: at minimum, extend
  `tests/DotNetWind.Cli.Tests/SmokeTests.cs` to exercise it end-to-end against a real temp
  project, the same way `init` already is. Every command currently shipped except `init` has zero
  automated coverage of any kind — don't add an eighth.
- **Anything in `ProcessRunner.cs`**: this class has no test file at all today. If you touch the
  `cmd.exe` wrapping logic specifically, that's the highest-value place to finally add coverage
  (mock `Process` behavior isn't straightforward here — a real-but-safe command like `cmd.exe /c
  echo` is a reasonable starting point).
- **`PackageJsonManager`**: existing tests only cover well-formed `package.json` inputs. A test
  with a malformed-but-valid file (e.g. `"scripts"` as a string instead of an object) would catch
  the silent-data-loss gap tracked in the backlog.

## API documentation

There are currently zero XML doc comments (`///`) anywhere in `src/`. New public members on
anything in `src/DotNetWind.Core/Abstractions/` (the interfaces every layer above `Core` depends
on) should get a doc comment explaining the contract — especially any semantics that aren't
obvious from the name (e.g. what "merge" vs "refresh" vs "create" means for
`IPackageJsonManager`'s different write methods).

## Style

- C# with `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>` everywhere
  — keep it that way; don't add `#pragma warning disable` or per-file `<Nullable>disable</Nullable>`
  to work around a real nullability issue.
- `net10.0`, `LangVersion=latest`.
- One namespace per project matching its assembly name (`DotNetWind.Core`, `DotNetWind.Cli`,
  etc.) — no cross-cutting "Common"/"Shared" namespace.
