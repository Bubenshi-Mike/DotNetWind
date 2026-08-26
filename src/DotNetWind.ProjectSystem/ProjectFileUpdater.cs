using System.Xml.Linq;

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

    public async Task<Result> RemoveTailwindBuildTargetAsync(
        string projectFilePath,
        CancellationToken cancellationToken = default)
    {
        if (!_fileSystem.FileExists(projectFilePath))
            return Result.Success();

        var content = await _fileSystem.ReadAllTextAsync(projectFilePath, cancellationToken);
        var updatedContent = RemoveTarget(content);

        if (!string.Equals(content, updatedContent, StringComparison.Ordinal))
            await _fileSystem.WriteAllTextAsync(projectFilePath, updatedContent, cancellationToken);

        return Result.Success();
    }

    private static bool HasTailwindTarget(string content) =>
        TryParseProject(content, out var document)
            ? document.Descendants().Any(e =>
                e.Name.LocalName == "Target" &&
                string.Equals((string?)e.Attribute("Name"), MsBuildTargetTemplate.TargetName, StringComparison.Ordinal))
            : content.Contains($"Name=\"{MsBuildTargetTemplate.TargetName}\"", StringComparison.Ordinal);

    private static string InjectTarget(string content)
    {
        if (TryParseProject(content, out var document) && document.Root is not null)
        {
            var ns = document.Root.Name.Namespace;
            document.Root.Add(
                new XElement(ns + "Target",
                    new XAttribute("Name", MsBuildTargetTemplate.TargetName),
                    new XAttribute("BeforeTargets", "Build"),
                    new XElement(ns + "Message",
                        new XAttribute("Text", "Building Tailwind CSS..."),
                        new XAttribute("Importance", "high")),
                    new XElement(ns + "Exec",
                        new XAttribute("Command", "npm run tw:build"),
                        new XAttribute("Condition", "'$(Configuration)' == 'Debug'")),
                    new XElement(ns + "Exec",
                        new XAttribute("Command", "npm run tw:build:min"),
                        new XAttribute("Condition", "'$(Configuration)' == 'Release'"))));

            return document.ToString(SaveOptions.DisableFormatting);
        }

        const string closingTag = "</Project>";
        var insertIndex = content.LastIndexOf(closingTag, StringComparison.OrdinalIgnoreCase);
        if (insertIndex < 0)
            return content + Environment.NewLine + MsBuildTargetTemplate.GetTarget();

        var target = Environment.NewLine + MsBuildTargetTemplate.GetTarget() + Environment.NewLine;
        return content.Insert(insertIndex, target);
    }

    private static string RemoveTarget(string content)
    {
        if (!TryParseProject(content, out var document))
            return content;

        var targets = document
            .Descendants()
            .Where(e =>
                e.Name.LocalName == "Target" &&
                string.Equals((string?)e.Attribute("Name"), MsBuildTargetTemplate.TargetName, StringComparison.Ordinal))
            .ToList();

        if (targets.Count == 0)
            return content;

        foreach (var target in targets)
            target.Remove();

        return document.ToString(SaveOptions.DisableFormatting);
    }

    private static bool TryParseProject(string content, out XDocument document)
    {
        try
        {
            document = XDocument.Parse(content, LoadOptions.PreserveWhitespace);
            return true;
        }
        catch
        {
            document = new XDocument();
            return false;
        }
    }
}
