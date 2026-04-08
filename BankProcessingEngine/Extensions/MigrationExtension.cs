using Infrastructure.Persistence;
namespace BankProcessingEngine.Extensions;

public static class MigrationExtensions
{
    public static WebApplication ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        
        var configuration = services.GetRequiredService<IConfiguration>();
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("MigrationRunner");

        var connectionString = configuration.GetConnectionString("Default") 
                               ?? throw new InvalidOperationException("Connection string 'Default' is missing.");

        try
        {
            MigrationRunner.Run(connectionString, logger);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An error occurred while migrating the database.");
            Environment.Exit(-1);
        }

        return app;
    }
}
