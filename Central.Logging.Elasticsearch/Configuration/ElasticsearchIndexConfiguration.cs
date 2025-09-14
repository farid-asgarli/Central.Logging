using Central.Logging.Elasticsearch.Utilities;

namespace Central.Logging.Elasticsearch.Configuration;

/// <summary>
/// Configuration for Elasticsearch index naming and formatting
/// </summary>
public class ElasticsearchIndexConfiguration
{
    private const string DefaultDateFormat = "yyyy.MM.dd";

    /// <summary>
    /// The namespace to include in index names
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// The application name to include in index names
    /// </summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    /// The date format to use in time-based indices
    /// </summary>
    public string DateFormat { get; set; } = DefaultDateFormat;

    /// <summary>
    /// Gets the formatted index pattern for Elasticsearch
    /// </summary>
    /// <returns>A formatted index pattern string</returns>
    public string GetFormattedIndexName()
    {
        var environment = EnvironmentConstants.CurrentEnv ?? "unknown";
        return IndexUtilities.GetFormattedIndexName(
            environment,
            DateFormat,
            Namespace,
            ApplicationName
        );
    }
}
