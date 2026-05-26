using System.CommandLine;
using DotNetWind.Cli.Commands;
using DotNetWind.Cli.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

var verboseOption = new Option<bool>("--verbose", "Show detailed diagnostic output");
var rootCommand = new RootCommand("DotNetWind — Tailwind CSS setup for .NET, without the pain.")
{
    verboseOption
};

// We need to resolve verbosity before building DI, so we parse it first.
var parseResult = rootCommand.Parse(args);
var verbose = parseResult.GetValueForOption(verboseOption);

var services = new ServiceCollection()
    .AddDotNetWind(verbose)
    .BuildServiceProvider();

rootCommand.AddCommand(InitCommand.Create(services));
rootCommand.AddCommand(BuildCommand.Create(services));
rootCommand.AddCommand(WatchCommand.Create(services));
rootCommand.AddCommand(DoctorCommand.Create(services));
rootCommand.AddCommand(CleanCommand.Create(services));
rootCommand.AddCommand(InfoCommand.Create(services));

return await rootCommand.InvokeAsync(args);
