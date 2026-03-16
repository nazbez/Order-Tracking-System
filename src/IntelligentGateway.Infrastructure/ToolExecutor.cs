using System.Diagnostics;
using IntelligentGateway.Application;
using IntelligentGateway.Domain;

namespace IntelligentGateway.Infrastructure;

public sealed class ToolExecutor(IEnumerable<IServiceAdapter> adapters) : IToolExecutor
{
    private readonly IReadOnlyDictionary<string, IServiceAdapter> _adapters = adapters.ToDictionary(a => a.Key, StringComparer.OrdinalIgnoreCase);

    public async Task<ToolCallResult> ExecuteAsync(ToolDefinition tool, string argumentsJson, CancellationToken cancellationToken)
    {
        if (!_adapters.TryGetValue(tool.AdapterKey, out var adapter))
        {
            return new ToolCallResult(tool.Id, false, "adapter_missing", null, $"Adapter '{tool.AdapterKey}' not found", 0);
        }

        var started = Stopwatch.StartNew();

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(tool.Timeout);

            JsonDocument? payload = null;
            Exception? last = null;

            for (var attempt = 0; attempt <= tool.MaxRetries; attempt++)
            {
                try
                {
                    payload = await adapter.ExecuteAsync(tool.Id, argumentsJson, timeoutCts.Token);
                    break;
                }
                catch (Exception ex)
                {
                    last = ex;
                    if (attempt == tool.MaxRetries)
                    {
                        throw;
                    }
                }
            }

            return new ToolCallResult(tool.Id, true, "success", payload, null, started.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            return new ToolCallResult(tool.Id, false, "failed", null, ex.Message, started.ElapsedMilliseconds);
        }
    }
}
