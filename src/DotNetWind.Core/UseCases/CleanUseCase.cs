namespace DotNetWind.Core.UseCases;

public sealed class CleanUseCase
{
    private readonly IProjectDetector _projectDetector;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<CleanUseCase> _logger;

    public CleanUseCase(
        IProjectDetector projectDetector,
        IFileSystem fileSystem,
        ILogger<CleanUseCase> logger)
    {
        _projectDetector = projectDetector;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task<Result<string>> ExecuteAsync(CleanOptions options, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Starting clean use case");

        var detectResult = await _projectDetector.DetectAsync(options.ProjectPath, cancellationToken);
        if (detectResult.IsFailure)
            return Result<string>.Failure(detectResult.ErrorMessage!);

        var project = detectResult.Value!;
        var outputPath = Path.Combine(project.ProjectDirectory, options.OutputCssRelativePath.Replace('/', Path.DirectorySeparatorChar));

        if (!_fileSystem.FileExists(outputPath))
            return Result<string>.Success($"Output file does not exist: {outputPath}");

        _fileSystem.DeleteFile(outputPath);
        _logger.LogDebug("Deleted output file: {Path}", outputPath);

        return Result<string>.Success(outputPath);
    }
}
