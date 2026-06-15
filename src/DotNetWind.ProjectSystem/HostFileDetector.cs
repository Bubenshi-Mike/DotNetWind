namespace DotNetWind.ProjectSystem;

public sealed class HostFileDetector : IHostFileDetector
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<HostFileDetector> _logger;

    public HostFileDetector(IFileSystem fileSystem, ILogger<HostFileDetector> logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public string? FindHostFile(ProjectInfo project)
    {
        var candidates = GetHostFileCandidates(project);
        foreach (var candidate in candidates)
        {
            var fullPath = Path.Combine(project.ProjectDirectory, candidate);
            if (_fileSystem.FileExists(fullPath))
            {
                _logger.LogDebug("Found host file: {Path}", fullPath);
                return fullPath;
            }
        }

        _logger.LogDebug("No host file found for project type {Type}", project.ProjectType);
        return null;
    }

    public bool HasCssReference(string hostFilePath, string cssRelativePath)
    {
        if (!_fileSystem.FileExists(hostFilePath))
            return false;

        var content = _fileSystem.ReadAllTextAsync(hostFilePath).GetAwaiter().GetResult();
        var normalizedPath = cssRelativePath.Replace('\\', '/');

        return content.Contains(normalizedPath, StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> GetHostFileCandidates(ProjectInfo project) =>
        project.ProjectType switch
        {
            DotNetProjectType.BlazorWebAssembly =>
            [
                Path.Combine("wwwroot", "index.html")
            ],
            DotNetProjectType.BlazorWebApp =>
            [
                Path.Combine("Components", "App.razor"),
                Path.Combine("App.razor")
            ],
            DotNetProjectType.BlazorServer =>
            [
                Path.Combine("Pages", "_Host.cshtml"),
                Path.Combine("Pages", "_Layout.cshtml")
            ],
            DotNetProjectType.Mvc =>
            [
                Path.Combine("Views", "Shared", "_Layout.cshtml")
            ],
            DotNetProjectType.RazorPages =>
            [
                Path.Combine("Pages", "Shared", "_Layout.cshtml")
            ],
            _ => []
        };
}
