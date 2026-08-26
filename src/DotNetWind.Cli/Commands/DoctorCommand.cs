namespace DotNetWind.Cli.Commands;

public static class DoctorCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("doctor")
        {
            Description = "Validate the Tailwind CSS setup"
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
        var jsonOption = new Option<bool>("--json")
        {
            Description = "Output results as JSON"
        };

        command.Options.Add(projectOption);
        command.Options.Add(inputOption);
        command.Options.Add(outputOption);
        command.Options.Add(jsonOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var project = parseResult.GetValue(projectOption);
            var input = parseResult.GetValue(inputOption)!;
            var output = parseResult.GetValue(outputOption)!;
            var asJson = parseResult.GetValue(jsonOption);

            var console = (IConsoleOutput)services.GetService(typeof(IConsoleOutput))!;
            var useCase = (DoctorUseCase)services.GetService(typeof(DoctorUseCase))!;

            if (!asJson)
                console.WriteHeader("DotNetWind Doctor");

            var options = new DoctorOptions(
                ProjectPath: project,
                InputCssRelativePath: input,
                OutputCssRelativePath: output);

            var checks = await useCase.ExecuteAsync(options, cancellationToken);

            if (asJson)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(checks, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });
                Console.WriteLine(json);
                return ExitCode.Success;
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
                return ExitCode.Success;
            }

            if (failCount == 0)
            {
                console.WriteWarning($"Setup has {warnCount} warning(s). Review the items above.");
                return ExitCode.Success;
            }

            console.WriteError($"Setup has {failCount} failure(s) and {warnCount} warning(s). Run 'dotnetwind init' to fix.");
            return ExitCode.ValidationFailed;
        });

        return command;
    }
}
