using Serilog.Events;

namespace Central.Logging.Abstractions;

/// <summary>
/// Configuration options for the Extended logging system
/// </summary>
public class CentralLoggingConfiguration
{
    private const string _defaultOutputTemplate =
        "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// The minimum log level to capture
    /// </summary>
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;

    /// <summary>
    /// Custom output template for console logging
    /// </summary>
    public string OutputTemplate { get; set; } = _defaultOutputTemplate;

    /// <summary>
    /// Whether to include scopes in log messages
    /// </summary>
    public bool IncludeScopes { get; set; } = true;

    /// <summary>
    /// Application name to include in logs
    /// </summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Namespace to include in logs
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Environment name to include in logs
    /// </summary>
    public string? Environment { get; set; }

    /// <summary>
    /// Additional properties to enrich all log messages
    /// </summary>
    public Dictionary<string, object> EnrichmentProperties { get; set; } = [];
}
