using DotNetWind.Core.Models;

namespace DotNetWind.Core.Abstractions;

public interface IHostFileDetector
{
    string? FindHostFile(ProjectInfo project);
    bool HasCssReference(string hostFilePath, string cssRelativePath);
}
