namespace DotNetWind.Tailwind.Tests.Fakes;

public sealed class FakeFileSystem : IFileSystem
{
    private readonly Dictionary<string, string> _files = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _directories = new(StringComparer.OrdinalIgnoreCase);

    public void AddFile(string path, string content) => _files[path] = content;
    public void AddDirectory(string path) => _directories.Add(path);

    public bool FileExists(string path) => _files.ContainsKey(path);
    public bool DirectoryExists(string path) => _directories.Contains(path);

    public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default) =>
        _files.TryGetValue(path, out var content)
            ? Task.FromResult(content)
            : Task.FromException<string>(new FileNotFoundException($"File not found: {path}"));

    public Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        _files[path] = content;
        return Task.CompletedTask;
    }

    public void CreateDirectory(string path) => _directories.Add(path);
    public void DeleteFile(string path) => _files.Remove(path);

    public void CopyFile(string sourcePath, string destinationPath, bool overwrite)
    {
        if (!_files.TryGetValue(sourcePath, out var content))
            throw new FileNotFoundException($"File not found: {sourcePath}");

        if (!overwrite && _files.ContainsKey(destinationPath))
            throw new IOException($"File already exists: {destinationPath}");

        _files[destinationPath] = content;
    }

    public IEnumerable<string> EnumerateFiles(string directory, string searchPattern, SearchOption searchOption) =>
        _files.Keys.Where(k => k.StartsWith(directory, StringComparison.OrdinalIgnoreCase));

    public string? GetWrittenContent(string path) =>
        _files.TryGetValue(path, out var c) ? c : null;
}
