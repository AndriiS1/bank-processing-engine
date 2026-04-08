using System.Data;
using System.Text.Json;
using Dapper;
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
                const string updateSql = """
                                             UPDATE bank_payments.users
                                             SET amount = amount - @Amount
                                             WHERE id = @UserId AND amount >= @Amount
                                         """;

                var affectedRows = await connection.ExecuteAsync(new CommandDefinition(
                    updateSql, new { request.UserId, request.Amount }, transaction, cancellationToken: cancellationToken));

                if (affectedRows == 0) throw new Exception("Insufficient funds or user not found");
                
                var paymentEvent = new { request.UserId, request.Amount, OccurredAt = DateTime.UtcNow };
                
                const string outboxSql = """
                                             INSERT INTO bank_payments.outbox_messages (type, payload)
                                             VALUES (@Type, @Payload::jsonb)
                                         """;

                await connection.ExecuteAsync(new CommandDefinition(
                    outboxSql, 
                    new { 
                        Type = "PaymentCreated", 
                        Payload = JsonSerializer.Serialize(paymentEvent) 
                    }, 
                    transaction, 
                    cancellationToken: cancellationToken));
                
                await transaction.CommitAsync(cancellationToken);
                return true;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}