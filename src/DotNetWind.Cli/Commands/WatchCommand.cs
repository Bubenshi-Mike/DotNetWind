using System.CommandLine;
using DotNetWind.Cli.Output;
using DotNetWind.Core.Models;
using DotNetWind.Core.UseCases;

namespace DotNetWind.Cli.Commands;

public static class WatchCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("watch", "Watch Tailwind CSS for changes");

        var projectOption = new Option<string?>("--project", "Path to the .csproj file");
        var inputOption = new Option<string>("--input", () => "Styles/tailwind.css", "Tailwind CSS input path");
        var outputOption = new Option<string>("--output", () => "wwwroot/css/style.css", "CSS output path");

        command.AddOption(projectOption);
        command.AddOption(inputOption);
        command.AddOption(outputOption);

        command.SetHandler(async (context) =>
        {
            var project = context.ParseResult.GetValueForOption(projectOption);
            var input = context.ParseResult.GetValueForOption(inputOption)!;
            var output = context.ParseResult.GetValueForOption(outputOption)!;
            var cancellationToken = context.GetCancellationToken();

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
                context.ExitCode = ExitCode.GeneralFailure;
            }
        });

        return command;
    }
}
