namespace DotNetWind.Cli.Commands;

public static class InfoCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("info")
        {
            Description = "Display project and tool information"
        };

        var projectOption = new Option<string?>("--project")
        {
            Description = "Path to the .csproj file"
        };

        command.Options.Add(projectOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var project = parseResult.GetValue(projectOption);

            var console = (IConsoleOutput)services.GetService(typeof(IConsoleOutput))!;
            var useCase = (InfoUseCase)services.GetService(typeof(InfoUseCase))!;

            console.WriteHeader("DotNetWind Info");

            var result = await useCase.ExecuteAsync(project, cancellationToken: cancellationToken);

            if (result.IsFailure)
            {
                console.WriteError(result.ErrorMessage!);
                return ExitCode.GeneralFailure;
            }

            var report = result.Value!;
            console.WriteTable(
            [
                ("Project", report.Project.ProjectName),
                ("Project type", report.Project.GetDisplayName()),
                ("Target framework", report.Project.TargetFramework ?? "unknown"),
                ("Tailwind input", report.InputCssPath),
                ("Tailwind output", report.OutputCssPath),
                ("Node.js", report.NodeVersion ?? "not found"),
                ("npm", report.NpmVersion ?? "not found"),
                ("Tailwind CLI", report.TailwindCliInstalled ? "installed" : "not installed"),
            ]);

            return ExitCode.Success;
        });

        return command;
    }
}
