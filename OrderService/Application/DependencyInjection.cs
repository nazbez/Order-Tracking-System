using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Application.Core.Decorators;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        services.AddMediator(assembly);

        services.Decorate(typeof(IRequestHandler<,>), typeof(ValidationDecorator<,>));
        
        services.AddValidatorsFromAssembly(assembly);
    }
}
