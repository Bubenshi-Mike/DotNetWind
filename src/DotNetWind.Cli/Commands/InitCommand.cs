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
        var frameworkOption = new Option<string?>("--framework")
        {
            Description = "Project type when auto-detection is ambiguous: blazor-wasm, blazor-server, blazor-webapp, mvc, razor-pages"
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
        var skipNodeInstallOption = new Option<bool>("--skip-node-install")
        {
            Description = "Do not install Node.js automatically if node/npm are missing"
        };
        var skipBuildOption = new Option<bool>("--skip-build")
        {
            Description = "Skip running initial Tailwind build"
        };
        var yesOption = new Option<bool>("--yes")
        {
            Description = "Allow non-interactive installation of missing prerequisites"
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
        command.Options.Add(frameworkOption);
        command.Options.Add(inputOption);
        command.Options.Add(outputOption);
        command.Options.Add(skipNpmOption);
        command.Options.Add(skipNodeInstallOption);
        command.Options.Add(skipBuildOption);
        command.Options.Add(forceOption);
        command.Options.Add(yesOption);
        command.Options.Add(verboseOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var project = parseResult.GetValue(projectOption);
            var framework = parseResult.GetValue(frameworkOption);
            var input = parseResult.GetValue(inputOption)!;
            var output = parseResult.GetValue(outputOption)!;
            var skipNpm = parseResult.GetValue(skipNpmOption);
            var skipNodeInstall = parseResult.GetValue(skipNodeInstallOption);
            var skipBuild = parseResult.GetValue(skipBuildOption);
            var force = parseResult.GetValue(forceOption);
            var assumeYes = parseResult.GetValue(yesOption);
            var verbose = parseResult.GetValue(verboseOption);

            var console = (IConsoleOutput)services.GetService(typeof(IConsoleOutput))!;
            var useCase = (InitUseCase)services.GetService(typeof(InitUseCase))!;
            var hostFileDetector = (IHostFileDetector)services.GetService(typeof(IHostFileDetector))!;

            console.WriteHeader("DotNetWind Init");

            if (!TryParseProjectType(framework, out var forcedProjectType))
            {
                console.WriteError($"Unknown framework '{framework}'. Valid values: blazor-wasm, blazor-server, blazor-webapp, mvc, razor-pages.");
                return ExitCode.ValidationFailed;
            }

            var options = new SetupOptions(
                ProjectPath: project,
                ForcedProjectType: forcedProjectType,
                InputCssRelativePath: input,
                OutputCssRelativePath: output,
                SkipNpmInstall: skipNpm,
                SkipNodeInstall: skipNodeInstall,
                SkipBuild: skipBuild,
                Force: force,
                AssumeYes: assumeYes,
                Verbose: verbose);

            console.WriteAction("Detecting project...");
            var result = await useCase.ExecuteAsync(options, cancellationToken);

            if (result.IsFailure)
            {
                console.WriteError(result.ErrorMessage!);
                return ToExitCode(result.ErrorKind);
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
            var cssLink = projectInfo.GetCssLink(output);

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

    private static bool TryParseProjectType(string? value, out DotNetProjectType? projectType)
    {
        projectType = value?.Trim().ToLowerInvariant() switch
        {
            null or "" => null,
            "blazor-wasm" or "wasm" or "blazorwebassembly" => DotNetProjectType.BlazorWebAssembly,
            "blazor-server" or "server" or "blazorserver" => DotNetProjectType.BlazorServer,
            "blazor-webapp" or "blazor-web-app" or "webapp" or "blazorwebapp" => DotNetProjectType.BlazorWebApp,
            "mvc" => DotNetProjectType.Mvc,
            "razor-pages" or "razorpages" => DotNetProjectType.RazorPages,
            _ => null
        };

        return value is null || projectType is not null;
    }

    private static int ToExitCode(ResultErrorKind errorKind) => errorKind switch
    {
        ResultErrorKind.Validation => ExitCode.ValidationFailed,
        ResultErrorKind.UnsupportedProjectType => ExitCode.UnsupportedProjectType,
        ResultErrorKind.MissingDependency => ExitCode.MissingDependency,
        ResultErrorKind.UserCancelled => ExitCode.UserCancelled,
        _ => ExitCode.GeneralFailure
    };
}
