namespace Domain.Models;

public record PaymentPayload(
    Guid TransactionId,
    Guid UserId,
    decimal Amount,
    DateTimeOffset Timestamp
);