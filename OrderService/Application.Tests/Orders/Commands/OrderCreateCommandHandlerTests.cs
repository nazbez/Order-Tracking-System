using Application.Abstractions.Data;
using Application.Orders.Commands;
using Application.Orders.Models;
using Domain.Entities;
using Moq;

namespace Application.Tests.Orders.Commands;

public sealed class OrderCreateCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> applicationDbContextMock;
    private readonly OrderCreateCommandHandler handler;
    
    public OrderCreateCommandHandlerTests()
    {
        applicationDbContextMock = new Mock<IApplicationDbContext>();
        handler = new OrderCreateCommandHandler(applicationDbContextMock.Object);
    }
    
    [Fact]
    public async Task HandleAsync_CreatesOrderAndReturnsOrderIdWhenRequestIsValid()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderEntity = Order.Create(string.Empty, Guid.NewGuid());
        orderEntity.Id = orderId;
        
        applicationDbContextMock.Setup(db => db.Orders.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => order.Id = orderId);

        var command = new OrderCreateCommand(
            Guid.NewGuid(),
            "123 Main St",
            new List<OrderItemDto>
            {
                new() { Price = 10, Quantity = 1, ProductName = "Product A" }
            });

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(orderId, result.Value);
        applicationDbContextMock.Verify(db => db.Orders.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        applicationDbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
