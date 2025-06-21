using System.Diagnostics.CodeAnalysis;
using Domain.Abstractions;
using Domain.Orders;

namespace Domain.OrderItems;

[ExcludeFromCodeCoverage]
public sealed class OrderItem : BaseEntity<Guid>
{
    public Guid OrderId { get; private set; }
    public string ProductName { get; private set; }
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }
    
    public Order? Order { get; private set; }

    private OrderItem(string productName, decimal price, int quantity)
    {
        Id = Guid.NewGuid();
        ProductName = productName;
        Price = price;
        Quantity = quantity;
        CreatedAt = DateTimeOffset.Now;
        UpdatedAt = DateTimeOffset.Now;
    }

    public static OrderItem Create(string productName, decimal price, int quantity)
    {
        return new OrderItem(productName, price, quantity);
    }
}
