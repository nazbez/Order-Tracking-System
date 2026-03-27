using System.Text.Json;
using IntelligentGateway.Application;
using IntelligentGateway.Domain;

namespace IntelligentGateway.Infrastructure;

public sealed class InMemoryToolRegistry : IToolRegistry
{
    private readonly IReadOnlyCollection<ToolDefinition> _tools;

    public InMemoryToolRegistry()
    {
        _tools =
        [
            new ToolDefinition(
                "get_order_by_id",
                "Get order by id",
                "Retrieves order details by orderId.",
                ["orders.read"],
                JsonDocument.Parse("""
                    {
                      "type":"object",
                      "required":["orderId"],
                      "properties":{
                        "orderId": { "type":"string", "minLength":1 }
                      }
                    }
                    """),
                "Order summary containing id, customerId, status and createdAt.",
                "order-service",
                TimeSpan.FromSeconds(5),
                1),
            new ToolDefinition(
                "get_latest_user_order",
                "Get latest user order",
                "Finds latest order for a given userId.",
                ["orders.read"],
                JsonDocument.Parse("""
                    {
                      "type":"object",
                      "required":["userId"],
                      "properties":{
                        "userId": { "type":"string", "minLength":1 }
                      }
                    }
                    """),
                "Order summary for most recent order.",
                "order-service",
                TimeSpan.FromSeconds(5),
                1),
            new ToolDefinition(
                "get_tracking_status",
                "Get tracking status",
                "Gets tracking status for order by orderId.",
                ["tracking.read"],
                JsonDocument.Parse("""
                    {
                      "type":"object",
                      "required":["orderId"],
                      "properties":{
                        "orderId": { "type":"string", "minLength":1 }
                      }
                    }
                    """),
                "Current tracking status string.",
                "tracking-service",
                TimeSpan.FromSeconds(5),
                1),
            new ToolDefinition(
                "get_delivery_eta",
                "Get delivery ETA",
                "Gets rough ETA by orderId. Placeholder adapter in MVP.",
                ["tracking.read", "courier.read"],
                JsonDocument.Parse("""
                    {
                      "type":"object",
                      "required":["orderId"],
                      "properties":{
                        "orderId": { "type":"string", "minLength":1 }
                      }
                    }
                    """),
                "Estimated delivery timestamp if known.",
                "courier-service",
                TimeSpan.FromSeconds(5),
                0)
        ];
    }

    public IReadOnlyCollection<ToolDefinition> GetAll() => _tools;

    public ToolDefinition? GetById(string toolId)
        => _tools.FirstOrDefault(t => string.Equals(t.Id, toolId, StringComparison.OrdinalIgnoreCase));
}
