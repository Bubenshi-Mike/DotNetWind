namespace DotNetWind.Cli.Commands;

public static class BuildCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("build")
        {
            Description = "Build Tailwind CSS"
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
        var minifyOption = new Option<bool>("--minify")
        {
            Description = "Minify the output CSS"
        };
        var verboseOption = new Option<bool>("--verbose")
        {
            Description = "Show detailed output"
        };

        command.Options.Add(projectOption);
        command.Options.Add(inputOption);
        command.Options.Add(outputOption);
        command.Options.Add(minifyOption);
        command.Options.Add(verboseOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var project = parseResult.GetValue(projectOption);
            var input = parseResult.GetValue(inputOption)!;
            var output = parseResult.GetValue(outputOption)!;
            var minify = parseResult.GetValue(minifyOption);
            var verbose = parseResult.GetValue(verboseOption);

            var console = (IConsoleOutput)services.GetService(typeof(IConsoleOutput))!;
            var useCase = (BuildUseCase)services.GetService(typeof(BuildUseCase))!;

            console.WriteHeader("DotNetWind Build");
            console.WriteAction(minify ? "Building Tailwind CSS (minified)..." : "Building Tailwind CSS...");

            var options = new BuildOptions(
                ProjectPath: project,
                InputCssRelativePath: input,
                OutputCssRelativePath: output,
                Minify: minify,
                Verbose: verbose);

            var result = await useCase.ExecuteAsync(options, cancellationToken);

            if (result.IsFailure)
            {
                console.WriteError(result.ErrorMessage!);
                return ExitCode.GeneralFailure;
            }

            console.WriteSuccess($"Tailwind CSS built → {output}");
            return ExitCode.Success;
        });

        return command;
    }
}
