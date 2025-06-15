using System.Diagnostics.CodeAnalysis;
using Application.Core.Decorators;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.Scan(t => t.FromAssembliesOf(typeof(DependencyInjection))
            .AddClasses(i => i.AssignableTo(typeof(IRequestHandler<,>)), true)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.Decorate(typeof(IRequestHandler<,>), typeof(ValidationDecorator<,>));
    }
}
