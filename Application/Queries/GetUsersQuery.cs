using Dapper;
using Domain.Models;
using Infrastructure.Persistence;
using MediatR;

namespace Application.Queries;

public record GetUsersQuery: IRequest<IEnumerable<User>>;

public class GetUsersQueryHandler(DapperContext dapperContext) : IRequestHandler<GetUsersQuery, IEnumerable<User>>
{
    public async Task<IEnumerable<User>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        const string sql = """
                               SELECT 
                                   id,
                                   amount,
                                   status, 
                                   created_at
                               FROM bank_payments.users
                           """;
        
        return await dapperContext.WithConnection(async connection =>
        {
            var results = await connection.QueryAsync<User>(new CommandDefinition(
                sql, 
                cancellationToken: cancellationToken));
                
            return results;
        });
    }
}