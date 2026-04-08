namespace Domain.Models;

public class User
{
    public required Guid Id { get; init; }
    public long Amount { get; init; }
    public string? Status { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
