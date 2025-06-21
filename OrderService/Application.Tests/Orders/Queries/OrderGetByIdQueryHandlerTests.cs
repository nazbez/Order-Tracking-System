using Application.Abstractions.Data;
using Application.Orders.Queries;
using Domain.Orders;
using ErrorOr;
using Moq;
using Moq.EntityFrameworkCore;

namespace Application.Tests.Orders.Queries;

public sealed class OrderGetByIdQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> applicationDbContextMock;
    private readonly OrderGetByIdQueryHandler handler;
    
    public OrderGetByIdQueryHandlerTests()
    {
        applicationDbContextMock = new Mock<IApplicationDbContext>();
        handler = new OrderGetByIdQueryHandler(applicationDbContextMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ReturnsOrderDtoWhenOrderExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create(string.Empty, Guid.NewGuid());
        order.Id = orderId;
        applicationDbContextMock.Setup(db => db.Orders).ReturnsDbSet(new List<Order> { order });
        
        // Act
        var result = await handler.HandleAsync(new OrderGetByIdQuery(orderId), CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(orderId, result.Value.Id);
    }

    [Fact]
    public async Task HandleAsync_ReturnsNotFoundErrorWhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        applicationDbContextMock.Setup(db => db.Orders).ReturnsDbSet(Array.Empty<Order>());

        // Act
        var result = await handler.HandleAsync(new OrderGetByIdQuery(orderId), CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e == Error.NotFound());
    }
}
