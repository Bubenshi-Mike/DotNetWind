using DotNetWind.Core.Abstractions;
using DotNetWind.Core.Models;
using Microsoft.Extensions.Logging;

namespace DotNetWind.Core.UseCases;

public sealed record ProjectInfoReport(
    ProjectInfo Project,
    string InputCssPath,
    string OutputCssPath,
    string? NodeVersion,
    string? NpmVersion,
    bool TailwindCliInstalled);

public sealed class InfoUseCase
{
    private readonly IProjectDetector _projectDetector;
    private readonly IProcessRunner _processRunner;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<InfoUseCase> _logger;

    public InfoUseCase(
        IProjectDetector projectDetector,
        IProcessRunner processRunner,
        IFileSystem fileSystem,
        ILogger<InfoUseCase> logger)
    {
        _projectDetector = projectDetector;
        _processRunner = processRunner;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task<Result<ProjectInfoReport>> ExecuteAsync(
        string? projectPath,
        string inputCssRelativePath = "Styles/tailwind.css",
        string outputCssRelativePath = "wwwroot/css/style.css",
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Starting info use case");

        var detectResult = await _projectDetector.DetectAsync(projectPath, cancellationToken);
        if (detectResult.IsFailure)
            return Result<ProjectInfoReport>.Failure(detectResult.ErrorMessage!);

        var project = detectResult.Value!;

        var nodeResult = await _processRunner.RunAsync("node", "--version", project.ProjectDirectory, cancellationToken);
        var npmResult = await _processRunner.RunAsync("npm", "--version", project.ProjectDirectory, cancellationToken);

        var packageJsonPath = Path.Combine(project.ProjectDirectory, "package.json");
        var tailwindInstalled = _fileSystem.FileExists(packageJsonPath);

        var report = new ProjectInfoReport(
            Project: project,
            InputCssPath: Path.Combine(project.ProjectDirectory, inputCssRelativePath.Replace('/', Path.DirectorySeparatorChar)),
            OutputCssPath: Path.Combine(project.ProjectDirectory, outputCssRelativePath.Replace('/', Path.DirectorySeparatorChar)),
            NodeVersion: nodeResult.IsSuccess ? nodeResult.StandardOutput.Trim() : null,
            NpmVersion: npmResult.IsSuccess ? npmResult.StandardOutput.Trim() : null,
            TailwindCliInstalled: tailwindInstalled);

        return Result<ProjectInfoReport>.Success(report);
    }
}
