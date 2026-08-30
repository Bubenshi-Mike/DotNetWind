using DotNetWind.Cli.Commands;
using DotNetWind.Cli.DependencyInjection;

var verboseOption = new Option<bool>("--verbose")
{
    Description = "Show detailed diagnostic output"
};

var rootCommand = new RootCommand("DotNetWind — Tailwind CSS setup for .NET, without the pain.")
{
    verboseOption
};

var verbose = rootCommand.Parse(args).GetValue(verboseOption);

var services = new ServiceCollection()
    .AddDotNetWind(verbose)
    .BuildServiceProvider();

rootCommand.Subcommands.Add(InitCommand.Create(services));
rootCommand.Subcommands.Add(BuildCommand.Create(services));
rootCommand.Subcommands.Add(WatchCommand.Create(services));
rootCommand.Subcommands.Add(DoctorCommand.Create(services));
rootCommand.Subcommands.Add(CleanCommand.Create(services));
rootCommand.Subcommands.Add(InfoCommand.Create(services));
rootCommand.Subcommands.Add(RepairCommand.Create(services));
rootCommand.Subcommands.Add(UpdateCommand.Create(services));
rootCommand.Subcommands.Add(UninstallCommand.Create(services));

return await rootCommand.Parse(args).InvokeAsync();
