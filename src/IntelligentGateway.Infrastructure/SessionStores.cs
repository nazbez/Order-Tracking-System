using System.Text.Json;
using IntelligentGateway.Application;
using IntelligentGateway.Domain;
using StackExchange.Redis;

namespace IntelligentGateway.Infrastructure;

public sealed class InMemorySessionStore : ISessionStore
{
    private readonly Dictionary<string, SessionContext> _sessions = new(StringComparer.OrdinalIgnoreCase);

    public Task<SessionContext?> GetAsync(string sessionId, CancellationToken cancellationToken)
    {
        _sessions.TryGetValue(sessionId, out var value);
        return Task.FromResult(value);
    }

    public Task UpsertAsync(SessionContext context, CancellationToken cancellationToken)
    {
        _sessions[context.SessionId] = context;
        return Task.CompletedTask;
    }
}

public sealed class RedisSessionStore(IConnectionMultiplexer multiplexer) : ISessionStore
{
    private readonly IDatabase _database = multiplexer.GetDatabase();

    public async Task<SessionContext?> GetAsync(string sessionId, CancellationToken cancellationToken)
    {
        var value = await _database.StringGetAsync(GetKey(sessionId));
        if (!value.HasValue)
        {
            return null;
        }

        return JsonSerializer.Deserialize<SessionContext>(value!);
    }

    public async Task UpsertAsync(SessionContext context, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(context);
        await _database.StringSetAsync(GetKey(context.SessionId), payload, TimeSpan.FromHours(6));
    }

    private static string GetKey(string sessionId) => $"igw:session:{sessionId}";
}
