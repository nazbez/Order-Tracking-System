using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using OrderTrackingSystem.Logging.Enrichers;
using Serilog;

namespace OrderTrackingSystem.AspNet.Extensions;

/// <summary>
/// Provides extension methods for configuring a <see cref="IHostBuilder"/> with Serilog logging.
/// </summary>
[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    /// <summary>
    /// Configures the <see cref="IHostBuilder"/> to use Serilog for logging.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to configure.</param>
    /// <returns>The configured <see cref="IHostBuilder"/>.</returns>
    public static IHostBuilder AddSerilog(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog((context, configuration) =>
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.With<ActivityEnricher>());

        return hostBuilder;
    }
}
