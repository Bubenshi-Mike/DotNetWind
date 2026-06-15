namespace DotNetWind.Cli.Commands;

public static class WatchCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("watch")
        {
            Description = "Watch Tailwind CSS for changes"
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
            Description = "CSS output path",
            DefaultValueFactory = _ => "wwwroot/css/style.css"
        };

        command.Options.Add(projectOption);
        command.Options.Add(inputOption);
        command.Options.Add(outputOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var project = parseResult.GetValue(projectOption);
            var input = parseResult.GetValue(inputOption)!;
            var output = parseResult.GetValue(outputOption)!;

            var console = (IConsoleOutput)services.GetService(typeof(IConsoleOutput))!;
            var useCase = (WatchUseCase)services.GetService(typeof(WatchUseCase))!;

            console.WriteHeader("DotNetWind Watch");
            console.WriteAction("Starting Tailwind CSS watch mode... (Ctrl+C to stop)");
            console.WriteLine();

            var options = new WatchOptions(
                ProjectPath: project,
                InputCssRelativePath: input,
                OutputCssRelativePath: output);

            var result = await useCase.ExecuteAsync(options, cancellationToken);

            if (result.IsFailure)
            {
                console.WriteError(result.ErrorMessage!);
                return ExitCode.GeneralFailure;
            }

            return ExitCode.Success;
        });

        return command;
    }
}
