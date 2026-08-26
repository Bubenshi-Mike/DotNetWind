namespace DotNetWind.Cli.Commands;

public static class UninstallCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("uninstall")
        {
            Description = "Remove DotNetWind-generated build configuration and CSS output"
        };

        var projectOption = new Option<string?>("--project")
        {
            Description = "Path to the .csproj file"
        };
        var inputOption = new Option<string>("--input")
        {
            Description = "Tailwind CSS input path",
            DefaultValueFactory = _ => "Styles/tailwind.css"
        };
        var outputOption = new Option<string>("--output")
        {
            Description = "CSS output path to remove",
            DefaultValueFactory = _ => "wwwroot/css/style.css"
        };
        var forceOption = new Option<bool>("--force")
        {
            Description = "Also remove the Tailwind input CSS file"
        };
        var dryRunOption = new Option<bool>("--dry-run")
        {
            Description = "Show what would be removed without changing files"
        };
        var noBackupOption = new Option<bool>("--no-backup")
        {
            Description = "Do not create .bak files before editing project/package files"
        };

        command.Options.Add(projectOption);
        command.Options.Add(inputOption);
        command.Options.Add(outputOption);
        command.Options.Add(forceOption);
        command.Options.Add(dryRunOption);
        command.Options.Add(noBackupOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var console = (IConsoleOutput)services.GetService(typeof(IConsoleOutput))!;
            var useCase = (UninstallUseCase)services.GetService(typeof(UninstallUseCase))!;

            console.WriteHeader("DotNetWind Uninstall");
            console.WriteAction("Removing DotNetWind setup...");

            var options = new UninstallOptions(
                ProjectPath: parseResult.GetValue(projectOption),
                InputCssRelativePath: parseResult.GetValue(inputOption)!,
                OutputCssRelativePath: parseResult.GetValue(outputOption)!,
                Force: parseResult.GetValue(forceOption),
                DryRun: parseResult.GetValue(dryRunOption),
                Backup: !parseResult.GetValue(noBackupOption));

            var result = await useCase.ExecuteAsync(options, cancellationToken);
            if (result.IsFailure)
            {
                console.WriteError(result.ErrorMessage!);
                return ToExitCode(result.ErrorKind);
            }

            if (options.DryRun)
                console.WriteSuccess("Dry run complete. No files were changed.");
            else
                console.WriteSuccess("DotNetWind setup removed");

            if (!options.Force && !options.DryRun)
                console.WriteInfo("Tailwind input CSS was left in place. Use --force to remove it.");

            return ExitCode.Success;
        });

        return command;
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
