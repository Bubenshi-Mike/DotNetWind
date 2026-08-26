# DotNetWind Maintenance Plan

## Support Goals

- Keep CLI help, README guidance, and implemented behavior aligned.
- Validate every supported project type before release.
- Keep generated Tailwind setup reproducible by avoiding unbounded dependency versions.
- Make project-file edits idempotent and safe for common `.csproj` formatting.

## Regular Checks

- Run `dotnet test DotNetWind.slnx` before merging.
- Smoke-test `dotnetwind init`, `build`, `doctor`, and `clean` against temporary Blazor Web App, Blazor WebAssembly, MVC, and Razor Pages projects.
- Smoke-test `repair` and `uninstall` idempotency by running each command twice against a configured temporary project.
- Smoke-test missing Node.js/npm behavior on a clean Windows environment and confirm `dotnetwind init --yes` installs Node.js LTS through `winget`.
- Check generated `package.json` scripts and `.csproj` targets for idempotency by running `init` twice.
- Confirm generated Tailwind dependencies still use npm's `latest` dist-tag unless product policy changes.

## Release Checklist

- Restore, build, test, and pack from a clean checkout.
- Run the CLI smoke-test matrix on Windows and Linux.
- Confirm README command options match `dotnetwind --help` and command-specific help.
- Publish only from a tagged release or an explicit workflow dispatch.

## Support Triage

- Ask for the `.csproj`, `package.json`, command used, OS, .NET SDK version, Node version, npm version, and `dotnetwind doctor --json` output.
- Classify reports by project detection, package installation, Tailwind build, host-file CSS reference, or MSBuild integration.
- Add a regression test before fixing any issue that changes project detection, generated scripts, or `.csproj` mutation.
