using DotNetWind.Core.Models;

namespace DotNetWind.Core.Abstractions;

public interface IProjectFileUpdater
{
    Task<Result> AddTailwindBuildTargetAsync(
        ProjectInfo project,
        TailwindPaths paths,
        CancellationToken cancellationToken = default);

    Task<bool> HasTailwindBuildTargetAsync(
        string projectFilePath,
        CancellationToken cancellationToken = default);
}
