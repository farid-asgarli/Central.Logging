namespace Central.Logging.Elasticsearch.Configuration;

/// <summary>
/// Main configuration class for Elasticsearch logging
/// </summary>
public class ElasticsearchConfiguration
{
    /// <summary>
    /// The Elasticsearch URL to connect to
    /// </summary>
    public string Url { get; set; } = GetUrl();

    /// <summary>
    /// Authentication configuration
    /// </summary>
    public ElasticsearchAuthenticationConfiguration Authentication { get; set; } = new();

    /// <summary>
    /// Index configuration
    /// </summary>
    public ElasticsearchIndexConfiguration Index { get; set; } = new();

    /// <summary>
    /// Sink configuration
    /// </summary>
    public ElasticsearchSinkConfiguration Sink { get; set; } = new();

    /// <summary>
    /// Gets the formatted index name
    /// </summary>
    /// <returns>The formatted index pattern</returns>
    public string GetIndex() => Index.GetFormattedIndexName();

    /// <summary>
    /// Gets the appropriate Elasticsearch URL based on the current environment
    /// </summary>
    /// <returns>The Elasticsearch URL</returns>
    public static string GetUrl()
    {
        bool hasUrl = urlMappings.TryGetValue(EnvironmentConstants.CurrentEnv, out string? elasticUrl);
        return hasUrl ? elasticUrl! : urlMappings[EnvironmentConstants.Alpha]!;
    }

    private static readonly IDictionary<string, string> urlMappings = new Dictionary<string, string>
    {
        { EnvironmentConstants.Alpha, "https://elasti-dev.pasha-life.az/" },
        { EnvironmentConstants.Beta, "https://elasti-dev.pasha-life.az/" },
        { EnvironmentConstants.Development, "https://elasti-dev.pasha-life.az/" },
        { EnvironmentConstants.Production, "https://elasti-prod.pasha-life.az/" },
    };
}
