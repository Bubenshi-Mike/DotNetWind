namespace DotNetWind.Core.Abstractions;

public interface IProcessRunner
{
    Task<ProcessResult> RunAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        CancellationToken cancellationToken = default);

    Task<ProcessResult> RunStreamingAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        Action<string>? onOutput = null,
        Action<string>? onError = null,
        CancellationToken cancellationToken = default);
}
