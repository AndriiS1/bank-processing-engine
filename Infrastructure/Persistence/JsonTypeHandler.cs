using System.Data;
using System.Text.Json;
using Dapper;

namespace Infrastructure.Persistence;

public class JsonTypeHandler : SqlMapper.ITypeHandler
{
    public void SetValue(IDbDataParameter parameter, object? value)
    {
        parameter.Value = value is null ? DBNull.Value : JsonSerializer.Serialize(value);
    }

    public object? Parse(Type destinationType, object value)
    {
        return JsonSerializer.Deserialize((string)value, destinationType, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}