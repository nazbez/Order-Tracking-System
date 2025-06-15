using System.Diagnostics.CodeAnalysis;

namespace Application.Orders.Commands.Models;

[ExcludeFromCodeCoverage]
public sealed class OrderItemCreateDto
{
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
