using System.CommandLine;
using DotNetWind.Cli.Output;
using DotNetWind.Core.Models;
using DotNetWind.Core.UseCases;

namespace DotNetWind.Cli.Commands;

public static class DoctorCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("doctor", "Validate the Tailwind CSS setup");

        var projectOption = new Option<string?>("--project", "Path to the .csproj file");
        var jsonOption = new Option<bool>("--json", "Output results as JSON");

        command.AddOption(projectOption);
        command.AddOption(jsonOption);

        command.SetHandler(async (context) =>
        {
            var project = context.ParseResult.GetValueForOption(projectOption);
            var asJson = context.ParseResult.GetValueForOption(jsonOption);
            var cancellationToken = context.GetCancellationToken();

            var console = (IConsoleOutput)services.GetService(typeof(IConsoleOutput))!;
            var useCase = (DoctorUseCase)services.GetService(typeof(DoctorUseCase))!;

            if (!asJson)
                console.WriteHeader("DotNetWind Doctor");

            var checks = await useCase.ExecuteAsync(project, cancellationToken);

            if (asJson)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(checks, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });
                Console.WriteLine(json);
                return;
            }

            foreach (var check in checks)
            {
                switch (check.Status)
                {
                    case DoctorStatus.Pass:
                        console.WriteSuccess($"{check.Name}: {check.Message}");
                        break;
                    case DoctorStatus.Warning:
                        console.WriteWarning($"{check.Name}: {check.Message}");
                        if (check.Recommendation is not null)
                            console.WriteInfo($"  → {check.Recommendation}");
                        break;
                    case DoctorStatus.Fail:
                        console.WriteError($"{check.Name}: {check.Message}");
                        if (check.Recommendation is not null)
                            console.WriteInfo($"  → {check.Recommendation}");
                        break;
                }
            }

            console.WriteLine();
            console.WriteRule();

            var failCount = checks.Count(c => c.IsFail);
            var warnCount = checks.Count(c => c.IsWarning);

            if (failCount == 0 && warnCount == 0)
            {
                console.WriteSuccess("Tailwind CSS appears to be correctly configured.");
                context.ExitCode = ExitCode.Success;
            }
            else if (failCount == 0)
            {
                console.WriteWarning($"Setup has {warnCount} warning(s). Review the items above.");
                context.ExitCode = ExitCode.Success;
            }
            else
            {
                console.WriteError($"Setup has {failCount} failure(s) and {warnCount} warning(s). Run 'dotnetwind init' to fix.");
                context.ExitCode = ExitCode.ValidationFailed;
            }
        });

        return command;
    }
}
