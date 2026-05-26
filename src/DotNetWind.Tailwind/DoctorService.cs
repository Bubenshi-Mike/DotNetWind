using DotNetWind.Core.Abstractions;
using DotNetWind.Core.Models;
using Microsoft.Extensions.Logging;

namespace DotNetWind.Tailwind;

public sealed class DoctorService : IDoctorService
{
    private readonly IProjectDetector _projectDetector;
    private readonly IProjectFileUpdater _projectFileUpdater;
    private readonly IPackageJsonManager _packageJsonManager;
    private readonly IHostFileDetector _hostFileDetector;
    private readonly IFileSystem _fileSystem;
    private readonly IProcessRunner _processRunner;
    private readonly ILogger<DoctorService> _logger;

    public DoctorService(
        IProjectDetector projectDetector,
        IProjectFileUpdater projectFileUpdater,
        IPackageJsonManager packageJsonManager,
        IHostFileDetector hostFileDetector,
        IFileSystem fileSystem,
        IProcessRunner processRunner,
        ILogger<DoctorService> logger)
    {
        _projectDetector = projectDetector;
        _projectFileUpdater = projectFileUpdater;
        _packageJsonManager = packageJsonManager;
        _hostFileDetector = hostFileDetector;
        _fileSystem = fileSystem;
        _processRunner = processRunner;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DoctorCheckResult>> RunChecksAsync(
        string? projectPath,
        CancellationToken cancellationToken = default)
    {
        var checks = new List<DoctorCheckResult>();

        var detectResult = await _projectDetector.DetectAsync(projectPath, cancellationToken);

        checks.Add(detectResult.IsSuccess
            ? Pass("Project file found", $"Found: {Path.GetFileName(detectResult.Value!.ProjectFilePath)}")
            : Fail("Project file found", detectResult.ErrorMessage!, "Run dotnetwind init in a directory with a .csproj file."));

        if (detectResult.IsFailure)
            return checks;

        var project = detectResult.Value!;

        checks.Add(project.ProjectType != DotNetProjectType.Unknown
            ? Pass("Project type detected", project.GetDisplayName())
            : Warn("Project type detected", "Could not determine project type.", "Use --framework to specify the type."));

        var packageJsonPath = Path.Combine(project.ProjectDirectory, "package.json");
        checks.Add(_fileSystem.FileExists(packageJsonPath)
            ? Pass("package.json found", "package.json exists")
            : Fail("package.json found", "package.json not found", "Run: dotnetwind init"));

        var nodeResult = await _processRunner.RunAsync("node", "--version", project.ProjectDirectory, cancellationToken);
        checks.Add(nodeResult.IsSuccess
            ? Pass("Node.js installed", nodeResult.StandardOutput.Trim())
            : Fail("Node.js installed", "Node.js not found", "Install Node.js from https://nodejs.org"));

        var npmResult = await _processRunner.RunAsync("npm", "--version", project.ProjectDirectory, cancellationToken);
        checks.Add(npmResult.IsSuccess
            ? Pass("npm installed", $"npm {npmResult.StandardOutput.Trim()}")
            : Fail("npm installed", "npm not found", "Install npm via Node.js installer"));

        if (_fileSystem.FileExists(packageJsonPath))
        {
            var hasTailwindDeps = await _packageJsonManager.HasTailwindDependenciesAsync(packageJsonPath, cancellationToken);
            checks.Add(hasTailwindDeps
                ? Pass("tailwindcss installed", "Found in devDependencies")
                : Fail("tailwindcss installed", "tailwindcss not in devDependencies", "Run: dotnetwind init"));

            var hasTailwindScripts = await _packageJsonManager.HasTailwindScriptsAsync(packageJsonPath, cancellationToken);
            checks.Add(hasTailwindScripts
                ? Pass("@tailwindcss/cli scripts", "tw:build, tw:build:min, tw:watch found")
                : Fail("@tailwindcss/cli scripts", "Tailwind scripts not in package.json", "Run: dotnetwind init"));
        }

        var inputCssPath = Path.Combine(project.ProjectDirectory, "Styles", "tailwind.css");
        checks.Add(_fileSystem.FileExists(inputCssPath)
            ? Pass("Styles/tailwind.css found", inputCssPath)
            : Fail("Styles/tailwind.css found", "Input CSS not found", "Run: dotnetwind init"));

        var outputCssPath = Path.Combine(project.ProjectDirectory, "wwwroot", "css", "style.css");
        checks.Add(_fileSystem.FileExists(outputCssPath)
            ? Pass("wwwroot/css/style.css found", outputCssPath)
            : Warn("wwwroot/css/style.css found", "Output CSS not found", "Run: dotnetwind build"));

        var hasMsBuildTarget = await _projectFileUpdater.HasTailwindBuildTargetAsync(project.ProjectFilePath, cancellationToken);
        checks.Add(hasMsBuildTarget
            ? Pass("MSBuild BuildTailwind target", "Target found in .csproj")
            : Fail("MSBuild BuildTailwind target", "BuildTailwind target not in .csproj", "Run: dotnetwind init"));

        var hostFile = _hostFileDetector.FindHostFile(project);
        if (hostFile is not null)
        {
            var hasCssRef = _hostFileDetector.HasCssReference(hostFile, "css/style.css");
            checks.Add(hasCssRef
                ? Pass("CSS reference in host file", $"Found in {Path.GetFileName(hostFile)}")
                : Warn("CSS reference in host file", $"css/style.css not referenced in {Path.GetFileName(hostFile)}",
                    $"Add: <link href=\"css/style.css\" rel=\"stylesheet\" /> to {Path.GetFileName(hostFile)}"));
        }
        else
        {
            checks.Add(Warn("CSS reference in host file", "Could not detect host file", "Manually add the CSS link to your layout or host file."));
        }

        return checks;
    }

    private static DoctorCheckResult Pass(string name, string message) =>
        new(name, DoctorStatus.Pass, message);

    private static DoctorCheckResult Warn(string name, string message, string? recommendation = null) =>
        new(name, DoctorStatus.Warning, message, recommendation);

    private static DoctorCheckResult Fail(string name, string message, string? recommendation = null) =>
        new(name, DoctorStatus.Fail, message, recommendation);
}
