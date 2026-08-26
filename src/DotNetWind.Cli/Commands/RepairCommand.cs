namespace DotNetWind.Cli.Commands;

public static class RepairCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("repair")
        {
            Description = "Re-apply the DotNetWind setup to repair missing generated files or configuration"
        };

        var projectOption = new Option<string?>("--project")
        {
            Description = "Path to the .csproj file"
        };
        var frameworkOption = new Option<string?>("--framework")
        {
            Description = "Project type when auto-detection is ambiguous: blazor-wasm, blazor-server, blazor-webapp, mvc, razor-pages, razor-class-library"
        };
        var inputOption = new Option<string>("--input")
        {
            Description = "Tailwind CSS input path",
            DefaultValueFactory = _ => "Styles/tailwind.css"
        };
        var outputOption = new Option<string>("--output")
        {
            Description = "CSS output path",
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
            Description = "Skip running Tailwind build"
        };
        var forceOption = new Option<bool>("--force")
        {
            Description = "Overwrite existing Tailwind input file"
        };
        var yesOption = new Option<bool>("--yes")
        {
            Description = "Allow non-interactive installation of missing prerequisites"
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

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var console = (IConsoleOutput)services.GetService(typeof(IConsoleOutput))!;
            var useCase = (RepairUseCase)services.GetService(typeof(RepairUseCase))!;

            console.WriteHeader("DotNetWind Repair");
            console.WriteAction("Re-applying DotNetWind setup...");

            var framework = parseResult.GetValue(frameworkOption);
            if (!TryParseProjectType(framework, out var forcedProjectType))
            {
                console.WriteError($"Unknown framework '{framework}'. Valid values: blazor-wasm, blazor-server, blazor-webapp, mvc, razor-pages, razor-class-library.");
                return ExitCode.ValidationFailed;
            }

            var options = new SetupOptions(
                ProjectPath: parseResult.GetValue(projectOption),
                ForcedProjectType: forcedProjectType,
                InputCssRelativePath: parseResult.GetValue(inputOption)!,
                OutputCssRelativePath: parseResult.GetValue(outputOption)!,
                SkipNpmInstall: parseResult.GetValue(skipNpmOption),
                SkipNodeInstall: parseResult.GetValue(skipNodeInstallOption),
                SkipBuild: parseResult.GetValue(skipBuildOption),
                Force: parseResult.GetValue(forceOption),
                AssumeYes: parseResult.GetValue(yesOption));

            var result = await useCase.ExecuteAsync(options, cancellationToken);
            if (result.IsFailure)
            {
                console.WriteError(result.ErrorMessage!);
                return ToExitCode(result.ErrorKind);
            }

            console.WriteSuccess($"Repaired: {result.Value!.ProjectName}");
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
            "razor-class-library" or "razorclasslibrary" or "rcl" or "razor-sdk" => DotNetProjectType.RazorClassLibrary,
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
