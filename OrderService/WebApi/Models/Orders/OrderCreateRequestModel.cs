using System.Diagnostics.CodeAnalysis;

namespace WebApi.Models.Orders;

[ExcludeFromCodeCoverage]
public sealed class OrderCreateRequestModel
{
    public Guid CustomerId { get; set; }
    public string DeliveryAddress { get; set; }
    public ICollection<OrderItemCreateRequestModel> OrderItems { get; set; } = [];
}


[ExcludeFromCodeCoverage]
public sealed class OrderItemCreateRequestModel
{
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

