namespace Application.Core.IntegrationEvents;

public interface IIntegrationEventPublisher
{
    Task Publish<T>(T integrationEvent, CancellationToken cancellationToken) where T : IIntegrationEvent;
} 
