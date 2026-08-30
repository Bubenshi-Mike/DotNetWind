# Security Policy

## Reporting a vulnerability

Please report security issues privately via [GitHub Security Advisories](https://github.com/Bubenshi-Mike/DotNetWind/security/advisories/new) rather than filing a public issue. Include the command you ran, your OS, and the `dotnetwind doctor --json` output if relevant. We'll acknowledge reports as soon as possible.

## What DotNetWind does, and why that matters here

DotNetWind is a local developer tool that:

- **Mutates your project files** — edits `.csproj` (adds an MSBuild target) and `package.json` (adds scripts/dependencies), and writes CSS files.
- **Shells out to external processes** — `npm`, `npx`, `node`, and (on Windows, only with `--yes`) `winget`.
- **Can install software** — `dotnetwind init --yes` will install Node.js LTS via `winget install --id OpenJS.NodeJS.LTS --exact --silent` if Node/npm aren't found, but *only* when you explicitly pass `--yes`. Without it, a missing Node.js/npm surfaces as a clear failure with instructions, never a silent install.

Because of that, this file also documents known limitations transparently, rather than only after they're fixed — see [docs/BACKLOG.md](docs/BACKLOG.md) for the full tracked list and where each item stands.

## Known limitations (current behavior, not yet fixed)

- **No rollback on partial failure.** `init`/`repair`/`update` write `package.json` and `.csproj` in sequence with no transaction — if the second write fails after the first succeeded, the project is left partially modified. `uninstall` takes a `.dotnetwind.bak` backup by default, but nothing currently reads it back (no restore command exists yet). **Practical advice:** run these commands against a project under version control, and review the diff before committing.
- **Windows process invocation goes through `cmd.exe /c "<command> <args>"`** (string-concatenated, not an argument array). Every current call site passes fixed literal arguments — no project- or user-derived value reaches this path today, so there's no known exploitable injection vector — but the underlying API accepts a single opaque string, which is a fragile foundation for future features. Tracked in the backlog.
- **`--input`/`--output` paths aren't validated to stay inside your project directory.** A crafted relative path (e.g. `--output ../../../elsewhere`) will resolve and write outside the project. Since this is a local tool operating on files you already control, the practical risk is low, but there's currently no containment check anywhere in the code.
- **A malformed-but-valid `package.json`** (e.g. `"scripts"` present but not a JSON object) can have that value silently replaced rather than triggering an error. Uncommon, but worth knowing if you hand-edit `package.json` outside normal tooling.

None of the above are being disclosed here because they were actively exploited — they were found via a direct code audit and are being published for transparency before they're fixed, consistent with how this project wants to handle security going forward.

## Supported versions

Only the latest published version on [NuGet.org](https://www.nuget.org/packages/DotNetWind) is supported. Package versions auto-increment on every relevant merge to `main` (see [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md#versioning--publishing)) — there is no separate LTS/patch-branch policy while the project is pre-1.0.
