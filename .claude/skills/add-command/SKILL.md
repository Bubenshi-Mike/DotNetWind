---
name: add-command
description: Scaffold a new DotNetWind CLI command following the repo's existing Command -> UseCase -> Abstraction pattern. Use when adding a new dotnetwind subcommand (e.g. a new verb like "eject" or "check").
---

# Add a new DotNetWind command

Every command in this repo follows the same shape end to end. Don't improvise a new pattern —
follow this one, using `src/DotNetWind.Cli/Commands/RepairCommand.cs` as the reference
implementation (it's small and touches every layer once).

## Steps

1. **Decide if you need a new UseCase or can reuse an existing one.**
   `RepairUseCase`/`UpdateUseCase` are thin delegations to `InitUseCase` — if the new command is
   a variant of an existing operation, prefer delegating rather than duplicating orchestration
   logic.

2. **If a new UseCase is needed**, add it to `src/DotNetWind.Core/UseCases/`:
   - Constructor takes the abstractions it needs from `src/DotNetWind.Core/Abstractions/`
     (`IProcessRunner`, `IFileSystem`, etc.) — never a concrete implementation type.
   - `ExecuteAsync(SomeOptions options, CancellationToken ct)` returns `Result` or `Result<T>`
     (`src/DotNetWind.Core/Models/Result.cs`). Use `ResultErrorKind` to classify failures
     (`Validation`, `UnsupportedProjectType`, `MissingDependency`, `UserCancelled`) — the CLI
     layer maps this to a specific process exit code, so pick the kind that actually matches.
   - Add a dedicated `*UseCaseTests.cs` in `tests/DotNetWind.Core.Tests/UseCases/` — this repo
     has several use cases with zero test coverage already (tracked in `docs/BACKLOG.md`); don't
     add another one. Mock the abstractions the same way `UninstallUseCaseTests.cs` does.

3. **Add the Command class** in `src/DotNetWind.Cli/Commands/YourCommand.cs`:
   - `public static class YourCommand { public static Command Create(IServiceProvider services) }`
   - Define `Option<T>` instances for each flag, matching the naming/description style of the
     existing commands (see `RepairCommand.cs` for the full option set most commands share:
     `--project`, `--framework`, `--input`, `--output`, `--skip-npm-install`,
     `--skip-node-install`, `--skip-build`, `--force`, `--yes`, `--dry-run`).
   - In `command.SetAction(...)`: resolve `IConsoleOutput` and your UseCase from `services`,
     build the options record from `parseResult.GetValue(...)`, call `ExecuteAsync`, and map the
     `Result` to `console.WriteError`/`console.WriteSuccess` + the right `ExitCode` — **never**
     call `Console.Write*` directly here or anywhere in `Tailwind`/`ProjectSystem`/`Core`; always
     go through `IConsoleOutput`.

4. **Register the command** in `src/DotNetWind.Cli/Program.cs`:
   ```csharp
   rootCommand.Subcommands.Add(YourCommand.Create(services));
   ```

5. **Register any new UseCase** in
   `src/DotNetWind.Cli/DependencyInjection/ServiceCollectionExtensions.cs`:
   ```csharp
   services.AddTransient<YourUseCase>();
   ```

6. **Add end-to-end coverage** in `tests/DotNetWind.Cli.Tests/SmokeTests.cs` — this is currently
   the *only* place any command gets exercised against a real generated project. Every command
   except `init` has zero smoke-test coverage today; don't ship a ninth command in that state if
   you can help it.

7. **Update `README.md`**'s command reference table and the full option listing for the new
   command, matching the existing format for other commands.

8. If the new command needs a genuinely new capability (not just a variant of file mutation /
   process invocation the existing abstractions already support), check
   `src/DotNetWind.Core/Abstractions/` first — extend an existing interface if it's a natural fit
   before adding a new one.
