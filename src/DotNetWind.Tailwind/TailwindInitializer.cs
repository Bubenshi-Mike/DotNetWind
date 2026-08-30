namespace DotNetWind.Tailwind;

public sealed class TailwindInitializer : ITailwindInitializer
{
    private readonly IFileSystem _fileSystem;
    private readonly IPackageJsonManager _packageJsonManager;
    private readonly IProjectFileUpdater _projectFileUpdater;
    private readonly IProcessRunner _processRunner;
    private readonly ITailwindRunner _tailwindRunner;
    private readonly INodeJsInstaller _nodeJsInstaller;
    private readonly ILogger<TailwindInitializer> _logger;

    public TailwindInitializer(
        IFileSystem fileSystem,
        IPackageJsonManager packageJsonManager,
        IProjectFileUpdater projectFileUpdater,
        IProcessRunner processRunner,
        ITailwindRunner tailwindRunner,
        INodeJsInstaller nodeJsInstaller,
        ILogger<TailwindInitializer> logger)
    {
        _fileSystem = fileSystem;
        _packageJsonManager = packageJsonManager;
        _projectFileUpdater = projectFileUpdater;
        _processRunner = processRunner;
        _tailwindRunner = tailwindRunner;
        _nodeJsInstaller = nodeJsInstaller;
        _logger = logger;
    }

    public async Task<Result> InitializeAsync(
        ProjectInfo project,
        TailwindPaths paths,
        SetupOptions options,
        CancellationToken cancellationToken = default)
    {
        EnsureStylesDirectory(paths.InputCssPath);
        EnsureOutputDirectory(paths.OutputCssPath);

        await CreateTailwindCssIfMissingAsync(paths.InputCssPath, options.Force, cancellationToken);

        var inputRelative = GetRelativePath(project.ProjectDirectory, paths.InputCssPath);
        var outputRelative = GetRelativePath(project.ProjectDirectory, paths.OutputCssPath);

        var packageJsonResult = options.RefreshPackageJsonEntries
            ? await _packageJsonManager.RefreshManagedEntriesAsync(
                paths.PackageJsonPath,
                inputRelative,
                outputRelative,
                cancellationToken)
            : await _packageJsonManager.CreateOrMergeAsync(
                paths.PackageJsonPath,
                inputRelative,
                outputRelative,
                cancellationToken);

        if (packageJsonResult.IsFailure)
            return packageJsonResult;

        var csprojResult = await _projectFileUpdater.AddTailwindBuildTargetAsync(project, paths, cancellationToken);
        if (csprojResult.IsFailure)
            return csprojResult;

        if (!options.SkipNpmInstall || !options.SkipBuild)
        {
            var dependencyResult = await _nodeJsInstaller.EnsureNodeAndNpmAsync(
                project.ProjectDirectory,
                allowInstall: options.AssumeYes,
                skipInstall: options.SkipNodeInstall,
                cancellationToken: cancellationToken);

            if (dependencyResult.IsFailure)
                return dependencyResult;
        }

        if (!options.SkipNpmInstall)
        {
            var npmResult = await _processRunner.RunAsync("npm", "install", project.ProjectDirectory, cancellationToken);
            if (!npmResult.IsSuccess)
            {
                var errorKind = npmResult.ExitCode == 127 ? ResultErrorKind.MissingDependency : ResultErrorKind.General;
                return Result.Failure($"npm install failed (exit code {npmResult.ExitCode}):\n{npmResult.StandardError}", errorKind);
            }
        }

        if (!options.SkipBuild)
        {
            var buildResult = await _tailwindRunner.BuildAsync(paths, minify: false, project.ProjectDirectory, cancellationToken);
            if (buildResult.IsFailure)
                return buildResult;
        }

        return Result.Success();
    }

    private void EnsureStylesDirectory(string inputCssPath)
    {
        var dir = Path.GetDirectoryName(inputCssPath)!;
        if (!_fileSystem.DirectoryExists(dir))
        {
            _logger.LogDebug("Creating directory: {Dir}", dir);
            _fileSystem.CreateDirectory(dir);
        }
    }

    private void EnsureOutputDirectory(string outputCssPath)
    {
        var dir = Path.GetDirectoryName(outputCssPath)!;
        if (!_fileSystem.DirectoryExists(dir))
        {
            _logger.LogDebug("Creating directory: {Dir}", dir);
            _fileSystem.CreateDirectory(dir);
        }
    }

    private async Task CreateTailwindCssIfMissingAsync(string path, bool force, CancellationToken cancellationToken)
    {
        if (_fileSystem.FileExists(path) && !force)
        {
            _logger.LogDebug("Tailwind CSS input file already exists at {Path}, skipping.", path);
            return;
        }

        _logger.LogDebug("Creating tailwind.css at {Path}", path);
        await _fileSystem.WriteAllTextAsync(path, TailwindCssTemplate.GetDefault(), cancellationToken);
    }

    private static string GetRelativePath(string basePath, string fullPath)
    {
        var relative = Path.GetRelativePath(basePath, fullPath);
        return relative.Replace('\\', '/');
    }
}
