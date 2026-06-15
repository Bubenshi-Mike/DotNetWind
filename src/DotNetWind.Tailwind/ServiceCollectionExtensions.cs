using DotNetWind.Tailwind.Infrastructure;

namespace DotNetWind.Tailwind;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTailwindServices(this IServiceCollection services)
    {
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IProcessRunner, ProcessRunner>();
        services.AddSingleton<IPackageJsonManager, PackageJsonManager>();
        services.AddSingleton<ITailwindInitializer, TailwindInitializer>();
        services.AddSingleton<ITailwindRunner, TailwindRunner>();
        services.AddSingleton<IDoctorService, DoctorService>();
        return services;
    }
}
