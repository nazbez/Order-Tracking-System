using Application.Abstractions.Data;
using OrderTrackingSystem.Core.IntegrationEvents;

namespace Infrastructure.Persistence.Outbox;

public sealed class OutboxIntegrationEventPublisher(IApplicationDbContext applicationDbContext)
    : IIntegrationEventPublisher
{
    public async Task Publish<T>(T integrationEvent, CancellationToken cancellationToken) where T : IIntegrationEvent
    {
        var outboxMessage = OutboxMessage.From(integrationEvent);
        
        await applicationDbContext.Set<OutboxMessage>().AddAsync(outboxMessage, cancellationToken);
    }
}
