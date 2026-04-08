using Domain.Abstractions;
using MediatR;
namespace Application.Requests;

public record PublishPaymentsCommand : IRequest;

public class PublishPaymentsCommandHandler(IOutboxService outboxService) : IRequestHandler<PublishPaymentsCommand>
{
    public async Task Handle(PublishPaymentsCommand request, CancellationToken cancellationToken)
    {
        await outboxService.ProcessMessagesAsync(CancellationToken.None);
    }
}
