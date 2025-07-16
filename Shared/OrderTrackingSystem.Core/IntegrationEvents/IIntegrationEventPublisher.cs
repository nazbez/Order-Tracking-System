namespace OrderTrackingSystem.Core.IntegrationEvents;

/// <summary>
/// Defines a publisher for integration events, enabling communication between different parts of the system.
/// </summary>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// Publishes an integration event asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the integration event to be published.</typeparam>
    /// <param name="integrationEvent">The integration event instance to be published.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Publish<T>(T integrationEvent, CancellationToken cancellationToken) where T : IIntegrationEvent;
}