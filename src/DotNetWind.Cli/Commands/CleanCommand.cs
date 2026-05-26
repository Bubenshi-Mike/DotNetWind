using System.CommandLine;
using DotNetWind.Cli.Output;
using DotNetWind.Core.Models;
using DotNetWind.Core.UseCases;

namespace DotNetWind.Cli.Commands;

public static class CleanCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("clean", "Remove generated Tailwind CSS output files");

        var projectOption = new Option<string?>("--project", "Path to the .csproj file");
        var outputOption = new Option<string>("--output", () => "wwwroot/css/style.css", "CSS output path to remove");

        command.AddOption(projectOption);
        command.AddOption(outputOption);

        command.SetHandler(async (context) =>
        {
            var project = context.ParseResult.GetValueForOption(projectOption);
            var output = context.ParseResult.GetValueForOption(outputOption)!;
            var cancellationToken = context.GetCancellationToken();

            var console = (IConsoleOutput)services.GetService(typeof(IConsoleOutput))!;
            var useCase = (CleanUseCase)services.GetService(typeof(CleanUseCase))!;

            console.WriteHeader("DotNetWind Clean");

            var options = new CleanOptions(ProjectPath: project, OutputCssRelativePath: output);
            var result = await useCase.ExecuteAsync(options, cancellationToken);

            if (result.IsFailure)
            {
                console.WriteError(result.ErrorMessage!);
                context.ExitCode = ExitCode.GeneralFailure;
                return;
            }

            console.WriteSuccess($"Removed: {result.Value}");
        });

        return command;
    }
}
