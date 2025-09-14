using Central.Logging.Abstractions;
using Central.Logging.Elasticsearch.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace Central.Logging.Elasticsearch;

/// <summary>
/// Extension methods for registering Elasticsearch logging services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Elasticsearch logging to the service collection with custom configuration
    /// </summary>
    public static IServiceCollection AddCentralLoggingElasticsearch(
        this IServiceCollection services,
        CentralLoggingConfigurationWithElastic configuration
    )
    {
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
