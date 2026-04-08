using DbUp;
using DbUp.Helpers;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public static partial class MigrationRunner
{
    public static void Run(string connectionString, ILogger logger)
    {
        logger.LogMigrationStarted(HidePassword(connectionString));

        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsFromFileSystem("/app/migrations_folder")
            .JournalTo(new NullJournal())
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            logger.LogError(result.Error, "Database upgrade failed.");
            throw result.Error;
        }

        logger.LogInformation("Database upgrade completed successfully.");
    }
    
    private static string HidePassword(string connectionString)
    {
        var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString)
        {
            Password = "********"
        };
        
        return builder.ToString();
    }
    
    [LoggerMessage(LogLevel.Information, "Beginning database upgrade for connection: {ConnectionString}")]
    static partial void LogMigrationStarted(this ILogger logger, string connectionString);
}
