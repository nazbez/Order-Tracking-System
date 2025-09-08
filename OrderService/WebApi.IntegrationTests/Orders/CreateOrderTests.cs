using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WebApi.Models.Orders;

namespace WebApi.IntegrationTests.Orders;

[Collection("OrderTests")]
public class CreateOrderTests(TestWebApplicationFactory factory)
{
    private const string BaseUrl = "/api/v1/orders";
    
    [Fact]
    public async Task CreateOrder_ShouldReturnCreated_WhenDataIsValid()
    {
        // Arrange
        var client = factory.CreateClient();
        var order = new OrderCreateRequestModel
        {
            CustomerId = Guid.NewGuid(),
            DeliveryAddress = "223 Main St",
            OrderItems = new List<OrderItemCreateRequestModel>
            {
                new()
                {
                    ProductName = "Product 1",
                    Price = 10.0m,
                    Quantity = 2
                },
                new()
                {
                    ProductName = "Product 2",
                    Price = 20.0m,
                    Quantity = 1
                }
            }
        };

        // Act
        var response = await client.PostAsync(BaseUrl, JsonContent.Create(order, options: JsonSerializerOptions.Web));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenDeliveryAddressIsEmpty()
    {
        // Arrange
        var client = factory.CreateClient();
        var order = new OrderCreateRequestModel
        {
            CustomerId = Guid.NewGuid(),
            DeliveryAddress = string.Empty,
            OrderItems = new List<OrderItemCreateRequestModel>
            {
                new()
                {
                    ProductName = "Product 1",
                    Price = 10.0m,
                    Quantity = 2
                },
                new()
                {
                    ProductName = "Product 2",
                    Price = 20.0m,
                    Quantity = 1
                }
            }
        };

        // Act
        var response = await client.PostAsync(BaseUrl, JsonContent.Create(order, options: JsonSerializerOptions.Web));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadFromJsonAsync<JsonElement>(JsonSerializerOptions.Default);

        Assert.Equal("DeliveryAddress", responseContent.GetProperty("code").GetString());
    }
    
    [Fact]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenOrderItemNameIsEmpty()
    {
        // Arrange
        var client = factory.CreateClient();
        var order = new OrderCreateRequestModel
        {
            CustomerId = Guid.NewGuid(),
            DeliveryAddress = "223 Main St",
            OrderItems = new List<OrderItemCreateRequestModel>
            {
                new()
                {
                    ProductName = string.Empty,
                    Price = 10.0m,
                    Quantity = 2
                },
            }
        };

        // Act
        var response = await client.PostAsync(BaseUrl, JsonContent.Create(order, options: JsonSerializerOptions.Web));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadFromJsonAsync<JsonElement>(JsonSerializerOptions.Default);

        Assert.Equal("OrderItems[0].ProductName", responseContent.GetProperty("code").GetString());
    }
    
    [Fact]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenOrderItemProductQuantityIsLessOrEqualZero()
    {
        // Arrange
        var client = factory.CreateClient();
        var order = new OrderCreateRequestModel
        {
            CustomerId = Guid.NewGuid(),
            DeliveryAddress = "223 Main St",
            OrderItems = new List<OrderItemCreateRequestModel>
            {
                new()
                {
                    ProductName = "Product 1",
                    Price = 10.0m,
                    Quantity = 0
                },
            }
        };

        // Act
        var response = await client.PostAsync(BaseUrl, JsonContent.Create(order, options: JsonSerializerOptions.Web));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadFromJsonAsync<JsonElement>(JsonSerializerOptions.Default);

        Assert.Equal("OrderItems[0].Quantity", responseContent.GetProperty("code").GetString());
    }
    
    [Fact]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenOrderItemProductPriceIsLessOrEqualZero()
    {
        // Arrange
        var client = factory.CreateClient();
        var order = new OrderCreateRequestModel
        {
            CustomerId = Guid.NewGuid(),
            DeliveryAddress = "223 Main St",
            OrderItems = new List<OrderItemCreateRequestModel>
            {
                new()
                {
                    ProductName = "Product 1",
                    Price = 0,
                    Quantity = 2
                },
            }
        };

        // Act
        var response = await client.PostAsync(BaseUrl, JsonContent.Create(order, options: JsonSerializerOptions.Web));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadFromJsonAsync<JsonElement>(JsonSerializerOptions.Default);

        Assert.Equal("OrderItems[0].Price", responseContent.GetProperty("code").GetString());
    }
}
