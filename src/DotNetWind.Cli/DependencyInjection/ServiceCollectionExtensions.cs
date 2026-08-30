using DotNetWind.ProjectSystem;
using DotNetWind.Tailwind;
using Microsoft.Extensions.Logging;

namespace DotNetWind.Cli.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDotNetWind(this IServiceCollection services, bool verbose)
    {
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(verbose ? LogLevel.Debug : LogLevel.Warning);
            builder.AddConsole();
        });

        services.AddSingleton<IConsoleOutput, SpectreConsoleOutput>();

        services.AddProjectSystem();
        services.AddTailwindServices();

        services.AddTransient<InitUseCase>();
        services.AddTransient<BuildUseCase>();
        services.AddTransient<WatchUseCase>();
        services.AddTransient<DoctorUseCase>();
        services.AddTransient<CleanUseCase>();
        services.AddTransient<InfoUseCase>();
        services.AddTransient<RepairUseCase>();
        services.AddTransient<UpdateUseCase>();
        services.AddTransient<UninstallUseCase>();

        return services;
    }
}
