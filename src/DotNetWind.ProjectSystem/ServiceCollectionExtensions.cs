namespace DotNetWind.ProjectSystem;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjectSystem(this IServiceCollection services)
    {
        services.AddSingleton<IProjectDetector, ProjectDetector>();
        services.AddSingleton<IProjectFileUpdater, ProjectFileUpdater>();
        services.AddSingleton<IHostFileDetector, HostFileDetector>();
        return services;
    }
}
