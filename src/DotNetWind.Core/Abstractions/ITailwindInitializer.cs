using DotNetWind.Core.Models;

namespace DotNetWind.Core.Abstractions;

public interface ITailwindInitializer
{
    Task<Result> InitializeAsync(
        ProjectInfo project,
        TailwindPaths paths,
        SetupOptions options,
        CancellationToken cancellationToken = default);
}
