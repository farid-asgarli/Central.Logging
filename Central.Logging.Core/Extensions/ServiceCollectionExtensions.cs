using Central.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Central.Logging.Core.Extensions;

/// <summary>
/// Extension methods for registering Core logging services with console output
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Core logging services to the service collection with custom configuration.
    /// Configures Serilog with console output, application name, namespace, environment,
    /// and custom enrichment properties.
    /// </summary>
    /// <param name="services">The service collection to add logging services to</param>
    /// <param name="configuration">The logging configuration containing settings for minimum log level,
    /// output template, application name, namespace, environment, and enrichment properties</param>
    /// <returns>The service collection for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configuration is null</exception>
    /// <example>
    /// <code>
    /// services.AddCentralLogging(new CentralLoggingConfiguration
    /// {
    ///     ApplicationName = "MyApp",
    ///     Environment = "Production",
    ///     MinimumLevel = LogEventLevel.Warning
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddCentralLogging(
        this IServiceCollection services,
        CentralLoggingConfiguration configuration
    )
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        var loggerConfig = new LoggingConfigurationBuilder()
            .WithAppName(configuration.ApplicationName)
            .WithNamespace(configuration.Namespace)
            .WithEnv(configuration.Environment)
            .WithEnrichmentProperties(configuration.EnrichmentProperties)
            .Configuration.MinimumLevel.Is(configuration.MinimumLevel)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: configuration.OutputTemplate,
                restrictedToMinimumLevel: configuration.MinimumLevel
            );

        LoggingServiceHelper.RegisterCommonServices(services, configuration, loggerConfig);

        return services;
    }

    /// <summary>
    /// Adds Core logging services to the service collection with default configuration.
    /// Uses default settings including Information level logging, standard console output template,
    /// and auto-detected application name.
    /// </summary>
    /// <param name="services">The service collection to add logging services to</param>
    /// <returns>The service collection for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null</exception>
    /// <remarks>
    /// This overload creates a default configuration with:
    /// <list type="bullet">
    /// <item><description>Minimum log level: Information</description></item>
    /// <item><description>Console output with standard template</description></item>
    /// <item><description>Auto-detected application name</description></item>
    /// <item><description>Default namespace and environment detection</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddCentralLogging();
    /// </code>
    /// </example>
    public static IServiceCollection AddCentralLogging(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        CentralLoggingConfiguration configuration = new();

        return services.AddCentralLogging(configuration);
    }
}
