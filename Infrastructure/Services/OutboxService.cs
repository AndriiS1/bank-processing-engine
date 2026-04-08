using System.Text.Json;
using Dapper;
using Domain;
using Domain.Abstractions;
using Domain.Models;
using Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public partial class OutboxService(DapperContext dapperContext, IKafkaProducer producer, ILogger<OutboxService> logger) : IOutboxService
{
    public async Task<int> ProcessMessagesAsync(CancellationToken ct)
    {
        return await dapperContext.WithConnection(async conn =>
        {
            const string selectSql = """
                                         SELECT id, type, payload 
                                         FROM bank_payments.outbox_messages 
                                         WHERE processed_at IS NULL 
                                         ORDER BY created_at 
                                         FOR UPDATE SKIP LOCKED 
                                         LIMIT 100
                                     """;

            var messages = (await conn.QueryAsync<OutboxMessage>(selectSql)).ToList();
            
            if (messages.Count == 0) return 0;

            var processedCount = 0;
            foreach (var msg in messages)
            {
                try
                {
                    await producer.ProduceAsync(Constants.PaymentEventsTopic, msg.Id.ToString(), JsonSerializer.Serialize(msg.Payload), ct);
                    
                    const string updateSql = "UPDATE bank_payments.outbox_messages SET processed_at = NOW() WHERE id = @Id";
                    await conn.ExecuteAsync(updateSql, new { msg.Id });
                    
                    processedCount++;
                }
                catch (Exception ex)
                {
                    LogTriggerFailed(msg.Id, ex);
                }
            }
            return processedCount;
        });
    }
    
    [LoggerMessage(LogLevel.Error, "Manual trigger failed for message {Id}")]
    partial void LogTriggerFailed(Guid id, Exception exception);
}
