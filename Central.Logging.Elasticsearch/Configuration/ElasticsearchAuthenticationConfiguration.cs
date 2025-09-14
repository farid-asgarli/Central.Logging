namespace Central.Logging.Elasticsearch.Configuration;

/// <summary>
/// Configuration for Elasticsearch authentication
/// </summary>
public class ElasticsearchAuthenticationConfiguration
{
    /// <summary>
    /// Username for basic authentication
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Password for basic authentication
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// API key for authentication
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Whether basic authentication credentials are provided
    /// </summary>
    public bool HasBasicAuth => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);

    /// <summary>
    /// Whether an API key is provided
    /// </summary>
    public bool HasApiKey => !string.IsNullOrEmpty(ApiKey);
}