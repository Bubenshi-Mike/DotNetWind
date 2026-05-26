using System.CommandLine;
using DotNetWind.Cli.Output;
using DotNetWind.Core.Abstractions;
using DotNetWind.Core.Models;
using DotNetWind.Core.UseCases;

namespace DotNetWind.Cli.Commands;

public static class InitCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("init", "Configure Tailwind CSS in the current .NET project");

        var projectOption = new Option<string?>("--project", "Path to the .csproj file");
        var inputOption = new Option<string>("--input", () => "Styles/tailwind.css", "Tailwind CSS input path (relative to project)");
        var outputOption = new Option<string>("--output", () => "wwwroot/css/style.css", "CSS output path (relative to project)");
        var skipNpmOption = new Option<bool>("--skip-npm-install", "Skip running npm install");
        var skipBuildOption = new Option<bool>("--skip-build", "Skip running initial Tailwind build");
        var forceOption = new Option<bool>("--force", "Overwrite existing files");
        var verboseOption = new Option<bool>("--verbose", "Show detailed output");

        command.AddOption(projectOption);
        command.AddOption(inputOption);
        command.AddOption(outputOption);
        command.AddOption(skipNpmOption);
        command.AddOption(skipBuildOption);
        command.AddOption(forceOption);
        command.AddOption(verboseOption);

        command.SetHandler(async (context) =>
        {
            var project = context.ParseResult.GetValueForOption(projectOption);
            var input = context.ParseResult.GetValueForOption(inputOption)!;
            var output = context.ParseResult.GetValueForOption(outputOption)!;
            var skipNpm = context.ParseResult.GetValueForOption(skipNpmOption);
            var skipBuild = context.ParseResult.GetValueForOption(skipBuildOption);
            var force = context.ParseResult.GetValueForOption(forceOption);
            var verbose = context.ParseResult.GetValueForOption(verboseOption);
            var cancellationToken = context.GetCancellationToken();

            var console = (IConsoleOutput)services.GetService(typeof(IConsoleOutput))!;
            var useCase = (InitUseCase)services.GetService(typeof(InitUseCase))!;
            var hostFileDetector = (IHostFileDetector)services.GetService(typeof(IHostFileDetector))!;

            console.WriteHeader("DotNetWind Init");

            var options = new SetupOptions(
                ProjectPath: project,
                InputCssRelativePath: input,
                OutputCssRelativePath: output,
                SkipNpmInstall: skipNpm,
                SkipBuild: skipBuild,
                Force: force,
                Verbose: verbose);

            console.WriteAction("Detecting project...");
            var result = await useCase.ExecuteAsync(options, cancellationToken);

            if (result.IsFailure)
            {
                console.WriteError(result.ErrorMessage!);
                context.ExitCode = ExitCode.GeneralFailure;
                return;
            }

            var projectInfo = result.Value!;
            console.WriteSuccess($"Project: {projectInfo.ProjectName} ({projectInfo.GetDisplayName()})");
            console.WriteSuccess($"Framework: {projectInfo.TargetFramework ?? "unknown"}");
            console.WriteSuccess("Styles/tailwind.css created");
            console.WriteSuccess("package.json configured");
            console.WriteSuccess("MSBuild target added to .csproj");

            if (!skipNpm)
                console.WriteSuccess("npm install completed");

            console.WriteLine();
            console.WriteHeader("Next Steps");

            var hostFile = hostFileDetector.FindHostFile(projectInfo);
            var cssLink = projectInfo.GetCssLink();

            if (hostFile is not null)
            {
                console.WriteInfo($"Add the following to {Path.GetFileName(hostFile)}:");
                console.WriteLine($"  {cssLink}");
            }
            else
            {
                console.WriteWarning("Could not detect host file. Add the CSS link manually:");
                console.WriteLine($"  {cssLink}");
            }

            console.WriteLine();
            console.WriteInfo("Run 'dotnetwind build' to build Tailwind CSS");
            console.WriteInfo("Run 'dotnetwind watch' to watch for changes");
            console.WriteInfo("Run 'dotnetwind doctor' to validate your setup");
        });

        return command;
    }
}
