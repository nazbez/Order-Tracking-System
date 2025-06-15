using System.Diagnostics.CodeAnalysis;

namespace Application.Orders.Models;

[ExcludeFromCodeCoverage]
public sealed class OrderItemDto
{
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
