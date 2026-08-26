# DotNetWind

> Tailwind CSS setup for .NET, without the pain.

DotNetWind is a .NET CLI tool that configures Tailwind CSS v4 in Blazor, ASP.NET Core MVC, Razor Pages, and Razor Class Library projects — in one command.

## Installation

```bash
dotnet tool install --global DotNetWind
```

## Quick Start

```bash
cd MyBlazorApp
dotnetwind init
```

That's it. DotNetWind will:

- Detect your project type
- Create `Styles/tailwind.css`
- Create or update `package.json` with Tailwind scripts
- Add a `BuildTailwind` MSBuild target to your `.csproj`
- Run `npm install`
- Print next steps

## Commands

| Command | Description |
|---------|-------------|
| `dotnetwind init` | Configure Tailwind CSS in the current project |
| `dotnetwind build` | Build Tailwind CSS manually |
| `dotnetwind watch` | Watch for CSS changes (development) |
| `dotnetwind doctor` | Validate the Tailwind setup |
| `dotnetwind clean` | Remove generated CSS output |
| `dotnetwind info` | Display project and tool information |
| `dotnetwind repair` | Re-apply missing DotNetWind setup files/configuration |
| `dotnetwind uninstall` | Remove DotNetWind build configuration and generated CSS output |

## Command Options

### `dotnetwind init`

```bash
dotnetwind init [options]

Options:
  --project <path>        Path to the .csproj file
  --framework <type>      Project type if auto-detection is ambiguous
                          Values: blazor-wasm, blazor-server, blazor-webapp, mvc, razor-pages, razor-class-library
  --input <path>          Tailwind CSS input path (default: Styles/tailwind.css)
  --output <path>         CSS output path (default: wwwroot/css/style.css)
  --skip-npm-install      Skip running npm install
  --skip-node-install     Do not install Node.js automatically if node/npm are missing
  --skip-build            Skip running initial Tailwind build
  --yes                   Allow non-interactive installation of missing prerequisites
  --force                 Overwrite existing files
  --verbose               Show detailed output
```

### `dotnetwind build`

```bash
dotnetwind build [options]

Options:
  --minify                Minify the output CSS
  --input <path>          Tailwind CSS input path
  --output <path>         CSS output path
```

### `dotnetwind doctor`

```bash
dotnetwind doctor [options]

Options:
  --json                  Output results as JSON (useful for CI/CD)
  --input <path>          Tailwind CSS input path (default: Styles/tailwind.css)
  --output <path>         CSS output path (default: wwwroot/css/style.css)
```

### `dotnetwind repair`

```bash
dotnetwind repair [options]

Options:
  --project <path>        Path to the .csproj file
  --framework <type>      Project type if auto-detection is ambiguous
                          Values: blazor-wasm, blazor-server, blazor-webapp, mvc, razor-pages, razor-class-library
  --input <path>          Tailwind CSS input path (default: Styles/tailwind.css)
  --output <path>         CSS output path (default: wwwroot/css/style.css)
  --skip-npm-install      Skip running npm install
  --skip-node-install     Do not install Node.js automatically if node/npm are missing
  --skip-build            Skip running Tailwind build
  --force                 Overwrite existing Tailwind input file
  --yes                   Allow non-interactive installation of missing prerequisites
```

### `dotnetwind uninstall`

```bash
dotnetwind uninstall [options]

Options:
  --project <path>        Path to the .csproj file
  --input <path>          Tailwind CSS input path (default: Styles/tailwind.css)
  --output <path>         CSS output path to remove (default: wwwroot/css/style.css)
  --force                 Also remove the Tailwind input CSS file
```

## Supported Project Types

- Blazor WebAssembly
- Blazor Web App (.NET 8+)
- Blazor Server
- ASP.NET Core MVC
- Razor Pages
- Razor Class Libraries (`Microsoft.NET.Sdk.Razor`)

See [the compatibility matrix](docs/compatibility-matrix.md) for detection rules, default paths, host-file behavior, and CSS link formats.

## What Gets Generated

After `dotnetwind init`, your project will have:

**`Styles/tailwind.css`** — Tailwind CSS v4 input file:
```css
@import "tailwindcss";

@theme {
    --font-sans: 'Arimo', 'Geist', ui-sans-serif, system-ui, sans-serif;
    /* ... */
}
```

**`package.json`** — with Tailwind scripts:
```json
{
  "scripts": {
    "tw:build": "npx @tailwindcss/cli -i Styles/tailwind.css -o wwwroot/css/style.css",
    "tw:build:min": "npx @tailwindcss/cli -i Styles/tailwind.css -o wwwroot/css/style.css --minify",
    "tw:watch": "npx @tailwindcss/cli -i Styles/tailwind.css -o wwwroot/css/style.css --watch"
  },
  "devDependencies": {
    "@tailwindcss/cli": "latest",
    "tailwindcss": "latest"
  }
}
```

**MSBuild target** added to your `.csproj`:
```xml
<Target Name="BuildTailwind" BeforeTargets="Build">
  <Exec Command="npm run tw:build" Condition="'$(Configuration)' == 'Debug'" />
  <Exec Command="npm run tw:build:min" Condition="'$(Configuration)' == 'Release'" />
</Target>
```

## Doctor Output Example

```
─ DotNetWind Doctor ─────────────────────────────────────

✓ Project file found: MyApp.csproj
✓ Project type detected: Blazor WebAssembly
✓ package.json found
✓ Node.js installed: v22.0.0
✓ npm installed: npm 10.0.0
✓ tailwindcss installed: Found in devDependencies
✓ @tailwindcss/cli scripts: tw:build, tw:build:min, tw:watch found
✓ Styles/tailwind.css found
! wwwroot/css/style.css found: Output CSS not found
  → Run: dotnetwind build
✓ MSBuild BuildTailwind target: Target found in .csproj
✓ CSS reference in host file: Found in index.html
```

## Requirements

- .NET 10 SDK or later
- Node.js 18+ and npm. If Node.js/npm are missing, `dotnetwind init --yes` attempts to install Node.js LTS automatically on Windows through `winget`. Use `--skip-node-install` to prevent automatic installation, or `--skip-npm-install --skip-build` for offline/config-only setup.

## Architecture

```
DotNetWind.sln
│
├── src/
│   ├── DotNetWind.Cli           # CLI entry point, commands, console output
│   ├── DotNetWind.Core          # Models, abstractions, use cases
│   ├── DotNetWind.ProjectSystem # Project detection, .csproj modification
│   ├── DotNetWind.Tailwind      # Tailwind setup, npm runner, doctor service
│   └── DotNetWind.Templates     # CSS, package.json, MSBuild templates
│
└── tests/
    ├── DotNetWind.Core.Tests
    ├── DotNetWind.ProjectSystem.Tests
    ├── DotNetWind.Tailwind.Tests
    └── DotNetWind.Cli.Tests
```

## License

MIT
