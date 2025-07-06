using System.Diagnostics.CodeAnalysis;

namespace KafkaConsumer.Models.Events;

[ExcludeFromCodeCoverage]
public sealed class OrderCreatedEvent
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string DeliveryAddress { get; set; }
}

