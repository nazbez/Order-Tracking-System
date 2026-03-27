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
    private const int MaxSessionToolSnapshots = 5;

    public async Task<FinalAnswer> HandleAsync(UserQuery query, string? role, string correlationId, CancellationToken cancellationToken)
    {
        var started = Stopwatch.StartNew();
        var trace = new ExecutionTrace();
        var toolCallSummaries = new List<ToolCallSummary>();

        var existingSession = await sessionStore.GetAsync(query.SessionId, cancellationToken);
        trace.SessionRead = existingSession is not null;

        var session = existingSession ?? new SessionContext { SessionId = query.SessionId, UserId = query.UserId };

        var allowed = toolPolicyService.FilterAllowedTools(query.UserId, role, toolRegistry.GetAll());

        trace.Steps.Add(MakeStep("policy", "Filtered allowed tools", new Dictionary<string, object?>
        {
            ["allowedToolCount"] = allowed.Count,
            ["correlationId"] = correlationId,
            ["role"] = role ?? "user"
        }));

        var executed = new List<ToolCallResult>();
        var toolFailed = false;

        for (var i = 0; i < MaxToolIterations; i++)
        {
            trace.OrchestrationStepsCount++;
            var plan = await chatOrchestrator.PlanAsync(query, session, allowed, executed, cancellationToken);

            if (plan.UsedSessionMemory)
            {
                trace.UsedSessionMemory = true;
                trace.Steps.Add(MakeStep("memory", "Planner reused session memory", new Dictionary<string, object?>
                {
                    ["resolvedOrderId"] = session.ResolvedOrderId
                }));
            }

            if (plan.RequiresClarification)
            {
                trace.ClarificationCount++;
                trace.TerminationReason = TerminationReason.ClarificationNeeded;
                trace.Steps.Add(MakeStep("clarification", "Clarification requested", new Dictionary<string, object?>
                {
                    ["question"] = plan.ClarificationQuestion,
                    ["scenario"] = plan.Scenario
                }));

                await PersistSessionAsync(session, trace, cancellationToken);

                return BuildAnswer(started.ElapsedMilliseconds, session.SessionId,
                    "I need a bit more detail to continue.",
                    requiresClarification: true,
                    plan.ClarificationQuestion,
                    toolCallSummaries,
                    trace);
            }

            if (plan.ToolCall is null)
            {
                var answer = plan.FinalAnswer ?? await chatOrchestrator.ComposeFinalAnswerAsync(query, session, executed, cancellationToken);
                trace.TerminationReason = string.Equals(plan.Scenario, "unsupported", StringComparison.OrdinalIgnoreCase)
                    ? TerminationReason.Unsupported
                    : TerminationReason.Success;

                await PersistSessionAsync(session, trace, cancellationToken);
                return BuildAnswer(started.ElapsedMilliseconds, session.SessionId, answer, false, null, toolCallSummaries, trace);
            }

            var tool = allowed.FirstOrDefault(t => string.Equals(t.Id, plan.ToolCall.ToolId, StringComparison.OrdinalIgnoreCase));
            if (tool is null)
            {
                trace.TerminationReason = TerminationReason.Forbidden;
                trace.Steps.Add(MakeStep("policy", "Tool denied", new Dictionary<string, object?> { ["toolId"] = plan.ToolCall.ToolId }));

                await PersistSessionAsync(session, trace, cancellationToken);
                return BuildAnswer(started.ElapsedMilliseconds, session.SessionId,
                    $"Tool '{plan.ToolCall.ToolId}' is not allowed for this user.", false, null, toolCallSummaries, trace);
            }

            var argsJson = plan.ToolCall.Arguments.GetRawText();
            var validation = validator.Validate(tool, argsJson);
            if (!validation.IsValid)
            {
                trace.ValidationFailuresCount++;
                trace.TerminationReason = TerminationReason.InvalidArguments;
                trace.Steps.Add(MakeStep("validation", "Tool arguments validation failed", new Dictionary<string, object?>
                {
                    ["toolId"] = tool.Id,
                    ["errors"] = validation.Errors
                }));

                await PersistSessionAsync(session, trace, cancellationToken);
                return BuildAnswer(started.ElapsedMilliseconds, session.SessionId,
                    "I couldn't run that tool because the arguments were invalid.",
                    true,
                    $"Please correct input for '{tool.Name}': {string.Join("; ", validation.Errors)}",
                    toolCallSummaries,
                    trace);
            }

            var result = await toolExecutor.ExecuteAsync(tool, argsJson, cancellationToken);
            executed.Add(result);
            trace.ToolCallsCount++;
            toolCallSummaries.Add(new ToolCallSummary(tool.Id, result.Status));

            AddToolSnapshot(session, result);
            if (TryExtractOrderId(result.Data, out var orderId))
            {
                session.ResolvedOrderId = orderId;
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
                toolFailed = true;
                logger.LogWarning("Tool {ToolId} failed. CorrelationId {CorrelationId}", tool.Id, correlationId);
                break;
            }
        }

        var fallbackAnswer = await chatOrchestrator.ComposeFinalAnswerAsync(query, session, executed, cancellationToken);
        trace.TerminationReason = toolFailed ? TerminationReason.DegradedFallback : TerminationReason.MaxStepsReached;

        await PersistSessionAsync(session, trace, cancellationToken);
        return BuildAnswer(started.ElapsedMilliseconds, session.SessionId, fallbackAnswer, false, null, toolCallSummaries, trace);
    }

    private static void AddToolSnapshot(SessionContext session, ToolCallResult result)
    {
        session.RecentToolResults.Add(new ToolCallResultSnapshot(result.ToolId, result.Status, result.Data?.RootElement.GetRawText()));
        if (session.RecentToolResults.Count > MaxSessionToolSnapshots)
        {
            session.RecentToolResults.RemoveAt(0);
        }
    }

    private async Task PersistSessionAsync(SessionContext session, ExecutionTrace trace, CancellationToken cancellationToken)
    {
        session.UpdatedAt = DateTimeOffset.UtcNow;
        await sessionStore.UpsertAsync(session, cancellationToken);
        trace.SessionUpdated = true;
        trace.Steps.Add(MakeStep("session", "Session updated", new Dictionary<string, object?>
        {
            ["resolvedOrderId"] = session.ResolvedOrderId,
            ["toolSnapshotCount"] = session.RecentToolResults.Count
        }));
    }

    private static FinalAnswer BuildAnswer(long durationMs, string sessionId, string answer, bool requiresClarification,
        string? clarificationQuestion, List<ToolCallSummary> toolCalls, ExecutionTrace trace)
    {
        trace.DurationMs = durationMs;
        return new FinalAnswer
        {
            SessionId = sessionId,
            Answer = answer,
            RequiresClarification = requiresClarification,
            ClarificationQuestion = clarificationQuestion,
            ToolCalls = toolCalls,
            Trace = trace
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
