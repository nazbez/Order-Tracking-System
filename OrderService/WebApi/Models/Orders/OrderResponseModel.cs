using System.Diagnostics.CodeAnalysis;

namespace WebApi.Models.Orders;

[ExcludeFromCodeCoverage]
public sealed class OrderResponseModel
{
    public Guid Id { get; set; }
    public string DeliveryAddress { get; set; }
    public string Status { get; set; }
    public Guid CustomerId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public ICollection<OrderItemResponseModel> OrderItems { get; set; } = [];
}

[ExcludeFromCodeCoverage]
public sealed class OrderItemResponseModel
{
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
