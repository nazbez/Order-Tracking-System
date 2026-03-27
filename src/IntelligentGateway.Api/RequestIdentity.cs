namespace IntelligentGateway.Api;

public sealed record RequestIdentity(string UserId, string Role);

public interface IRequestIdentityResolver
{
    RequestIdentity Resolve(HttpContext httpContext, string? fallbackUserId);
}

public sealed class HeaderRequestIdentityResolver : IRequestIdentityResolver
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase) { "admin", "user" };

    public RequestIdentity Resolve(HttpContext httpContext, string? fallbackUserId)
    {
        var resolvedUserId = httpContext.Request.Headers.TryGetValue("x-user-id", out var userIdHeader)
            && !string.IsNullOrWhiteSpace(userIdHeader)
                ? userIdHeader.ToString()
                : (string.IsNullOrWhiteSpace(fallbackUserId) ? "anonymous" : fallbackUserId);

        var requestedRole = httpContext.Request.Headers.TryGetValue("x-user-role", out var roleHeader)
            ? roleHeader.ToString()
            : "user";

        var resolvedRole = AllowedRoles.Contains(requestedRole) ? requestedRole.ToLowerInvariant() : "user";

        return new RequestIdentity(resolvedUserId, resolvedRole);
    }
}
