using DotNetWind.Core.Models;

namespace DotNetWind.Core.Abstractions;

public interface IProjectDetector
{
    Task<Result<ProjectInfo>> DetectAsync(string? projectPath, CancellationToken cancellationToken = default);
}
