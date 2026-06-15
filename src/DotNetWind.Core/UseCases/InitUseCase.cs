namespace DotNetWind.Core.UseCases;

public sealed class InitUseCase
{
    private readonly IProjectDetector _projectDetector;
    private readonly ITailwindInitializer _tailwindInitializer;
    private readonly ILogger<InitUseCase> _logger;

    public InitUseCase(
        IProjectDetector projectDetector,
        ITailwindInitializer tailwindInitializer,
        ILogger<InitUseCase> logger)
    {
        _projectDetector = projectDetector;
        _tailwindInitializer = tailwindInitializer;
        _logger = logger;
    }

    public async Task<Result<ProjectInfo>> ExecuteAsync(SetupOptions options, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Starting init use case with options: {Options}", options);

        var detectResult = await _projectDetector.DetectAsync(options.ProjectPath, cancellationToken);
        if (detectResult.IsFailure)
            return Result<ProjectInfo>.Failure(detectResult.ErrorMessage!);

        var project = detectResult.Value!;

        if (project.ProjectType == DotNetProjectType.Unknown)
            return Result<ProjectInfo>.Failure($"Could not determine project type for '{project.ProjectName}'. Use --framework to specify the project type.");

        var paths = new TailwindPaths(
            InputCssPath: Path.Combine(project.ProjectDirectory, options.InputCssRelativePath.Replace('/', Path.DirectorySeparatorChar)),
            OutputCssPath: Path.Combine(project.ProjectDirectory, options.OutputCssRelativePath.Replace('/', Path.DirectorySeparatorChar)),
            PackageJsonPath: Path.Combine(project.ProjectDirectory, "package.json"),
            HostFilePath: null);

        var initResult = await _tailwindInitializer.InitializeAsync(project, paths, options, cancellationToken);
        if (initResult.IsFailure)
            return Result<ProjectInfo>.Failure(initResult.ErrorMessage!);

        return Result<ProjectInfo>.Success(project);
    }
}
