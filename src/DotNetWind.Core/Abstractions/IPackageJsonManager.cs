using DotNetWind.Core.Models;

namespace DotNetWind.Core.Abstractions;

public interface IPackageJsonManager
{
    Task<Result> CreateOrMergeAsync(
        string packageJsonPath,
        string inputCssRelativePath,
        string outputCssRelativePath,
        CancellationToken cancellationToken = default);

    Task<bool> HasTailwindScriptsAsync(string packageJsonPath, CancellationToken cancellationToken = default);
    Task<bool> HasTailwindDependenciesAsync(string packageJsonPath, CancellationToken cancellationToken = default);
}
