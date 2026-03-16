using IntelligentGateway.Domain;

namespace IntelligentGateway.Application;

public interface IUserQueryHandler
{
    Task<FinalAnswer> HandleAsync(UserQuery query, string? role, string correlationId, CancellationToken cancellationToken);
}

public interface IToolRegistry
{
    IReadOnlyCollection<ToolDefinition> GetAll();
    ToolDefinition? GetById(string toolId);
}

public interface IToolPolicyService
{
    IReadOnlyCollection<ToolDefinition> FilterAllowedTools(string userId, string? role, IReadOnlyCollection<ToolDefinition> tools);
    bool CanExecute(string userId, string? role, ToolDefinition tool);
}

public interface IToolArgumentValidator
{
    ValidationResult Validate(ToolDefinition tool, string argumentsJson);
}

public sealed record ValidationResult(bool IsValid, IReadOnlyCollection<string> Errors)
{
    public static ValidationResult Success() => new(true, []);
    public static ValidationResult Failed(params string[] errors) => new(false, errors);
}

public interface IToolExecutor
{
    Task<ToolCallResult> ExecuteAsync(ToolDefinition tool, string argumentsJson, CancellationToken cancellationToken);
}

public interface ISessionStore
{
    Task<SessionContext?> GetAsync(string sessionId, CancellationToken cancellationToken);
    Task UpsertAsync(SessionContext context, CancellationToken cancellationToken);
}

public interface IChatOrchestrator
{
    Task<ToolCallPlan> PlanAsync(
        UserQuery query,
        SessionContext session,
        IReadOnlyCollection<ToolDefinition> allowedTools,
        IReadOnlyCollection<ToolCallResult> priorResults,
        CancellationToken cancellationToken);

    Task<string> ComposeFinalAnswerAsync(
        UserQuery query,
        SessionContext session,
        IReadOnlyCollection<ToolCallResult> results,
        CancellationToken cancellationToken);
}

public interface IServiceAdapter
{
    string Key { get; }
    Task<JsonDocument> ExecuteAsync(string toolId, string argumentsJson, CancellationToken cancellationToken);
}
