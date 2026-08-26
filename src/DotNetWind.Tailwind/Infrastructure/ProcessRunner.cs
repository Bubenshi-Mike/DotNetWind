using System.Diagnostics;

namespace DotNetWind.Tailwind.Infrastructure;

public sealed class ProcessRunner : IProcessRunner
{
    private readonly ILogger<ProcessRunner> _logger;

    public ProcessRunner(ILogger<ProcessRunner> logger)
    {
        _logger = logger;
    }

    public async Task<ProcessResult> RunAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        CancellationToken cancellationToken = default)
    {
        var resolvedFileName = ResolveExecutable(fileName);
        _logger.LogDebug("Running: {FileName} {Arguments} in {WorkingDirectory}", resolvedFileName, arguments, workingDirectory);

        using var process = new Process
        {
            StartInfo = BuildStartInfo(resolvedFileName, arguments, workingDirectory)
        };

        try
        {
            process.Start();
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            return new ProcessResult(127, "", ex.Message);
        }

        var stdOut = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stdErr = await process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        return new ProcessResult(process.ExitCode, stdOut, stdErr);
    }

    public async Task<ProcessResult> RunStreamingAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        Action<string>? onOutput = null,
        Action<string>? onError = null,
        CancellationToken cancellationToken = default)
    {
        var resolvedFileName = ResolveExecutable(fileName);
        _logger.LogDebug("Running streaming: {FileName} {Arguments}", resolvedFileName, arguments);

        using var process = new Process
        {
            StartInfo = BuildStartInfo(resolvedFileName, arguments, workingDirectory),
            EnableRaisingEvents = true
        };

        var outputBuilder = new System.Text.StringBuilder();
        var errorBuilder = new System.Text.StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is null) return;
            outputBuilder.AppendLine(e.Data);
            onOutput?.Invoke(e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is null) return;
            errorBuilder.AppendLine(e.Data);
            onError?.Invoke(e.Data);
        };

        try
        {
            process.Start();
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            return new ProcessResult(127, "", ex.Message);
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw;
        }

        return new ProcessResult(process.ExitCode, outputBuilder.ToString(), errorBuilder.ToString());
    }

    private static ProcessStartInfo BuildStartInfo(string fileName, string arguments, string workingDirectory) =>
        new()
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

    private static string ResolveExecutable(string fileName)
    {
        if (OperatingSystem.IsWindows() && !fileName.Contains('.'))
            return fileName + ".cmd";
        return fileName;
    }
}
