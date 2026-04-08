namespace Domain.Models;

public class OutboxMessage
{
    public required Guid Id { get; init; }
    public required string Type { get; init; }
    public required PaymentPayload Payload { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ProcessedAt { get; init; }
}