using Dapper;
using Domain.Abstractions;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("Connection string 'Default' is missing.");

        services.AddSingleton(new DbConnectionFactory(connectionString));
        services.AddScoped<DapperContext>();
        services.AddScoped<IOutboxService, OutboxService>();
        services.AddSingleton<IKafkaProducer, KafkaProducer>();

        return services;
    }
}
