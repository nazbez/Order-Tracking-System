using System.Diagnostics;
using System.Diagnostics.Metrics;
using IntelligentGateway.Api;
using IntelligentGateway.Application;
using IntelligentGateway.Domain;
using IntelligentGateway.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddIntelligentGatewayApplication();
builder.Services.AddIntelligentGatewayInfrastructure(builder.Configuration);

builder.Services.AddSingleton<IRequestIdentityResolver, HeaderRequestIdentityResolver>();
builder.Services.AddSingleton<GatewayMetrics>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers.TryGetValue("x-correlation-id", out var headerValue)
        ? headerValue.ToString()
        : Guid.NewGuid().ToString("N");

    context.Items["correlationId"] = correlationId;
    context.Response.Headers["x-correlation-id"] = correlationId;

    await next();
});

app.UseSwagger();
app.UseSwaggerUI();

app.MapHealthChecks("/health");

app.MapGet("/api/tools", (IToolRegistry registry) =>
{
    var tools = registry.GetAll().Select(t => new
    {
        t.Id,
        t.Name,
        t.Description,
        PolicyTags = t.PolicyTags,
        InputSchema = t.InputSchema.RootElement,
        t.OutputDescription,
        t.Timeout,
        t.MaxRetries
    });

    return Results.Ok(tools);
});

app.MapGet("/api/sessions/{sessionId}", async (string sessionId, ISessionStore sessionStore, CancellationToken cancellationToken) =>
{
    var session = await sessionStore.GetAsync(sessionId, cancellationToken);
    return session is null ? Results.NotFound() : Results.Ok(session);
});

app.MapPost("/api/query", async (
        QueryRequest request,
        HttpContext httpContext,
        IRequestIdentityResolver identityResolver,
        IUserQueryHandler handler,
        GatewayMetrics metrics,
        CancellationToken cancellationToken) =>
    {
        var correlationId = httpContext.Items["correlationId"]?.ToString() ?? Guid.NewGuid().ToString("N");
        var sessionId = string.IsNullOrWhiteSpace(request.SessionId) ? Guid.NewGuid().ToString("N") : request.SessionId;

        var identity = identityResolver.Resolve(httpContext, request.UserId);
        var query = new UserQuery(sessionId, identity.UserId, request.Message);

        var started = Stopwatch.StartNew();
        var response = await handler.HandleAsync(query, identity.Role, correlationId, cancellationToken);
        metrics.RecordRequest(started.ElapsedMilliseconds, response.Trace);

        return Results.Ok(response);
    })
    .WithName("HandleUserQuery");

app.Run();

public sealed record QueryRequest(string? SessionId, string? UserId, string Message);

public sealed class GatewayMetrics
{
    private readonly Histogram<long> _requestDurationMs;
    private readonly Counter<int> _toolCalls;
    private readonly Counter<int> _validationFailures;
    private readonly Counter<int> _clarifications;
    private readonly Counter<int> _sessionReads;
    private readonly Counter<int> _sessionUpdates;
    private readonly Counter<int> _terminationReasons;

    public GatewayMetrics()
    {
        var meter = new Meter("IntelligentGateway", "1.1.0");
        _requestDurationMs = meter.CreateHistogram<long>("gateway.request.duration.ms");
        _toolCalls = meter.CreateCounter<int>("gateway.tool.calls");
        _validationFailures = meter.CreateCounter<int>("gateway.validation.failures");
        _clarifications = meter.CreateCounter<int>("gateway.clarification.count");
        _sessionReads = meter.CreateCounter<int>("gateway.session.read.count");
        _sessionUpdates = meter.CreateCounter<int>("gateway.session.update.count");
        _terminationReasons = meter.CreateCounter<int>("gateway.termination.reason.count");
    }

    public void RecordRequest(long durationMs, ExecutionTrace trace)
    {
        _requestDurationMs.Record(durationMs);
        _toolCalls.Add(trace.ToolCallsCount);
        _validationFailures.Add(trace.ValidationFailuresCount);
        _clarifications.Add(trace.ClarificationCount);
        _sessionReads.Add(trace.SessionRead ? 1 : 0);
        _sessionUpdates.Add(trace.SessionUpdated ? 1 : 0);
        _terminationReasons.Add(1, new KeyValuePair<string, object?>("reason", trace.TerminationReason.ToString()));
    }
}

public partial class Program;
