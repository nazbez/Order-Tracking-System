using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using IntelligentGateway.Domain;

namespace IntelligentGateway.Application;

public sealed class UserQueryHandler(
    ISessionStore sessionStore,
    IToolRegistry toolRegistry,
    IToolPolicyService toolPolicyService,
    IChatOrchestrator chatOrchestrator,
    IToolArgumentValidator validator,
    IToolExecutor toolExecutor,
    ILogger<UserQueryHandler> logger) : IUserQueryHandler
{
    private const int MaxToolIterations = 3;

    public async Task<FinalAnswer> HandleAsync(UserQuery query, string? role, string correlationId, CancellationToken cancellationToken)
    {
        var started = Stopwatch.StartNew();
        var trace = new ExecutionTrace();
        var toolCallSummaries = new List<ToolCallSummary>();

        var session = await sessionStore.GetAsync(query.SessionId, cancellationToken)
            ?? new SessionContext { SessionId = query.SessionId, UserId = query.UserId };

        session.RecentUserMessages.Add(query.Message);
        var allowed = toolPolicyService.FilterAllowedTools(query.UserId, role, toolRegistry.GetAll());

        trace.Steps.Add(MakeStep("policy", "Filtered allowed tools", new Dictionary<string, object?>
        {
            ["allowedToolCount"] = allowed.Count,
            ["correlationId"] = correlationId
        }));

        var executed = new List<ToolCallResult>();

        for (var i = 0; i < MaxToolIterations; i++)
        {
            var plan = await chatOrchestrator.PlanAsync(query, session, allowed, executed, cancellationToken);

            if (plan.RequiresClarification)
            {
                trace.ClarificationCount++;
                trace.Steps.Add(MakeStep("clarification", "Clarification requested", new Dictionary<string, object?>
                {
                    ["question"] = plan.ClarificationQuestion
                }));

                await sessionStore.UpsertAsync(session, cancellationToken);

                trace.DurationMs = started.ElapsedMilliseconds;
                return new FinalAnswer
                {
                    SessionId = session.SessionId,
                    Answer = "I need a bit more detail to continue.",
                    RequiresClarification = true,
                    ClarificationQuestion = plan.ClarificationQuestion,
                    ToolCalls = toolCallSummaries,
                    Trace = trace
                };
            }

            if (plan.ToolCall is null)
            {
                var answer = plan.FinalAnswer ?? await chatOrchestrator.ComposeFinalAnswerAsync(query, session, executed, cancellationToken);
                session.UpdatedAt = DateTimeOffset.UtcNow;
                await sessionStore.UpsertAsync(session, cancellationToken);
                trace.DurationMs = started.ElapsedMilliseconds;
                return new FinalAnswer
                {
                    SessionId = session.SessionId,
                    Answer = answer,
                    RequiresClarification = false,
                    ClarificationQuestion = null,
                    ToolCalls = toolCallSummaries,
                    Trace = trace
                };
            }

            var tool = allowed.FirstOrDefault(t => string.Equals(t.Id, plan.ToolCall.ToolId, StringComparison.OrdinalIgnoreCase));
            if (tool is null)
            {
                trace.Steps.Add(MakeStep("policy", "Tool denied", new Dictionary<string, object?> { ["toolId"] = plan.ToolCall.ToolId }));
                trace.DurationMs = started.ElapsedMilliseconds;
                return new FinalAnswer
                {
                    SessionId = session.SessionId,
                    Answer = $"Tool '{plan.ToolCall.ToolId}' is not allowed for this user.",
                    RequiresClarification = false,
                    Trace = trace,
                    ToolCalls = toolCallSummaries
                };
            }

            var argsJson = plan.ToolCall.Arguments.GetRawText();
            var validation = validator.Validate(tool, argsJson);
            if (!validation.IsValid)
            {
                trace.ValidationFailuresCount++;
                trace.Steps.Add(MakeStep("validation", "Tool arguments validation failed", new Dictionary<string, object?>
                {
                    ["toolId"] = tool.Id,
                    ["errors"] = validation.Errors
                }));

                return new FinalAnswer
                {
                    SessionId = session.SessionId,
                    Answer = "I couldn't run that tool because the arguments were invalid.",
                    RequiresClarification = true,
                    ClarificationQuestion = $"Please correct input for '{tool.Name}': {string.Join("; ", validation.Errors)}",
                    Trace = trace,
                    ToolCalls = toolCallSummaries
                };
            }

            var result = await toolExecutor.ExecuteAsync(tool, argsJson, cancellationToken);
            executed.Add(result);
            trace.ToolCallsCount++;
            toolCallSummaries.Add(new ToolCallSummary(tool.Id, result.Status));

            session.RecentToolResults.Add(new ToolCallResultSnapshot(tool.Id, result.Status, result.Data?.RootElement.GetRawText()));
            if (TryExtractOrderId(result.Data, out var orderId))
            {
                session.Entities["orderId"] = orderId;
            }

            trace.Steps.Add(MakeStep("tool", "Tool executed", new Dictionary<string, object?>
            {
                ["toolId"] = tool.Id,
                ["status"] = result.Status,
                ["durationMs"] = result.DurationMs
            }));

            if (!result.Success)
            {
                logger.LogWarning("Tool {ToolId} failed. CorrelationId {CorrelationId}", tool.Id, correlationId);
                break;
            }
        }

        var fallbackAnswer = await chatOrchestrator.ComposeFinalAnswerAsync(query, session, executed, cancellationToken);
        session.UpdatedAt = DateTimeOffset.UtcNow;
        await sessionStore.UpsertAsync(session, cancellationToken);
        trace.DurationMs = started.ElapsedMilliseconds;

        return new FinalAnswer
        {
            SessionId = session.SessionId,
            Answer = fallbackAnswer,
            RequiresClarification = false,
            Trace = trace,
            ToolCalls = toolCallSummaries
        };
    }

    private static ExecutionStep MakeStep(string kind, string message, Dictionary<string, object?> metadata) =>
        new(kind, message, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), metadata);

    private static bool TryExtractOrderId(JsonDocument? data, out string orderId)
    {
        orderId = string.Empty;
        if (data is null)
        {
            return false;
        }

        if (data.RootElement.TryGetProperty("id", out var idValue))
        {
            orderId = idValue.ToString();
            return !string.IsNullOrWhiteSpace(orderId);
        }

        if (data.RootElement.TryGetProperty("orderId", out var orderIdValue))
        {
            orderId = orderIdValue.ToString();
            return !string.IsNullOrWhiteSpace(orderId);
        }

        return false;
    }
}
