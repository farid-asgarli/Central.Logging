namespace Central.Logging.Elasticsearch.Configuration;

/// <summary>
/// Configuration for Elasticsearch sink behavior
/// </summary>
public class ElasticsearchSinkConfiguration
{
    /// <summary>
    /// Whether to automatically register the index template
    /// </summary>
    public bool AutoRegisterTemplate { get; set; } = true;

    /// <summary>
    /// Number of primary shards for the index
    /// </summary>
    public int NumberOfShards { get; set; } = 1;

    /// <summary>
    /// Number of replica shards for the index
    /// </summary>
    public int NumberOfReplicas { get; set; } = 0;

    /// <summary>
    /// Buffer base filename for file-based buffering
    /// </summary>
    public TimeSpan? BufferBaseFilename { get; set; }

    /// <summary>
    /// Maximum number of events to include in a single batch
    /// </summary>
    public int BatchPostingLimit { get; set; } = 50;

    /// <summary>
    /// Time period between batch submissions
    /// </summary>
    public TimeSpan Period { get; set; } = TimeSpan.FromSeconds(2);
}