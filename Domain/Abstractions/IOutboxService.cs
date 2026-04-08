namespace Domain.Abstractions;

public interface IOutboxService
{
    Task<int> ProcessMessagesAsync(CancellationToken ct);
}
