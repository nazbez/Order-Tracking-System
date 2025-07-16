using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace OrderTrackingSystem.Core.Mediator;

/// <summary>
/// Provides extension methods for configuring dependency injection related to the mediator pattern.
/// </summary>
[ExcludeFromCodeCoverage]
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers mediator-related services in the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="assembly">The assembly to scan for implementations of <see cref="IRequestHandler{TRequest, TResponse}"/>.</param>
    public static void AddMediator(this IServiceCollection services, Assembly assembly)
    {
        services.Scan(t => t.FromAssemblies(assembly)
            .AddClasses(i => i.AssignableTo(typeof(IRequestHandler<,>)), true)
            .AsImplementedInterfaces()
            .WithScopedLifetime());
    }
}