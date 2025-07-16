using System.Diagnostics.CodeAnalysis;

namespace Application.Orders.IntegrationEvents;

[ExcludeFromCodeCoverage]
public sealed class OrderCreatedIntegrationEvent(Guid id, Guid customerId, DateTimeOffset createdAt, string deliveryAddress)
    : IIntegrationEvent
{
    public Guid Id { get; private set; } = id;
    public Guid CustomerId { get; private set; } = customerId;
    public DateTimeOffset CreatedAt { get; private set; } = createdAt;
    public string DeliveryAddress { get; set; } = deliveryAddress;
}
