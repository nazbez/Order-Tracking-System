using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Domain.Abstractions;
using Domain.OrderItems;
using Domain.Orders.Enums;

namespace Domain.Orders;

[ExcludeFromCodeCoverage]
public sealed class Order : BaseEntity<Guid>
{
    public string DeliveryAddress { get; private set; }
    public Status Status { get; private set; }
    
    [Description("Customer identifier. Will be replaced with a reference to the Customer entity in the future.")]
    public Guid CustomerId { get; private set; }

    private readonly List<OrderItem> orderItems = [];
    public IReadOnlyCollection<OrderItem> OrderItems => orderItems;

    private Order(string deliveryAddress, Guid customerId)
    {
        Id = Guid.NewGuid();
        DeliveryAddress = deliveryAddress;
        Status = Status.Placed;
        CustomerId = customerId;
        CreatedAt = DateTimeOffset.Now;
        UpdatedAt = DateTimeOffset.Now;
    }
    
    public void AddOrderItem(string productName, decimal price, int quantity)
    {
        var orderItem = OrderItem.Create(productName, price, quantity);
        orderItems.Add(orderItem);
        UpdatedAt = DateTimeOffset.Now;
    }
    
    public static Order Create(string deliveryAddress, Guid customerId)
    {
        var order = new Order(deliveryAddress, customerId);
        
        return order;
    }
}
