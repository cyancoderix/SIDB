using Microsoft.Extensions.DependencyInjection;

namespace CyanDevelopers.SimpleIntegratedDB.Services;
/// <summary>
/// The extensions to DI from SimpleIntegratedDB.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds your database to the IServiceCollection as a singleton.
    /// </summary>
    /// <typeparam name="Db">Your database class.</typeparam>
    /// <param name="services">The IServiceCollection object.</param>
    public static void AddIntegratedDatabaseContext<Db>(this IServiceCollection services) where Db : Database =>
        // TODO Ctor validation
        services.AddSingleton<Db>();
}