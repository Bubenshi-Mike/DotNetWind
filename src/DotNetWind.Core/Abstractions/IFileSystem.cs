namespace DotNetWind.Core.Abstractions;

public interface IFileSystem
{
    bool FileExists(string path);
    bool DirectoryExists(string path);
    Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default);
    Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default);
    void CreateDirectory(string path);
    void DeleteFile(string path);
    IEnumerable<string> EnumerateFiles(string directory, string searchPattern, SearchOption searchOption);
}
