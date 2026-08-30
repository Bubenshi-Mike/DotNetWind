using System.Diagnostics;

namespace DotNetWind.Cli.Tests;

public sealed class SmokeTests
{
    [Theory]
    [InlineData("blazor", "BlazorApp", null)]
    [InlineData("blazorwasm", "WasmApp", null)]
    [InlineData("mvc", "MvcApp", null)]
    [InlineData("webapp", "RazorPagesApp", null)]
    [InlineData("razorclasslib", "RclApp", "razor-class-library")]
    public async Task Init_WithGeneratedProject_ConfiguresTailwindFiles(string template, string projectName, string? framework)
    {
        using var workspace = TemporaryWorkspace.Create();
        var projectDirectory = Path.Combine(workspace.Path, projectName);
        var projectPath = Path.Combine(projectDirectory, $"{projectName}.csproj");

        await RunAsync("dotnet", $"new {template} -n {projectName} --no-restore", workspace.Path);

        var frameworkOption = framework is null ? "" : $" --framework {framework}";
        await RunAsync(
            "dotnet",
            $"{Quote(GetCliAssemblyPath())} init --project {Quote(projectPath)}{frameworkOption} --skip-npm-install --skip-build",
            GetRepositoryRoot());

        File.Exists(Path.Combine(projectDirectory, "Styles", "tailwind.css")).ShouldBeTrue();
        File.Exists(Path.Combine(projectDirectory, "package.json")).ShouldBeTrue();

        var projectContent = await File.ReadAllTextAsync(projectPath);
        projectContent.ShouldContain("BuildTailwind");

        var packageJson = await File.ReadAllTextAsync(Path.Combine(projectDirectory, "package.json"));
        packageJson.ShouldContain("tw:build");
        packageJson.ShouldContain("\"tailwindcss\": \"latest\"");
    }

    private static async Task RunAsync(string fileName, string arguments, string workingDirectory)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        process.ExitCode.ShouldBe(0, $"Command failed: {fileName} {arguments}{Environment.NewLine}{output}{Environment.NewLine}{error}");
    }

    private static string GetCliAssemblyPath() =>
        Path.Combine(GetRepositoryRoot(), "src", "DotNetWind.Cli", "bin", GetConfiguration(), "net10.0", "DotNetWind.Cli.dll");

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "DotNetWind.slnx")))
            directory = directory.Parent;

        return directory?.FullName ?? throw new InvalidOperationException("Could not find repository root.");
    }

    private static string GetConfiguration() =>
        AppContext.BaseDirectory.Contains($"{Path.DirectorySeparatorChar}Release{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
            ? "Release"
            : "Debug";

    private static string Quote(string value) => $"\"{value}\"";

    private sealed class TemporaryWorkspace : IDisposable
    {
        private TemporaryWorkspace(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public static TemporaryWorkspace Create()
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "dotnetwind-smoke-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return new TemporaryWorkspace(path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}
