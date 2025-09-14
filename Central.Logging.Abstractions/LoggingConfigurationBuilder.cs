using Serilog;

namespace Central.Logging.Abstractions;

public class LoggingConfigurationBuilder
{
    private readonly LoggerConfiguration loggerConfig = new();

    public LoggerConfiguration Configuration
    {
        get => loggerConfig;
    }

    public LoggingConfigurationBuilder WithNamespace(string? ns)
    {
        if (!string.IsNullOrEmpty(ns))
            loggerConfig.Enrich.WithProperty("Namespace", ns);
        return this;
    }

    public LoggingConfigurationBuilder WithEnv(string? env)
    {
        if (!string.IsNullOrEmpty(env))
            loggerConfig.Enrich.WithProperty(nameof(Environment), env);
        return this;
    }

    public LoggingConfigurationBuilder WithAppName(string? appName)
    {
        if (!string.IsNullOrEmpty(appName))
            loggerConfig.Enrich.WithProperty("Application", appName);
        return this;
    }

    public LoggingConfigurationBuilder WithEnrichmentProperties(
        Dictionary<string, object> enrichmentProperties
    )
    {
        foreach (var prop in enrichmentProperties)
            loggerConfig.Enrich.WithProperty(prop.Key, prop.Value);

        return this;
    }
}
