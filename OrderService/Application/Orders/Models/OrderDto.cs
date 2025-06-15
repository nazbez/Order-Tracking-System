using System.Diagnostics.CodeAnalysis;

namespace Application.Orders.Models;

[ExcludeFromCodeCoverage]
public sealed class OrderDto
{
    public Guid Id { get; set; }
    public string DeliveryAddress { get; private set; }
    public string Status { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateTimeOffset CreatedAt { get; set; }
    public ICollection<OrderItemDto> OrderItems { get; set; } = [];
}
