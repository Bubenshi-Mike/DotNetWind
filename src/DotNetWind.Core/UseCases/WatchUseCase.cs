using DotNetWind.Core.Abstractions;
using DotNetWind.Core.Models;
using Microsoft.Extensions.Logging;

namespace DotNetWind.Core.UseCases;

public sealed class WatchUseCase
{
    private readonly IProjectDetector _projectDetector;
    private readonly ITailwindRunner _tailwindRunner;
    private readonly ILogger<WatchUseCase> _logger;

    public WatchUseCase(
        IProjectDetector projectDetector,
        ITailwindRunner tailwindRunner,
        ILogger<WatchUseCase> logger)
    {
        _projectDetector = projectDetector;
        _tailwindRunner = tailwindRunner;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(WatchOptions options, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Starting watch use case");

        var detectResult = await _projectDetector.DetectAsync(options.ProjectPath, cancellationToken);
        if (detectResult.IsFailure)
            return Result.Failure(detectResult.ErrorMessage!);

        var project = detectResult.Value!;
        var paths = new TailwindPaths(
            InputCssPath: Path.Combine(project.ProjectDirectory, options.InputCssRelativePath.Replace('/', Path.DirectorySeparatorChar)),
            OutputCssPath: Path.Combine(project.ProjectDirectory, options.OutputCssRelativePath.Replace('/', Path.DirectorySeparatorChar)),
            PackageJsonPath: Path.Combine(project.ProjectDirectory, "package.json"),
            HostFilePath: null);

        return await _tailwindRunner.WatchAsync(paths, project.ProjectDirectory, cancellationToken);
    }
}
