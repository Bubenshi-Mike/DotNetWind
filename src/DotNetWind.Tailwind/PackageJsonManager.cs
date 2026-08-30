namespace DotNetWind.Tailwind;

public sealed class PackageJsonManager : IPackageJsonManager
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<PackageJsonManager> _logger;

    public PackageJsonManager(IFileSystem fileSystem, ILogger<PackageJsonManager> logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task<Result> CreateOrMergeAsync(
        string packageJsonPath,
        string inputCssRelativePath,
        string outputCssRelativePath,
        CancellationToken cancellationToken = default)
    {
        var input = inputCssRelativePath.Replace('\\', '/');
        var output = outputCssRelativePath.Replace('\\', '/');

        if (!_fileSystem.FileExists(packageJsonPath))
        {
            _logger.LogDebug("Creating package.json at {Path}", packageJsonPath);
            var content = PackageJsonTemplate.GetDefault(input, output);
            await _fileSystem.WriteAllTextAsync(packageJsonPath, content, cancellationToken);
            return Result.Success();
        }

        _logger.LogDebug("Merging into existing package.json at {Path}", packageJsonPath);
        return await MergeIntoExistingAsync(packageJsonPath, input, output, overwriteManagedEntries: false, cancellationToken);
    }

    public Task<Result> RefreshManagedEntriesAsync(
        string packageJsonPath,
        string inputCssRelativePath,
        string outputCssRelativePath,
        CancellationToken cancellationToken = default)
    {
        var input = inputCssRelativePath.Replace('\\', '/');
        var output = outputCssRelativePath.Replace('\\', '/');

        if (!_fileSystem.FileExists(packageJsonPath))
            return CreateOrMergeAsync(packageJsonPath, input, output, cancellationToken);

        _logger.LogDebug("Refreshing DotNetWind package.json entries at {Path}", packageJsonPath);
        return MergeIntoExistingAsync(packageJsonPath, input, output, overwriteManagedEntries: true, cancellationToken);
    }

    public async Task<bool> HasTailwindScriptsAsync(string packageJsonPath, CancellationToken cancellationToken = default)
    {
        if (!_fileSystem.FileExists(packageJsonPath)) return false;

        var root = await ReadJsonAsync(packageJsonPath, cancellationToken);
        var scripts = root?["scripts"] as JsonObject;
        return scripts?.ContainsKey("tw:build") ?? false;
    }

    public async Task<bool> HasTailwindDependenciesAsync(string packageJsonPath, CancellationToken cancellationToken = default)
    {
        if (!_fileSystem.FileExists(packageJsonPath)) return false;

        var root = await ReadJsonAsync(packageJsonPath, cancellationToken);
        var devDeps = root?["devDependencies"] as JsonObject;
        return devDeps?.ContainsKey("tailwindcss") ?? false;
    }

    public async Task<Result> RemoveTailwindEntriesAsync(string packageJsonPath, CancellationToken cancellationToken = default)
    {
        if (!_fileSystem.FileExists(packageJsonPath))
            return Result.Success();

        var root = await ReadJsonAsync(packageJsonPath, cancellationToken);
        if (root is null)
            return Result.Failure("Failed to parse existing package.json.");

        RemoveScripts(root);
        RemoveDevDependencies(root);

        var serialized = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
        await _fileSystem.WriteAllTextAsync(packageJsonPath, serialized, cancellationToken);
        return Result.Success();
    }

    private async Task<Result> MergeIntoExistingAsync(
        string packageJsonPath,
        string input,
        string output,
        bool overwriteManagedEntries,
        CancellationToken cancellationToken)
    {
        var root = await ReadJsonAsync(packageJsonPath, cancellationToken);
        if (root is null)
            return Result.Failure("Failed to parse existing package.json.");

        if (overwriteManagedEntries)
        {
            RefreshScripts(root, input, output);
            RefreshDevDependencies(root);
        }
        else
        {
            EnsureScripts(root, input, output);
            EnsureDevDependencies(root);
        }

        var serialized = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
        await _fileSystem.WriteAllTextAsync(packageJsonPath, serialized, cancellationToken);
        return Result.Success();
    }

    private static void EnsureScripts(JsonObject root, string input, string output)
    {
        AddScripts(root, input, output, overwrite: false);
    }

    private static void RefreshScripts(JsonObject root, string input, string output)
    {
        AddScripts(root, input, output, overwrite: true);
    }

    private static void AddScripts(JsonObject root, string input, string output, bool overwrite)
    {
        if (root["scripts"] is not JsonObject scripts)
        {
            scripts = new JsonObject();
            root["scripts"] = scripts;
        }

        AddOrSet(scripts, "tw:build", $"npx @tailwindcss/cli -i {input} -o {output}", overwrite);
        AddOrSet(scripts, "tw:build:min", $"npx @tailwindcss/cli -i {input} -o {output} --minify", overwrite);
        AddOrSet(scripts, "tw:watch", $"npx @tailwindcss/cli -i {input} -o {output} --watch", overwrite);
    }

    private static void EnsureDevDependencies(JsonObject root)
    {
        AddDevDependencies(root, overwrite: false);
    }

    private static void RefreshDevDependencies(JsonObject root)
    {
        AddDevDependencies(root, overwrite: true);
    }

    private static void AddDevDependencies(JsonObject root, bool overwrite)
    {
        if (root["devDependencies"] is not JsonObject devDeps)
        {
            devDeps = new JsonObject();
            root["devDependencies"] = devDeps;
        }

        AddOrSet(devDeps, "@tailwindcss/cli", "latest", overwrite);
        AddOrSet(devDeps, "tailwindcss", "latest", overwrite);
    }

    private static void AddOrSet(JsonObject obj, string key, string value, bool overwrite)
    {
        if (overwrite)
            obj[key] = value;
        else
            obj.TryAdd(key, JsonValue.Create(value));
    }

    private static void RemoveScripts(JsonObject root)
    {
        if (root["scripts"] is not JsonObject scripts)
            return;

        scripts.Remove("tw:build");
        scripts.Remove("tw:build:min");
        scripts.Remove("tw:watch");

        if (scripts.Count == 0)
            root.Remove("scripts");
    }

    private static void RemoveDevDependencies(JsonObject root)
    {
        if (root["devDependencies"] is not JsonObject devDeps)
            return;

        devDeps.Remove("@tailwindcss/cli");
        devDeps.Remove("tailwindcss");

        if (devDeps.Count == 0)
            root.Remove("devDependencies");
    }

    private async Task<JsonObject?> ReadJsonAsync(string path, CancellationToken cancellationToken)
    {
        var text = await _fileSystem.ReadAllTextAsync(path, cancellationToken);
        return JsonNode.Parse(text) as JsonObject;
    }
}
