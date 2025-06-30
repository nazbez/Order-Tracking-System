using System.Diagnostics.CodeAnalysis;

namespace OutboxWorker.Models.Events;

[ExcludeFromCodeCoverage]
public class OrderCreatedEvent : IEvent<Guid>
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Status Status { get; set; }
}

public enum Status : byte
{
    Placed,
    Assigned,
    Preparing,
    OutForDelivery,
    Delivered,
    Cancelled
}
