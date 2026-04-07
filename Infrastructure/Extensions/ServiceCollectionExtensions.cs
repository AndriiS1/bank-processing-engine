using Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("Connection string 'Default' is missing.");;

        services.AddSingleton(new DbConnectionFactory(connectionString));
        services.AddScoped<DapperContext>();

        return services;
    }
}
