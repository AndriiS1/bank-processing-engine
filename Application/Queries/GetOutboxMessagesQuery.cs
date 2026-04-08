using Dapper;
using Domain.Models;
using Infrastructure.Persistence;
using MediatR;

namespace Application.Queries;

public record GetOutboxMessagesQuery: IRequest<IEnumerable<OutboxMessage>>;

public class GetPaymentMessagesQueryHandler(DapperContext dapperContext) : IRequestHandler<GetOutboxMessagesQuery, IEnumerable<OutboxMessage>>
{
    public async Task<IEnumerable<OutboxMessage>> Handle(GetOutboxMessagesQuery query, CancellationToken cancellationToken)
    {
        const string sql = """
                               SELECT
                                   id,
                                   type,
                                   payload,
                                   created_at,
                                   processed_at
                               FROM bank_payments.outbox_messages
                               ORDER BY created_at DESC
                           """;
        
        return await dapperContext.WithConnection(async connection =>
        {
            var results = await connection.QueryAsync<OutboxMessage>(new CommandDefinition(
                sql, 
                cancellationToken: cancellationToken));
                
            return results;
        });
    }
}