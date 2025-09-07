using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WebApi.Models.Orders;

namespace WebApi.IntegrationTests.Orders;

[Collection("OrderTests")]
public class GetOrderByIdTests(TestWebApplicationFactory factory) : BaseTests(factory)
{
    private const string BaseUrl = "/api/v1/orders";

    [Fact]
    public async Task GetOrderById_ShouldReturnOk_WhenOrderExists()
    {
        // Arrange
        var client = Factory.CreateClient();
        var order = TestDataSeeder.Orders.First();

        // Act
        var response = await client.GetAsync($"{BaseUrl}/{order.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadFromJsonAsync<OrderResponseModel>(JsonSerializerOptions.Web);

        Assert.NotNull(responseContent);
        Assert.Equal(order.Id, responseContent.Id);
        Assert.Equal(order.CustomerId, responseContent.CustomerId);
        Assert.Equal(order.DeliveryAddress, responseContent.DeliveryAddress);
        Assert.Equal(order.Status.ToString(), responseContent.Status);
        Assert.Equal(order.CreatedAt.Date, responseContent.CreatedAt.Date);
    }
    
    [Fact]
    public async Task GetOrderById_ShouldReturnBadRequest_WhenOrderDoesNotExist()
    {
        // Arrange
        var client = Factory.CreateClient();
        var orderId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"{BaseUrl}/{orderId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
