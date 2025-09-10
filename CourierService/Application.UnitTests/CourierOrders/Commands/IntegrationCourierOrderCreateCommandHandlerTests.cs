using Application.Abstractions.Data;
using Application.CourierOrders.Commands;
using Domain.CourierOrders;
using Moq;

namespace Application.UnitTests.CourierOrders.Commands;

public sealed class IntegrationCourierOrderCreateCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> applicationDbContextMock;
    private readonly IntegrationCourierOrderCreateCommandHandler handler;
    
    public IntegrationCourierOrderCreateCommandHandlerTests()
    {
        applicationDbContextMock = new Mock<IApplicationDbContext>();
        handler = new IntegrationCourierOrderCreateCommandHandler(applicationDbContextMock.Object);
    }
    
    [Fact]
    public async Task HandleAsync_CreatesCourierOrderAndReturnsOrderIdWhenRequestIsValid()
    {
        // Arrange
        var courierOrderId = Guid.NewGuid();
        
        applicationDbContextMock.Setup(db => db.CourierOrders.AddAsync(It.IsAny<CourierOrder>(), It.IsAny<CancellationToken>()))
            .Callback<CourierOrder, CancellationToken>((courierOrder, _) => courierOrder.Id = courierOrderId);

        var command = new IntegrationCourierOrderCreateCommand(
            Guid.NewGuid(),
            "123 Main St",
            Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(courierOrderId, result.Value);
        applicationDbContextMock.Verify(db => db.CourierOrders.AddAsync(It.IsAny<CourierOrder>(), It.IsAny<CancellationToken>()), Times.Once);
        applicationDbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
