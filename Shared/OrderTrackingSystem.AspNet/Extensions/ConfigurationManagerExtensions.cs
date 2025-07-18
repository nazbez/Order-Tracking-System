using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace OrderTrackingSystem.AspNet.Extensions;

/// <summary>
/// Provides extension methods for configuring an <see cref="IConfigurationManager"/> instance.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ConfigurationManagerExtensions
{
    /// <summary>
    /// Adds configuration sources to the <see cref="IConfigurationManager"/> instance.
    /// </summary>
    /// <param name="configurationManager">The <see cref="IConfigurationManager"/> to configure.</param>
    /// <param name="environment">The hosting environment, used to determine environment-specific configuration files.</param>
    /// <returns>The configured <see cref="IConfigurationManager"/> instance.</returns>
    public static IConfigurationManager AddConfiguration(this IConfigurationManager configurationManager, IWebHostEnvironment environment)
    {
        configurationManager
            .AddJsonFile("Configurations/appsettings.json")
            .AddEnvironmentVariables();

        if (File.Exists($"Configurations/appsettings.{environment.EnvironmentName}.json"))
        {
            configurationManager
                .AddJsonFile($"Configurations/appsettings.{environment.EnvironmentName}.json", optional: false, reloadOnChange: true);
        }

        return configurationManager;
    }
}
