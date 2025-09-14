using Grpc.Core;
using OrderTrackingSystem.Grpc;

namespace WebApi.IntegrationTests.Orders.Grpc;

[Collection("OrderTests")]
public class GetOrderStatusTests(TestWebApplicationFactory factory)
{
    [Fact]
    public async Task GetOrderStatus_ShouldReturnOrderStatus_WhenOrderExists()
    {
        // Arrange
        var client = factory.CreateGrpcClient();
        var existingOrder = TestDataSeeder.Orders.First();
        var request = new OrderStatusRequest { OrderId = existingOrder.Id.ToString() };

        // Act
        var response = await client.GetOrderStatusAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(existingOrder.Id.ToString(), response.OrderId);
        Assert.Equal(existingOrder.Status.ToString(), response.Status);
    }
    
    [Fact]
    public async Task GetOrderStatus_ShouldThrowNotFoundException_WhenOrderDoesNotExist()
    {
        // Arrange
        var client = factory.CreateGrpcClient();
        var request = new OrderStatusRequest { OrderId = Guid.NewGuid().ToString() };

        // Act
        // Assert
        var exception = await Assert.ThrowsAsync<RpcException>(async () => await client.GetOrderStatusAsync(request));
        Assert.NotNull(exception);
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }
}
