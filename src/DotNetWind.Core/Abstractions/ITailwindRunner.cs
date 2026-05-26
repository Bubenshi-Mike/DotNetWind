using DotNetWind.Core.Models;

namespace DotNetWind.Core.Abstractions;

public interface ITailwindRunner
{
    Task<Result> BuildAsync(
        TailwindPaths paths,
        bool minify,
        string workingDirectory,
        CancellationToken cancellationToken = default);

    Task<Result> WatchAsync(
        TailwindPaths paths,
        string workingDirectory,
        CancellationToken cancellationToken = default);
}
