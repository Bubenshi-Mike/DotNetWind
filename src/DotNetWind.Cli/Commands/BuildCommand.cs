using System.CommandLine;
using DotNetWind.Cli.Output;
using DotNetWind.Core.Models;
using DotNetWind.Core.UseCases;

namespace DotNetWind.Cli.Commands;

public static class BuildCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("build", "Build Tailwind CSS");

        var projectOption = new Option<string?>("--project", "Path to the .csproj file");
        var inputOption = new Option<string>("--input", () => "Styles/tailwind.css", "Tailwind CSS input path");
        var outputOption = new Option<string>("--output", () => "wwwroot/css/style.css", "CSS output path");
        var minifyOption = new Option<bool>("--minify", "Minify the output CSS");
        var verboseOption = new Option<bool>("--verbose", "Show detailed output");

        command.AddOption(projectOption);
        command.AddOption(inputOption);
        command.AddOption(outputOption);
        command.AddOption(minifyOption);
        command.AddOption(verboseOption);

        command.SetHandler(async (context) =>
        {
            var project = context.ParseResult.GetValueForOption(projectOption);
            var input = context.ParseResult.GetValueForOption(inputOption)!;
            var output = context.ParseResult.GetValueForOption(outputOption)!;
            var minify = context.ParseResult.GetValueForOption(minifyOption);
            var verbose = context.ParseResult.GetValueForOption(verboseOption);
            var cancellationToken = context.GetCancellationToken();

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
                context.ExitCode = ExitCode.GeneralFailure;
                return;
            }

            console.WriteSuccess($"Tailwind CSS built → {output}");
        });

        return command;
    }
}
