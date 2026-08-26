namespace DotNetWind.Core.UseCases;

public sealed class UninstallUseCase
{
    private readonly IProjectDetector _projectDetector;
    private readonly IProjectFileUpdater _projectFileUpdater;
    private readonly IPackageJsonManager _packageJsonManager;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<UninstallUseCase> _logger;

    public UninstallUseCase(
        IProjectDetector projectDetector,
        IProjectFileUpdater projectFileUpdater,
        IPackageJsonManager packageJsonManager,
        IFileSystem fileSystem,
        ILogger<UninstallUseCase> logger)
    {
        _projectDetector = projectDetector;
        _projectFileUpdater = projectFileUpdater;
        _packageJsonManager = packageJsonManager;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(UninstallOptions options, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Starting uninstall use case");

        var detectResult = await _projectDetector.DetectAsync(options.ProjectPath, cancellationToken);
        if (detectResult.IsFailure)
            return Result.Failure(detectResult.ErrorMessage!, detectResult.ErrorKind);

        var project = detectResult.Value!;
        var packageJsonPath = Path.Combine(project.ProjectDirectory, "package.json");
        var inputCssPath = Path.Combine(project.ProjectDirectory, options.InputCssRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var outputCssPath = Path.Combine(project.ProjectDirectory, options.OutputCssRelativePath.Replace('/', Path.DirectorySeparatorChar));

        var packageResult = await _packageJsonManager.RemoveTailwindEntriesAsync(packageJsonPath, cancellationToken);
        if (packageResult.IsFailure)
            return packageResult;

        var projectResult = await _projectFileUpdater.RemoveTailwindBuildTargetAsync(project.ProjectFilePath, cancellationToken);
        if (projectResult.IsFailure)
            return projectResult;

        if (_fileSystem.FileExists(outputCssPath))
            _fileSystem.DeleteFile(outputCssPath);

        if (options.Force && _fileSystem.FileExists(inputCssPath))
            _fileSystem.DeleteFile(inputCssPath);

        return Result.Success();
    }
}
