namespace DotNetWind.Tailwind;

public sealed class TailwindRunner : ITailwindRunner
{
    private readonly IProcessRunner _processRunner;
    private readonly ILogger<TailwindRunner> _logger;

    public TailwindRunner(IProcessRunner processRunner, ILogger<TailwindRunner> logger)
    {
        _processRunner = processRunner;
        _logger = logger;
    }

    public async Task<Result> BuildAsync(
        TailwindPaths paths,
        bool minify,
        string workingDirectory,
        CancellationToken cancellationToken = default)
    {
        var script = minify ? "tw:build:min" : "tw:build";
        _logger.LogDebug("Running npm run {Script}", script);

        var result = await _processRunner.RunStreamingAsync(
            "npm",
            $"run {script}",
            workingDirectory,
            onOutput: line => _logger.LogDebug("[npm] {Line}", line),
            onError: line => _logger.LogDebug("[npm:err] {Line}", line),
            cancellationToken: cancellationToken);

        if (result.IsSuccess)
            return Result.Success();

        var errorKind = result.ExitCode == 127 ? ResultErrorKind.MissingDependency : ResultErrorKind.General;
        return Result.Failure($"Tailwind build failed (exit {result.ExitCode}):\n{result.StandardError}", errorKind);
    }

    public async Task<Result> WatchAsync(
        TailwindPaths paths,
        string workingDirectory,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Running npm run tw:watch");

        try
        {
            await _processRunner.RunStreamingAsync(
                "npm",
                "run tw:watch",
                workingDirectory,
                onOutput: line => Console.WriteLine(line),
                onError: line => Console.Error.WriteLine(line),
                cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Watch cancelled.");
        }

        return Result.Success();
    }
}
