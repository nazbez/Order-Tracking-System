using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WebApi.Models.CourierOrders;

namespace WebApi.IntegrationTests.IntegrationCourierOrders;

[Collection("CourierTests")]
public class CreateCourierOrderTests(TestWebApplicationFactory factory)
{
    private const string BaseUrl = "/api/v1/integration-courier-orders";
    
    [Fact]
    public async Task CreateCourierOrder_ReturnsCreated_WhenDataIsValid()
    {
        // Arrange
        var client = factory.CreateClient();
        var request = new IntegrationCourierOrderCreateRequestModel
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            DeliveryAddress = "123 Main St",
        };
        
        // Act
        var response = await client.PostAsync(BaseUrl, JsonContent.Create(request, options: JsonSerializerOptions.Web));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenDeliveryAddressIsEmpty()
    {
        // Arrange
        var client = factory.CreateClient();
        var request = new IntegrationCourierOrderCreateRequestModel
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            DeliveryAddress = string.Empty,
        };

        // Act
        var response = await client.PostAsync(BaseUrl, JsonContent.Create(request, options: JsonSerializerOptions.Web));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadFromJsonAsync<JsonElement>(JsonSerializerOptions.Default);

        Assert.Equal("DeliveryAddress", responseContent.GetProperty("code").GetString());
    }
}
