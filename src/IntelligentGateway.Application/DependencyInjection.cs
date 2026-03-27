using Microsoft.Extensions.DependencyInjection;

namespace IntelligentGateway.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddIntelligentGatewayApplication(this IServiceCollection services)
    {
        services.AddSingleton<IUserQueryHandler, UserQueryHandler>();
        return services;
    }
}
