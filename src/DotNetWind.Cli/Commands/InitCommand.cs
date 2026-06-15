using DotNetWind.Core.Abstractions;

namespace DotNetWind.Cli.Commands;

public static class InitCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("init")
        {
            Description = "Configure Tailwind CSS in the current .NET project"
        };

        var projectOption = new Option<string?>("--project")
        {
            Description = "Path to the .csproj file"
        };
        var inputOption = new Option<string>("--input")
        {
            Description = "Tailwind CSS input path (relative to project)",
            DefaultValueFactory = _ => "Styles/tailwind.css"
        };
        var outputOption = new Option<string>("--output")
        {
            Description = "CSS output path (relative to project)",
            DefaultValueFactory = _ => "wwwroot/css/style.css"
        };
        var skipNpmOption = new Option<bool>("--skip-npm-install")
        {
            Description = "Skip running npm install"
        };
        var skipBuildOption = new Option<bool>("--skip-build")
        {
            Description = "Skip running initial Tailwind build"
        };
        var forceOption = new Option<bool>("--force")
        {
            Description = "Overwrite existing files"
        };
        var verboseOption = new Option<bool>("--verbose")
        {
            Description = "Show detailed output"
        };

        command.Options.Add(projectOption);
        command.Options.Add(inputOption);
        command.Options.Add(outputOption);
        command.Options.Add(skipNpmOption);
        command.Options.Add(skipBuildOption);
        command.Options.Add(forceOption);
        command.Options.Add(verboseOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var project = parseResult.GetValue(projectOption);
            var input = parseResult.GetValue(inputOption)!;
            var output = parseResult.GetValue(outputOption)!;
            var skipNpm = parseResult.GetValue(skipNpmOption);
            var skipBuild = parseResult.GetValue(skipBuildOption);
            var force = parseResult.GetValue(forceOption);
            var verbose = parseResult.GetValue(verboseOption);

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
                return ExitCode.GeneralFailure;
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

            return ExitCode.Success;
        });

        return command;
    }
}
