namespace DotNetWind.Tailwind.Infrastructure;

public sealed class FileSystem : IFileSystem
{
    public bool FileExists(string path) => File.Exists(path);

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default) =>
        await File.ReadAllTextAsync(path, cancellationToken);

    public async Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default) =>
        await File.WriteAllTextAsync(path, content, cancellationToken);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public void DeleteFile(string path) => File.Delete(path);

    public IEnumerable<string> EnumerateFiles(string directory, string searchPattern, SearchOption searchOption) =>
        Directory.EnumerateFiles(directory, searchPattern, searchOption);
}
