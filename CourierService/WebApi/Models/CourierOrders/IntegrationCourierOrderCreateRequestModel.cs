namespace WebApi.Models.CourierOrders;

public sealed class IntegrationCourierOrderCreateRequestModel
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string DeliveryAddress { get; set; }
}
