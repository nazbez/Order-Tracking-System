using IntelligentGateway.Application;
using IntelligentGateway.Domain;

namespace IntelligentGateway.Infrastructure;

public sealed class SimpleToolPolicyService : IToolPolicyService
{
    public IReadOnlyCollection<ToolDefinition> FilterAllowedTools(string userId, string? role, IReadOnlyCollection<ToolDefinition> tools)
    {
        var tags = GetAllowedTags(userId, role);
        return tools.Where(t => t.PolicyTags.All(tags.Contains)).ToArray();
    }

    public bool CanExecute(string userId, string? role, ToolDefinition tool)
    {
        var tags = GetAllowedTags(userId, role);
        return tool.PolicyTags.All(tags.Contains);
    }

    private static HashSet<string> GetAllowedTags(string userId, string? role)
    {
        if (string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(userId, "demo-user", StringComparison.OrdinalIgnoreCase))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "orders.read", "tracking.read", "courier.read" };
        }

        return new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "tracking.read" };
    }
}
