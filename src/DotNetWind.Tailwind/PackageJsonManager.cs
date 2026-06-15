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
        return await MergeIntoExistingAsync(packageJsonPath, input, output, cancellationToken);
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

    private async Task<Result> MergeIntoExistingAsync(
        string packageJsonPath,
        string input,
        string output,
        CancellationToken cancellationToken)
    {
        var root = await ReadJsonAsync(packageJsonPath, cancellationToken);
        if (root is null)
            return Result.Failure("Failed to parse existing package.json.");

        EnsureScripts(root, input, output);
        EnsureDevDependencies(root);

        var serialized = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
        await _fileSystem.WriteAllTextAsync(packageJsonPath, serialized, cancellationToken);
        return Result.Success();
    }

    private static void EnsureScripts(JsonObject root, string input, string output)
    {
        if (root["scripts"] is not JsonObject scripts)
        {
            scripts = new JsonObject();
            root["scripts"] = scripts;
        }

        scripts.TryAdd("tw:build", JsonValue.Create($"npx @tailwindcss/cli -i {input} -o {output}"));
        scripts.TryAdd("tw:build:min", JsonValue.Create($"npx @tailwindcss/cli -i {input} -o {output} --minify"));
        scripts.TryAdd("tw:watch", JsonValue.Create($"npx @tailwindcss/cli -i {input} -o {output} --watch"));
    }

    private static void EnsureDevDependencies(JsonObject root)
    {
        if (root["devDependencies"] is not JsonObject devDeps)
        {
            devDeps = new JsonObject();
            root["devDependencies"] = devDeps;
        }

        devDeps.TryAdd("@tailwindcss/cli", JsonValue.Create("latest"));
        devDeps.TryAdd("tailwindcss", JsonValue.Create("latest"));
    }

    private async Task<JsonObject?> ReadJsonAsync(string path, CancellationToken cancellationToken)
    {
        var text = await _fileSystem.ReadAllTextAsync(path, cancellationToken);
        return JsonNode.Parse(text) as JsonObject;
    }
}
