namespace DotNetWind.ProjectSystem.Tests.Fakes;

public sealed class FakeFileSystem : IFileSystem
{
    private readonly Dictionary<string, string> _files = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _directories = new(StringComparer.OrdinalIgnoreCase);

    public void AddFile(string path, string content) => _files[Normalize(path)] = content;
    public void AddDirectory(string path) => _directories.Add(Normalize(path));

    public bool FileExists(string path) => _files.ContainsKey(Normalize(path));
    public bool DirectoryExists(string path) => _directories.Contains(Normalize(path));

    public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default) =>
        _files.TryGetValue(Normalize(path), out var content)
            ? Task.FromResult(content)
            : Task.FromException<string>(new FileNotFoundException($"File not found: {path}"));

    public Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        _files[Normalize(path)] = content;
        return Task.CompletedTask;
    }

    public void CreateDirectory(string path) => _directories.Add(Normalize(path));

    public void DeleteFile(string path) => _files.Remove(Normalize(path));

    public void CopyFile(string sourcePath, string destinationPath, bool overwrite)
    {
        var normalizedSource = Normalize(sourcePath);
        var normalizedDestination = Normalize(destinationPath);
        if (!_files.TryGetValue(normalizedSource, out var content))
            throw new FileNotFoundException($"File not found: {sourcePath}");

        if (!overwrite && _files.ContainsKey(normalizedDestination))
            throw new IOException($"File already exists: {destinationPath}");

        _files[normalizedDestination] = content;
    }

    public IEnumerable<string> EnumerateFiles(string directory, string searchPattern, SearchOption searchOption)
    {
        var normalizedDirectory = Normalize(directory);
        var pattern = searchPattern.Replace("*", "").Replace(".", "\\.");
        return _files.Keys.Where(k =>
            k.StartsWith(normalizedDirectory, StringComparison.OrdinalIgnoreCase) &&
            k.EndsWith(pattern.Replace("\\.", "."), StringComparison.OrdinalIgnoreCase));
    }

    public string? GetWrittenContent(string path) =>
        _files.TryGetValue(Normalize(path), out var c) ? c : null;

    private static string Normalize(string path) => path.Replace('\\', '/');
}
