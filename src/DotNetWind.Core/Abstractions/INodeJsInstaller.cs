namespace DotNetWind.Core.Abstractions;

public interface INodeJsInstaller
{
    Task<Result> EnsureNodeAndNpmAsync(
        string workingDirectory,
        bool allowInstall,
        bool skipInstall,
        CancellationToken cancellationToken = default);
}
