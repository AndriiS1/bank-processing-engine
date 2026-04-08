using System.Data;
using System.Text.Json;
using Dapper;
using Domain.Models;
using Infrastructure.Persistence;
using MediatR;
namespace Application.Requests;

public record CreatePaymentCommand(Guid UserId, long Amount) : IRequest<bool>;

public class CreatePaymentCommandHandler(DapperContext dapperContext) : IRequestHandler<CreatePaymentCommand, bool>
{
public async Task<bool> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        return await dapperContext.WithConnection(async connection =>
        {
            await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            
            try
            {
                const string validateSql = """
                    SELECT amount 
                    FROM bank_payments.users 
                    WHERE id = @UserId 
                    FOR SHARE;
                """;

                var currentBalance = await connection.QueryFirstOrDefaultAsync<long?>(
                    new CommandDefinition(validateSql, new { request.UserId }, transaction, cancellationToken: cancellationToken));

                if (currentBalance == null) 
                    throw new Exception("User not found");

                if (currentBalance < request.Amount) 
                    throw new Exception("Insufficient funds");
                
                var payload = new PaymentPayload(
                    TransactionId: Guid.NewGuid(),
                    UserId: request.UserId,
                    Amount: request.Amount,
                    Timestamp: DateTimeOffset.UtcNow
                );
                
                const string outboxSql = """
                    INSERT INTO bank_payments.outbox_messages (id, type, payload)
                    VALUES (@Id, @Type, @Payload::jsonb)
                """;

                await connection.ExecuteAsync(new CommandDefinition(
                    outboxSql, 
                    new { 
                        Id = Guid.NewGuid(),
                        Type = "PaymentCreated", 
                        Payload = JsonSerializer.Serialize(payload) 
                    }, 
                    transaction, 
                    cancellationToken: cancellationToken));
                
                await transaction.CommitAsync(cancellationToken);
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}