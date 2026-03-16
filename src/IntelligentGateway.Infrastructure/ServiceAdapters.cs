using System.Net;
using System.Text.Json;
using IntelligentGateway.Application;
using Microsoft.Extensions.Options;

namespace IntelligentGateway.Infrastructure;

public sealed class AdapterOptions
{
    public string OrderServiceBaseUrl { get; set; } = "http://localhost:5101";
    public string TrackingServiceBaseUrl { get; set; } = "http://localhost:5102";
    public string CourierServiceBaseUrl { get; set; } = "http://localhost:5103";
}

public sealed class OrderServiceAdapter(HttpClient httpClient, IOptions<AdapterOptions> options) : IServiceAdapter
{
    public string Key => "order-service";

    public async Task<JsonDocument> ExecuteAsync(string toolId, string argumentsJson, CancellationToken cancellationToken)
    {
        using var args = JsonDocument.Parse(argumentsJson);
        var baseUrl = options.Value.OrderServiceBaseUrl.TrimEnd('/');

        if (toolId == "get_order_by_id")
        {
            var orderId = args.RootElement.GetProperty("orderId").GetString();
            var response = await httpClient.GetAsync($"{baseUrl}/api/v1/orders/{orderId}", cancellationToken);
            return await ToJson(response, cancellationToken);
        }

        if (toolId == "get_latest_user_order")
        {
            // TODO: replace placeholder when OrderService exposes list/filter endpoint.
            var userId = args.RootElement.GetProperty("userId").GetString();
            var payload = JsonSerializer.SerializeToDocument(new
            {
                id = $"latest-for-{userId}",
                customerId = userId,
                status = "Processing",
                createdAt = DateTimeOffset.UtcNow.AddMinutes(-25)
            });

            return payload;
        }

        return JsonDocument.Parse("{" + "\"error\":\"unsupported tool for adapter\"}");
    }

    private static async Task<JsonDocument> ToJson(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return JsonSerializer.SerializeToDocument(new { error = "Not found", status = "not_found" });
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return JsonSerializer.SerializeToDocument(new { error = body, status = "failed" });
        }

        return JsonDocument.Parse(body);
    }
}

public sealed class TrackingServiceAdapter(HttpClient httpClient, IOptions<AdapterOptions> options) : IServiceAdapter
{
    public string Key => "tracking-service";

    public async Task<JsonDocument> ExecuteAsync(string toolId, string argumentsJson, CancellationToken cancellationToken)
    {
        if (toolId != "get_tracking_status")
        {
            return JsonSerializer.SerializeToDocument(new { error = "unsupported tool" });
        }

        using var args = JsonDocument.Parse(argumentsJson);
        var orderId = args.RootElement.GetProperty("orderId").GetString();
        var baseUrl = options.Value.TrackingServiceBaseUrl.TrimEnd('/');

        var response = await httpClient.GetAsync($"{baseUrl}/api/v1/orders/{orderId}/status", cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return JsonSerializer.SerializeToDocument(new { error = body, status = "unavailable" });
        }

        return JsonSerializer.SerializeToDocument(new { orderId, status = body.Trim('"') });
    }
}

public sealed class CourierServiceAdapter : IServiceAdapter
{
    public string Key => "courier-service";

    public Task<JsonDocument> ExecuteAsync(string toolId, string argumentsJson, CancellationToken cancellationToken)
    {
        using var args = JsonDocument.Parse(argumentsJson);
        var orderId = args.RootElement.GetProperty("orderId").GetString();

        // TODO: replace mock output when courier ETA endpoint exists.
        var doc = JsonSerializer.SerializeToDocument(new
        {
            orderId,
            eta = DateTimeOffset.UtcNow.AddHours(2),
            source = "mock-placeholder"
        });

        return Task.FromResult(doc);
    }
}
