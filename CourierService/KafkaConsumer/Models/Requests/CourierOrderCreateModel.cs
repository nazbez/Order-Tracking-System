using System.Diagnostics.CodeAnalysis;

namespace KafkaConsumer.Models.Requests;

[ExcludeFromCodeCoverage]
public sealed class CourierOrderCreateModel(Guid orderId, Guid customerId, string deliveryAddress)
{
    public Guid OrderId { get; private set; } = orderId;
    public Guid CustomerId { get; private set; } = customerId;
    public string DeliveryAddress { get; private set; } = deliveryAddress;
}
