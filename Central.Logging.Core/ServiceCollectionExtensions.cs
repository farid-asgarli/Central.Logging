using Central.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Central.Logging.Core;

/// <summary>
/// Extension methods for registering Elasticsearch logging services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Elasticsearch logging to the service collection with custom configuration
    /// </summary>
    public static IServiceCollection AddCentralLogging(
        this IServiceCollection services,
        CentralLoggingConfiguration configuration
    )
    {
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
    /// Adds Elasticsearch logging to the service collection with default configuration
    /// </summary>
    public static IServiceCollection AddCentralLogging(this IServiceCollection services)
    {
        CentralLoggingConfiguration configuration = new();

        return services.AddCentralLogging(configuration);
    }
}
