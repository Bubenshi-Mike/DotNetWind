namespace DotNetWind.Cli.Commands;

public static class CleanCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("clean")
        {
            Description = "Remove generated Tailwind CSS output files"
        };

        var projectOption = new Option<string?>("--project")
        {
            Description = "Path to the .csproj file"
        };
        var outputOption = new Option<string>("--output")
        {
            Description = "CSS output path to remove",
            DefaultValueFactory = _ => "wwwroot/css/style.css"
        };

        command.Options.Add(projectOption);
        command.Options.Add(outputOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var project = parseResult.GetValue(projectOption);
            var output = parseResult.GetValue(outputOption)!;

            var console = (IConsoleOutput)services.GetService(typeof(IConsoleOutput))!;
            var useCase = (CleanUseCase)services.GetService(typeof(CleanUseCase))!;

            console.WriteHeader("DotNetWind Clean");

            var options = new CleanOptions(ProjectPath: project, OutputCssRelativePath: output);
            var result = await useCase.ExecuteAsync(options, cancellationToken);

            if (result.IsFailure)
            {
                console.WriteError(result.ErrorMessage!);
                return ExitCode.GeneralFailure;
            }

            console.WriteSuccess($"Removed: {result.Value}");
            return ExitCode.Success;
        });

        return command;
    }
}
