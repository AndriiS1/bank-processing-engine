using System.Text.Json;
using Confluent.Kafka;
using Dapper;
using Domain.Models;
using Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public partial class Consumer(
    IConfiguration config, 
    IServiceScopeFactory scopeFactory, 
    ILogger<Consumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            GroupId = "payment-processor-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        consumer.Subscribe("payment-events");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(ct);
                if (result == null) continue;

                await HandleMessage(result, ct);
                
                consumer.Commit(result); 
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                logger.LogError(ex, "Consumer loop encountered an error");
                await Task.Delay(1000, ct); 
            }
        }
    }

    private async Task HandleMessage(ConsumeResult<string, string> result, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var dapper = scope.ServiceProvider.GetRequiredService<DapperContext>();
        
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var payload = JsonSerializer.Deserialize<PaymentPayload>(result.Message.Value, options);
        
        if (payload == null)
        {
            LogRetrievedInvalidPayload(result.Message.Key);
            return;
        }
        
        await dapper.WithTransaction(async (conn, trans) =>
        {
            const string idempotencySql = """
                INSERT INTO bank_payments.processed_events (event_id)
                VALUES (@EventId) ON CONFLICT DO NOTHING
            """;
            
            var eventId = Guid.Parse(result.Message.Key);
            var affected = await conn.ExecuteAsync(new CommandDefinition(
                idempotencySql, 
                new { EventId = eventId }, 
                trans, 
                cancellationToken: ct));

            if (affected == 0)
            {
                LogDuplicateEvent(eventId);
                return; 
            }
            
            const string updateStatusSql = """
                UPDATE bank_payments.users 
                SET status = 'Completed',
                    amount = amount + @AmountCents
                WHERE id = @UserId
            """;

            var updateCount = await conn.ExecuteAsync(new CommandDefinition(
                updateStatusSql, 
                new { 
                    UserId = payload.UserId,
                    AmountCents = payload.Amount 
                }, 
                trans, 
                cancellationToken: ct));

            if (updateCount == 0)
            {
                throw new Exception($"User {payload.UserId} not found. Rolling back transaction.");
            }
            
            LogPaymentProcessed(payload.Amount, payload.UserId, eventId);
        });
    }
    
    [LoggerMessage(LogLevel.Error, "Received empty or invalid payload for message {Key}")]
    partial void LogRetrievedInvalidPayload(string key);
    
    [LoggerMessage(LogLevel.Warning, "Duplicate event detected. Skipping: {Id}")]
    partial void LogDuplicateEvent(Guid id);
    
    [LoggerMessage(LogLevel.Information, "Processed payment of {Amount} for User {UserId} (Event: {Id})")]
    partial void LogPaymentProcessed(decimal amount, Guid userId, Guid id);
}