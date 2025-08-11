using Microsoft.Extensions.DependencyInjection;

namespace CyanDevelopers.SimpleIntegratedDB.Services;
public static class DependencyInjectionExtensions
{
    public static void AddIntegratedDatabaseContext<Db>(this IServiceCollection services) where Db : Database =>
        // TODO Ctor validation
        services.AddSingleton<Db>();
}