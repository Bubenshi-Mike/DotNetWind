using System.Xml.Linq;

namespace DotNetWind.ProjectSystem;

public sealed class ProjectDetector : IProjectDetector
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<ProjectDetector> _logger;

    public ProjectDetector(IFileSystem fileSystem, ILogger<ProjectDetector> logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task<Result<ProjectInfo>> DetectAsync(string? projectPath, CancellationToken cancellationToken = default)
    {
        var csprojPath = ResolveProjectFile(projectPath);
        if (csprojPath is null)
            return Result<ProjectInfo>.Failure("No .csproj file found in the current directory. Use --project to specify one.");

        _logger.LogDebug("Detected project file: {Path}", csprojPath);

        var projectDirectory = Path.GetDirectoryName(csprojPath)!;
        var projectName = Path.GetFileNameWithoutExtension(csprojPath);

        var content = await _fileSystem.ReadAllTextAsync(csprojPath, cancellationToken);

        var targetFramework = ExtractTargetFramework(content);
        var projectType = DetectProjectType(content, projectDirectory);

        _logger.LogDebug("Detected project type: {Type}", projectType);

        return Result<ProjectInfo>.Success(new ProjectInfo(
            ProjectFilePath: csprojPath,
            ProjectDirectory: projectDirectory,
            ProjectName: projectName,
            TargetFramework: targetFramework,
            ProjectType: projectType));
    }

    private string? ResolveProjectFile(string? projectPath)
    {
        if (projectPath is not null)
        {
            if (projectPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) && _fileSystem.FileExists(projectPath))
                return projectPath;

            return null;
        }

        var currentDirectory = Directory.GetCurrentDirectory();
        var csprojFiles = _fileSystem
            .EnumerateFiles(currentDirectory, "*.csproj", SearchOption.TopDirectoryOnly)
            .ToList();

        return csprojFiles.Count == 1 ? csprojFiles[0] : null;
    }

    private static string? ExtractTargetFramework(string content)
    {
        try
        {
            var document = XDocument.Parse(content);
            var targetFramework = document
                .Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "TargetFramework")
                ?.Value
                .Trim();

            if (!string.IsNullOrWhiteSpace(targetFramework))
                return targetFramework;

            return document
                .Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "TargetFrameworks")
                ?.Value
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();
        }
        catch
        {
            const string open = "<TargetFramework>";
            const string close = "</TargetFramework>";
            var start = content.IndexOf(open, StringComparison.Ordinal);
            if (start < 0) return null;
            start += open.Length;
            var end = content.IndexOf(close, start, StringComparison.Ordinal);
            return end < 0 ? null : content[start..end].Trim();
        }
    }

    private DotNetProjectType DetectProjectType(string content, string projectDirectory)
    {
        if (content.Contains("Microsoft.NET.Sdk.BlazorWebAssembly", StringComparison.OrdinalIgnoreCase) ||
            content.Contains("Microsoft.AspNetCore.Components.WebAssembly", StringComparison.OrdinalIgnoreCase) ||
            _fileSystem.FileExists(Path.Combine(projectDirectory, "wwwroot", "index.html")))
        {
            if (!content.Contains("Microsoft.NET.Sdk.BlazorWebAssembly", StringComparison.OrdinalIgnoreCase) &&
                IsBlazorWebApp(content, projectDirectory))
                return DotNetProjectType.BlazorWebApp;

            return DotNetProjectType.BlazorWebAssembly;
        }

        if (IsBlazorWebApp(content, projectDirectory))
            return DotNetProjectType.BlazorWebApp;

        if (IsBlazorServer(content, projectDirectory))
            return DotNetProjectType.BlazorServer;

        if (IsMvc(projectDirectory))
            return DotNetProjectType.Mvc;

        if (IsRazorPages(projectDirectory))
            return DotNetProjectType.RazorPages;

        return DotNetProjectType.Unknown;
    }

    private bool IsBlazorWebApp(string content, string projectDirectory) =>
        content.Contains("Microsoft.NET.Sdk.Web", StringComparison.OrdinalIgnoreCase) &&
        (_fileSystem.FileExists(Path.Combine(projectDirectory, "Components", "App.razor")) ||
         _fileSystem.FileExists(Path.Combine(projectDirectory, "Components", "Routes.razor")));

    private bool IsBlazorServer(string content, string projectDirectory) =>
        content.Contains("Microsoft.NET.Sdk.Web", StringComparison.OrdinalIgnoreCase) &&
        _fileSystem.FileExists(Path.Combine(projectDirectory, "Pages", "_Host.cshtml"));

    private bool IsMvc(string projectDirectory) =>
        _fileSystem.DirectoryExists(Path.Combine(projectDirectory, "Controllers")) &&
        _fileSystem.DirectoryExists(Path.Combine(projectDirectory, "Views"));

    private bool IsRazorPages(string projectDirectory) =>
        _fileSystem.DirectoryExists(Path.Combine(projectDirectory, "Pages")) &&
        _fileSystem.FileExists(Path.Combine(projectDirectory, "Pages", "Shared", "_Layout.cshtml"));
}
