using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Domain.Abstractions;

namespace Domain.CourierOrders;

[ExcludeFromCodeCoverage]
public sealed class CourierOrder : BaseEntity<Guid>
{
    public Guid OrderId { get; private set; }
    public string DeliveryAddress { get; private set; }
    public Guid CustomerId { get; private set; }
    
    [Description("Customer identifier. Will be replaced with a reference to the Courier entity in the future.")]
    public Guid? CourierId { get; private set; }
    public DateTimeOffset? DeliveredAt { get; private set; }
    
    private CourierOrder(Guid orderId, string deliveryAddress, Guid customerId)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        DeliveryAddress = deliveryAddress;
        CustomerId = customerId;
        CreatedAt = DateTimeOffset.Now;
        UpdatedAt = DateTimeOffset.Now;
    }

    public static CourierOrder Create(Guid orderId, string deliveryAddress, Guid customerId)
    {
        return new CourierOrder(orderId, deliveryAddress, customerId);
    }
}
