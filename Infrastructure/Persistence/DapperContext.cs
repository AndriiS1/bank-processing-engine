using System.Data.Common;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public partial class DapperContext(ILogger<DapperContext> logger, DbConnectionFactory factory)
{
    public async Task<T> WithConnection<T>(
        Func<DbConnection, Task<T>> action,
        int maxRetries = 3,
        TimeSpan? delay = null)
    {
        delay ??= TimeSpan.FromSeconds(2);
        var attempt = 1;

        for (; attempt <= maxRetries; attempt++)
        {
            try
            {
                await using var connection = factory.CreateConnection() as DbConnection 
                                             ?? throw new InvalidOperationException("Connection is not a DbConnection");

                await connection.OpenAsync();
                return await action(connection);
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                LogRetryConnectionMessage(attempt, ex.Message, delay.Value.TotalSeconds);
                await Task.Delay(delay.Value);
            }
        }
        
        await using var finalConnection = factory.CreateConnection() as DbConnection 
                                          ?? throw new InvalidOperationException("Connection is not a DbConnection");
        await finalConnection.OpenAsync();
        return await action(finalConnection);
    }
    
    public async Task WithTransaction(Func<DbConnection, DbTransaction, Task> action)
    {
        await WithConnection(async connection =>
        {
            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                await action(connection, transaction);
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
    
    [LoggerMessage(LogLevel.Warning, "Attempt {Attempt} failed: {ExMessage}. Retrying in {ValueTotalSeconds}s...")]
    partial void LogRetryConnectionMessage(int attempt, string exMessage, double valueTotalSeconds);
}