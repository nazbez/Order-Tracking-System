using System.Text.Json;
using IntelligentGateway.Application;
using IntelligentGateway.Domain;
using Microsoft.Extensions.AI;

namespace IntelligentGateway.Infrastructure;

public sealed class RuleBasedChatOrchestrator(IChatClient chatClient) : IChatOrchestrator
{
    public Task<ToolCallPlan> PlanAsync(
        UserQuery query,
        SessionContext session,
        IReadOnlyCollection<ToolDefinition> allowedTools,
        IReadOnlyCollection<ToolCallResult> priorResults,
        CancellationToken cancellationToken)
    {
        var message = Normalize(query.Message);
        var requested = ParseIntent(message);
        var sessionOrderId = session.ResolvedOrderId ?? session.Entities.GetValueOrDefault("orderId");

        if (priorResults.Count == 0)
        {
            if (requested.AsksLatest)
            {
                return PlanTool("get_latest_user_order", new { userId = query.UserId }, scenario: requested.AsksEta ? "latest_then_eta" : "latest_then_status");
            }

            if (requested.AsksEta)
            {
                if (requested.OrderId is not null)
                {
                    return PlanTool("get_delivery_eta", new { orderId = requested.OrderId }, scenario: "eta_by_order_id");
                }

                if (!string.IsNullOrWhiteSpace(sessionOrderId))
                {
                    return PlanTool("get_delivery_eta", new { orderId = sessionOrderId }, usedSessionMemory: true, scenario: "eta_follow_up_from_session");
                }

                return Clarify("Please provide your order ID so I can check delivery ETA.", "clarify_missing_order_for_eta");
            }

            if (requested.AsksStatus)
            {
                if (requested.OrderId is not null)
                {
                    return PlanTool("get_tracking_status", new { orderId = requested.OrderId }, scenario: "status_by_order_id");
                }

                if (!string.IsNullOrWhiteSpace(sessionOrderId))
                {
                    return PlanTool("get_tracking_status", new { orderId = sessionOrderId }, usedSessionMemory: true, scenario: "status_follow_up_from_session");
                }

                return Clarify("Please provide your order ID, or ask for your latest order status.", "clarify_missing_order_for_status");
            }

            return Unsupported();
        }

        if (priorResults.Count == 1 && priorResults[0].ToolId == "get_latest_user_order")
        {
            var latestOrderId = ExtractOrderId(priorResults[0].Data) ?? sessionOrderId;
            if (string.IsNullOrWhiteSpace(latestOrderId))
            {
                return Clarify("I could not determine your latest order ID. Please provide it explicitly.", "clarify_latest_resolution_failed");
            }

            if (requested.AsksEta)
            {
                return PlanTool("get_delivery_eta", new { orderId = latestOrderId }, scenario: "latest_resolved_then_eta");
            }

            return PlanTool("get_tracking_status", new { orderId = latestOrderId }, scenario: "latest_resolved_then_status");
        }

        return Done();
    }

    public Task<string> ComposeFinalAnswerAsync(
        UserQuery query,
        SessionContext session,
        IReadOnlyCollection<ToolCallResult> results,
        CancellationToken cancellationToken)
    {
        if (results.Count == 0)
        {
            return Task.FromResult("I can help with order status or delivery ETA requests, but I could not map this request to a supported scenario.");
        }

        var eta = results.LastOrDefault(r => r.ToolId == "get_delivery_eta" && r.Success);
        if (eta?.Data is not null)
        {
            var orderId = eta.Data.RootElement.GetProperty("orderId").GetString() ?? session.ResolvedOrderId ?? "N/A";
            var etaValue = eta.Data.RootElement.TryGetProperty("eta", out var etaProperty)
                ? etaProperty.GetString()
                : "unknown";
            return Task.FromResult($"Estimated delivery time for order {orderId} is {etaValue}.");
        }

        var tracking = results.LastOrDefault(r => r.ToolId == "get_tracking_status" && r.Success);
        if (tracking?.Data is not null)
        {
            var status = tracking.Data.RootElement.GetProperty("status").GetString() ?? "Unknown";
            var orderId = tracking.Data.RootElement.GetProperty("orderId").GetString() ?? session.ResolvedOrderId ?? "N/A";
            return Task.FromResult($"Order {orderId} is currently '{status}'.");
        }

        var latest = results.LastOrDefault(r => r.ToolId == "get_latest_user_order" && r.Success);
        if (latest?.Data is not null)
        {
            var id = latest.Data.RootElement.GetProperty("id").GetString() ?? "N/A";
            var status = latest.Data.RootElement.GetProperty("status").GetString() ?? "Unknown";
            return Task.FromResult($"Your latest order is {id} with status '{status}'.");
        }

        var last = results.Last();
        return Task.FromResult(last.Success
            ? "Request completed."
            : $"Service is currently unavailable: {last.Error}");
    }

    private static ToolCallPlan PlanTool(string toolId, object args, bool usedSessionMemory = false, string scenario = "tool_call")
    {
        var argsDoc = JsonSerializer.SerializeToDocument(args);
        return new ToolCallPlan(false, null, null, new ToolCallRequest(toolId, argsDoc.RootElement.Clone()), usedSessionMemory, scenario);
    }

    private static ToolCallPlan Clarify(string question, string scenario)
        => new(true, question, null, null, false, scenario);

    private static ToolCallPlan Done()
        => new(false, null, "Done.", null, false, "done");

    private static ToolCallPlan Unsupported()
        => new(false, null,
            "Unsupported request. I can help with: order status by order ID, latest order status, and delivery ETA.",
            null,
            false,
            "unsupported");

    private static ParsedIntent ParseIntent(string message)
    {
        var asksLatest = ContainsAny(message, "latest", "most recent", "recent");
        var asksEta = ContainsAny(message, "eta", "arrive", "arrival", "delivery time");
        var asksStatus = ContainsAny(message, "status", "where is", "track", "tracking") || (!asksEta && ContainsAny(message, "order"));

        var orderId = ExtractOrderId(message);
        return new ParsedIntent(asksStatus, asksLatest, asksEta, orderId);
    }

    private static bool ContainsAny(string value, params string[] tokens)
        => tokens.Any(value.Contains);

    private static string Normalize(string message)
        => message.Trim().ToLowerInvariant();

    private static string? ExtractOrderId(string text)
    {
        var tokens = text.Split([' ', '?', '.', ',', ':', ';'], StringSplitOptions.RemoveEmptyEntries);
        return tokens.FirstOrDefault(t => t.All(char.IsLetterOrDigit) && t.Length >= 3 && t.Any(char.IsDigit));
    }

    private static string? ExtractOrderId(JsonDocument? data)
    {
        if (data is null)
        {
            return null;
        }

        if (data.RootElement.TryGetProperty("id", out var idValue))
        {
            return idValue.ToString();
        }

        if (data.RootElement.TryGetProperty("orderId", out var orderIdValue))
        {
            return orderIdValue.ToString();
        }

        return null;
    }

    private sealed record ParsedIntent(bool AsksStatus, bool AsksLatest, bool AsksEta, string? OrderId);
}

public sealed class NoOpChatClient : IChatClient
{
    public object? GetService(Type serviceType, object? serviceKey = null)
        => null;

    public Task<ChatResponse> GetResponseAsync(IList<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        => Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "ok")));

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IList<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        => AsyncEnumerable.Empty<ChatResponseUpdate>();

    public void Dispose()
    {
    }
}
