using Central.Logging.Abstractions;
using Central.Logging.Elasticsearch.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace Central.Logging.Elasticsearch.Extensions;

/// <summary>
/// Extension methods for registering Elasticsearch logging services with both console and Elasticsearch output
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Central logging with Elasticsearch support to the service collection.
    /// Configures Serilog with both console and Elasticsearch sinks, including authentication,
    /// index formatting, batching, and error handling.
    /// </summary>
    /// <param name="services">The service collection to add logging services to</param>
    /// <param name="configuration">The Elasticsearch logging configuration containing base logging settings
    /// and Elasticsearch-specific configuration including URL, authentication, index settings, and sink options</param>
    /// <returns>The service collection for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configuration is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when Elasticsearch configuration is missing or URL is not specified</exception>
    /// <remarks>
    /// This method configures:
    /// <list type="bullet">
    /// <item><description>Console logging with the specified output template and minimum level</description></item>
    /// <item><description>Elasticsearch sink with automatic index creation and template registration</description></item>
    /// <item><description>Authentication support for both basic auth and API key</description></item>
    /// <item><description>Batching configuration for optimal performance</description></item>
    /// <item><description>Error handling and failure callbacks</description></item>
    /// <item><description>Index naming with environment, namespace, and application name</description></item>
    /// </list>
    /// The method automatically inherits application name and namespace from base configuration if not specified in Elasticsearch config.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddCentralLoggingElasticsearch(new CentralLoggingConfigurationWithElastic
    /// {
    ///     ApplicationName = "MyApp",
    ///     Environment = "Production",
    ///     MinimumLevel = LogEventLevel.Information,
    ///     Elasticsearch = new ElasticsearchConfiguration
    ///     {
    ///         Url = "https://elasticsearch.example.com",
    ///         Authentication = new ElasticsearchAuthenticationConfiguration
    ///         {
    ///             ApiKey = "your-api-key"
    ///         },
    ///         Index = new ElasticsearchIndexConfiguration
    ///         {
    ///             ApplicationName = "myapp"
    ///         }
    ///     }
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddCentralLoggingElasticsearch(
        this IServiceCollection services,
        CentralLoggingConfigurationWithElastic configuration
    )
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        var loggerConfig = new LoggingConfigurationBuilder() // Build the logger
            .WithAppName(
                configuration.Elasticsearch?.Index.ApplicationName ?? configuration.ApplicationName
            )
            .WithNamespace(configuration.Elasticsearch?.Index.Namespace ?? configuration.Namespace)
            .WithEnv(configuration.Environment)
            .WithEnrichmentProperties(configuration.EnrichmentProperties)
            .Configuration.MinimumLevel.Is(configuration.MinimumLevel)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: configuration.OutputTemplate,
                restrictedToMinimumLevel: configuration.MinimumLevel
            );

        var elasticConfig =
            configuration.Elasticsearch
            ?? throw new InvalidOperationException(
                "Elasticsearch configuration is required but not set."
            );

        if (string.IsNullOrEmpty(elasticConfig.Url))
            throw new InvalidOperationException("Elasticsearch URL must be specified.");

        if (
            string.IsNullOrEmpty(elasticConfig.Index.ApplicationName)
            && !string.IsNullOrEmpty(configuration.ApplicationName)
        )
            elasticConfig.Index.ApplicationName = configuration.ApplicationName;

        ElasticsearchSinkOptions sinkOptions = new([new Uri(elasticConfig.Url)])
        {
            IndexFormat = elasticConfig.GetIndex(),
            AutoRegisterTemplate = elasticConfig.Sink.AutoRegisterTemplate,
            NumberOfShards = elasticConfig.Sink.NumberOfShards,
            NumberOfReplicas = elasticConfig.Sink.NumberOfReplicas,
            BatchPostingLimit = elasticConfig.Sink.BatchPostingLimit,
            Period = elasticConfig.Sink.Period,
            FailureCallback = (e, ex) =>
                Console.WriteLine($"Unable to submit event {e.MessageTemplate}: {ex?.Message}"),
            EmitEventFailure =
                EmitEventFailureHandling.WriteToSelfLog | EmitEventFailureHandling.RaiseCallback,
        };

        // Auth
        if (elasticConfig.Authentication.HasBasicAuth)
            sinkOptions.ModifyConnectionSettings = conn =>
                conn.BasicAuthentication(
                    elasticConfig.Authentication.Username!,
                    elasticConfig.Authentication.Password!
                );
        else if (elasticConfig.Authentication.HasApiKey)
            sinkOptions.ModifyConnectionSettings = conn =>
                conn.ApiKeyAuthentication(new(elasticConfig.Authentication.ApiKey!));

        loggerConfig.WriteTo.Elasticsearch(sinkOptions);

        LoggingServiceHelper.RegisterCommonServices(services, configuration, loggerConfig);

        return services;
    }
}
