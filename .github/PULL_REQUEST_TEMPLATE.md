## Summary

<!-- What changed and why. Link to a BACKLOG.md item if this addresses one. -->

## Checklist

- [ ] `dotnet build DotNetWind.slnx -c Release` passes
- [ ] `dotnet test DotNetWind.slnx -c Release` passes
- [ ] If this changes CLI behavior: ran the relevant manual smoke test(s) against a real
      generated project (see `docs/maintenance-plan.md`) — most command paths have no automated
      coverage yet, so this is currently the real verification step
- [ ] If this touches `src/`, `Directory.Build.props`/`.targets`, or `README.md`/`assets/icon.png`:
      merging this will auto-publish a new version to NuGet.org — confirmed this is ready to ship
- [ ] Docs updated if this changes behavior described in `README.md`,
      `docs/ARCHITECTURE.md`, or `docs/DEVELOPMENT.md`
