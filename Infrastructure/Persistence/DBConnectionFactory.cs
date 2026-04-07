using Npgsql;
using System.Data;

namespace Infrastructure.Persistence;

public class DbConnectionFactory(string connectionString)
{
    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(connectionString);
    }
}
