namespace DotNetWind.ProjectSystem;

public sealed class ProjectFileUpdater : IProjectFileUpdater
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<ProjectFileUpdater> _logger;

    public ProjectFileUpdater(IFileSystem fileSystem, ILogger<ProjectFileUpdater> logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task<Result> AddTailwindBuildTargetAsync(
        ProjectInfo project,
        TailwindPaths paths,
        CancellationToken cancellationToken = default)
    {
        var content = await _fileSystem.ReadAllTextAsync(project.ProjectFilePath, cancellationToken);

        if (HasTailwindTarget(content))
        {
            _logger.LogDebug("BuildTailwind target already exists in {Path}", project.ProjectFilePath);
            return Result.Success();
        }

        var updatedContent = InjectTarget(content);
        await _fileSystem.WriteAllTextAsync(project.ProjectFilePath, updatedContent, cancellationToken);

        _logger.LogDebug("Added BuildTailwind target to {Path}", project.ProjectFilePath);
        return Result.Success();
    }

    public async Task<bool> HasTailwindBuildTargetAsync(string projectFilePath, CancellationToken cancellationToken = default)
    {
        if (!_fileSystem.FileExists(projectFilePath))
            return false;

        var content = await _fileSystem.ReadAllTextAsync(projectFilePath, cancellationToken);
        return HasTailwindTarget(content);
    }

    private static bool HasTailwindTarget(string content) =>
        content.Contains($"Name=\"{MsBuildTargetTemplate.TargetName}\"", StringComparison.Ordinal);

    private static string InjectTarget(string content)
    {
        const string closingTag = "</Project>";
        var insertIndex = content.LastIndexOf(closingTag, StringComparison.OrdinalIgnoreCase);
        if (insertIndex < 0)
            return content + Environment.NewLine + MsBuildTargetTemplate.GetTarget();

        var target = Environment.NewLine + MsBuildTargetTemplate.GetTarget() + Environment.NewLine;
        return content.Insert(insertIndex, target);
    }
}
