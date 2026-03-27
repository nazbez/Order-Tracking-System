using System.Text.Json;

namespace IntelligentGateway.Domain;

public enum TerminationReason
{
    Success,
    ClarificationNeeded,
    Forbidden,
    InvalidArguments,
    UnsupportedRequest,
    MaxStepsReached,
    DegradedFallback
}

public sealed record UserQuery(string SessionId, string UserId, string Message);

public sealed record ToolDefinition(
    string Id,
    string Name,
    string Description,
    IReadOnlyCollection<string> PolicyTags,
    JsonDocument InputSchema,
    string OutputDescription,
    string AdapterKey,
    TimeSpan Timeout,
    int MaxRetries);

public sealed record ToolCallRequest(string ToolId, JsonElement Arguments);

public sealed record ToolCallPlan(
    bool RequiresClarification,
    string? ClarificationQuestion,
    string? FinalAnswer,
    ToolCallRequest? ToolCall,
    bool UsedSessionMemory = false,
    string Scenario = "unknown");

public sealed record ToolCallResult(string ToolId, bool Success, string Status, JsonDocument? Data, string? Error, long DurationMs);

public sealed record ClarificationRequest(string Question);

public sealed record ExecutionStep(string Kind, string Message, long TimestampUnixMs, Dictionary<string, object?> Metadata);

public sealed class ExecutionTrace
{
    public long DurationMs { get; set; }
    public int OrchestrationStepsCount { get; set; }
    public int ToolCallsCount { get; set; }
    public int ValidationFailuresCount { get; set; }
    public int ClarificationCount { get; set; }
    public bool SessionRead { get; set; }
    public bool SessionUpdated { get; set; }
    public bool UsedSessionMemory { get; set; }
    public TerminationReason TerminationReason { get; set; } = TerminationReason.Success;
    public List<ExecutionStep> Steps { get; } = [];
}

public sealed class SessionContext
{
    public required string SessionId { get; init; }
    public required string UserId { get; init; }
    public string? ResolvedOrderId { get; set; }
    public List<ToolCallResultSnapshot> RecentToolResults { get; init; } = [];
    public Dictionary<string, string> Entities { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed record ToolCallResultSnapshot(string ToolId, string Status, string? DataJson);

public sealed class FinalAnswer
{
    public required string SessionId { get; init; }
    public required string Answer { get; init; }
    public bool RequiresClarification { get; init; }
    public string? ClarificationQuestion { get; init; }
    public List<ToolCallSummary> ToolCalls { get; init; } = [];
    public required ExecutionTrace Trace { get; init; }
}

public sealed record ToolCallSummary(string ToolId, string Status);
