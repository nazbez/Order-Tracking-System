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
        var message = query.Message.ToLowerInvariant();

        if ((message.Contains("where is my order") || message.Contains("where is order")) && !ContainsOrderId(message) && !session.Entities.ContainsKey("orderId"))
        {
            return Task.FromResult(new ToolCallPlan(true, "Please provide your order ID.", null, null));
        }

        if (priorResults.Count == 0)
        {
            if (message.Contains("latest") && message.Contains("status"))
            {
                var argsDoc = JsonSerializer.SerializeToDocument(new { userId = query.UserId });
                return Task.FromResult(new ToolCallPlan(false, null, null, new ToolCallRequest("get_latest_user_order", argsDoc.RootElement.Clone())));
            }

            if (ContainsOrderId(message, out var orderId))
            {
                var argsDoc = JsonSerializer.SerializeToDocument(new { orderId });
                return Task.FromResult(new ToolCallPlan(false, null, null, new ToolCallRequest("get_tracking_status", argsDoc.RootElement.Clone())));
            }
        }

        if (priorResults.Count == 1 && priorResults[0].ToolId == "get_latest_user_order")
        {
            var result = priorResults[0].Data?.RootElement;
            if (result.HasValue && result.Value.TryGetProperty("id", out var id))
            {
                var argsDoc = JsonSerializer.SerializeToDocument(new { orderId = id.ToString() });
                return Task.FromResult(new ToolCallPlan(false, null, null, new ToolCallRequest("get_tracking_status", argsDoc.RootElement.Clone())));
            }
        }

        return Task.FromResult(new ToolCallPlan(false, null, "Done.", null));
    }

    public async Task<string> ComposeFinalAnswerAsync(
        UserQuery query,
        SessionContext session,
        IReadOnlyCollection<ToolCallResult> results,
        CancellationToken cancellationToken)
    {
        if (results.Count == 0)
        {
            return "I could not execute any tool for this query.";
        }

        var tracking = results.LastOrDefault(r => r.ToolId == "get_tracking_status" && r.Success);
        if (tracking?.Data is not null)
        {
            var status = tracking.Data.RootElement.GetProperty("status").GetString() ?? "Unknown";
            var orderId = tracking.Data.RootElement.GetProperty("orderId").GetString() ?? session.Entities.GetValueOrDefault("orderId", "N/A");
            return $"Order {orderId} is currently '{status}'.";
        }

        var latest = results.LastOrDefault(r => r.ToolId == "get_latest_user_order" && r.Success);
        if (latest?.Data is not null)
        {
            var id = latest.Data.RootElement.GetProperty("id").GetString() ?? "N/A";
            var status = latest.Data.RootElement.GetProperty("status").GetString() ?? "Unknown";
            return $"Your latest order is {id} with status '{status}'.";
        }

        var last = results.Last();
        return last.Success ? "Request completed." : $"Service is currently unavailable: {last.Error}";
    }

    private static bool ContainsOrderId(string message) => ContainsOrderId(message, out _);

    private static bool ContainsOrderId(string message, out string orderId)
    {
        orderId = string.Empty;
        var tokens = message.Split([' ', '?', '.', ',', ':', ';'], StringSplitOptions.RemoveEmptyEntries);
        var token = tokens.FirstOrDefault(t => t.All(char.IsLetterOrDigit) && t.Length >= 3 && t.Any(char.IsDigit));
        if (token is null)
        {
            return false;
        }

        orderId = token;
        return true;
    }
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
