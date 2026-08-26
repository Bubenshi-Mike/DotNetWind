namespace DotNetWind.Tailwind;

public sealed class NodeJsInstaller : INodeJsInstaller
{
    private readonly IProcessRunner _processRunner;
    private readonly ILogger<NodeJsInstaller> _logger;

    public NodeJsInstaller(IProcessRunner processRunner, ILogger<NodeJsInstaller> logger)
    {
        _processRunner = processRunner;
        _logger = logger;
    }

    public async Task<Result> EnsureNodeAndNpmAsync(
        string workingDirectory,
        bool allowInstall,
        bool skipInstall,
        CancellationToken cancellationToken = default)
    {
        if (await HasNodeAndNpmAsync(workingDirectory, cancellationToken))
            return Result.Success();

        if (skipInstall)
            return Result.Failure("Node.js and npm are required for npm install or Tailwind builds. Install Node.js LTS from https://nodejs.org, or rerun with --skip-npm-install --skip-build.", ResultErrorKind.MissingDependency);

        if (!allowInstall)
            return Result.Failure("Node.js or npm was not found. To install Node.js LTS automatically, rerun with --yes. To configure files only, rerun with --skip-npm-install --skip-build.", ResultErrorKind.UserCancelled);

        _logger.LogInformation("Node.js or npm was not found. Attempting to install Node.js LTS.");

        var installResult = await InstallNodeLtsAsync(workingDirectory, cancellationToken);
        if (installResult.IsFailure)
            return installResult;

        return await HasNodeAndNpmAsync(workingDirectory, cancellationToken)
            ? Result.Success()
            : Result.Failure("Node.js LTS installation completed, but node/npm are still not available on PATH. Restart your terminal and run dotnetwind init again.", ResultErrorKind.MissingDependency);
    }

    private async Task<bool> HasNodeAndNpmAsync(string workingDirectory, CancellationToken cancellationToken)
    {
        var nodeResult = await _processRunner.RunAsync("node", "--version", workingDirectory, cancellationToken);
        if (!nodeResult.IsSuccess)
            return false;

        var npmResult = await _processRunner.RunAsync("npm", "--version", workingDirectory, cancellationToken);
        return npmResult.IsSuccess;
    }

    private async Task<Result> InstallNodeLtsAsync(string workingDirectory, CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsWindows())
        {
            var result = await _processRunner.RunStreamingAsync(
                "winget",
                "install --id OpenJS.NodeJS.LTS --exact --silent --accept-package-agreements --accept-source-agreements",
                workingDirectory,
                cancellationToken: cancellationToken);

            return result.IsSuccess
                ? Result.Success()
                : Result.Failure($"Could not install Node.js LTS with winget (exit {result.ExitCode}). Install Node.js LTS manually from https://nodejs.org, or rerun with --skip-npm-install --skip-build.", ResultErrorKind.MissingDependency);
        }

        return Result.Failure("Automatic Node.js LTS installation is currently supported on Windows with winget. Install Node.js LTS from https://nodejs.org, or rerun with --skip-npm-install --skip-build.", ResultErrorKind.MissingDependency);
    }
}
