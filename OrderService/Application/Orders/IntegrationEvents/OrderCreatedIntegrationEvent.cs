using System.Diagnostics.CodeAnalysis;
using Application.Core.IntegrationEvents;
using Domain.Orders.Enums;

namespace Application.Orders.IntegrationEvents;

[ExcludeFromCodeCoverage]
public sealed class OrderCreatedIntegrationEvent(Guid id, Guid customerId, DateTimeOffset createdAt, Status status) : IIntegrationEvent
{
    public Guid Id { get; private set; } = id;
    public Guid CustomerId { get; private set; } = customerId;
    public DateTimeOffset CreatedAt { get; set; } = createdAt;
    public Status Status { get; private set; } = status;
}
